using System.Drawing.Imaging;

namespace Console_based
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float _x, float _y, float _z) {
            x = _x;
            y = _y;
            z = _z;
        }
        public void Normalize()
        {
            float magnitude = GetMagnitude();
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }

        public void Normalize(float magnitude)
        {
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }

        public float GetMagnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
        static public float GetDotProduct(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
    };

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public Vector4(float _x, float _y, float _z, float _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
    }

    public struct Matrix44
    {
        public float[,] elements;
        public Matrix44()
        {
            elements = new float[4, 4];
        }
    };

    //TO:DO
    //update when doing camera rotation, adding viewspace and fixing winding check
    static class Renderer
    {
        static Matrix44 _projMatrix;
        static int _width;
        static int _height;
        static Bitmap _bmp;
       
        static float[] _zBuffer;
        static Rectangle _rect;
        static float _toScreenSpaceXMod;
        static float _toScreenSpaceYMod;

        static List<FlatMesh> _meshes;
        static List<FlatMesh> _flatMeshes;
        static Camera _camera;
        static LightManager _lightManager;

        static Vertex[] _cachedVertexBuffer = new Vertex[2000];
        static int[] _cachedIndexBuffer = new int[2000];
        static int _cachedTotalIndices = 0;
        static bool[] _cacheBoolsInBox = new bool[2000];
        static int _cacheTotalPoints = 0;

        static Vector3[] _texturePng;
        static int _textureWidth;
        static int _textureHeigth;

        static public void setDrawRules(int width,int height)
        {
            _meshes = new List<FlatMesh>();
            _flatMeshes = new List<FlatMesh>();
            _width = width;
            _height = height;
            _toScreenSpaceXMod = (float)0.5 * width;
            _toScreenSpaceYMod = (float)0.5 * height;

            _rect = new Rectangle(0, 0,_width, _height);
            _zBuffer = new float[_width*_height];
            Array.Fill(_zBuffer, 1);
        }
        static public void setCamera(Camera camera)
        {
            _camera = camera;
        }

        static public void setTexture(string path)
        {
            Bitmap bmp = new Bitmap(path);
            _textureWidth = bmp.Width;
            _textureHeigth = bmp.Height;

            _texturePng = new Vector3[_textureWidth * (_textureHeigth+1)];

            int i = 0;
            for(int y=0; y < _textureHeigth; y++)
            {
                for (int x = 0; x < _textureHeigth; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    int pos = x + y * _textureWidth;
                    _texturePng[pos].x = c.R;
                    _texturePng[pos].y = c.G;
                    _texturePng[pos].z = c.B;
                }
            }
        }

        static public unsafe void DrawTexture(byte* buffer, int stride)
        {
            for(int y = 0; y < _textureHeigth; y++)
            {
                for(int x = 0; x < _textureWidth; x++)
                {
                    SetPixel(buffer, stride, x, y, _texturePng[x + y * _textureWidth]);
                }
            }
        }

        static public void setLightManager(LightManager lightManager)
        {
            _lightManager = lightManager;
        }

        static public void AddMesh(FlatMesh mesh)
        {
            _meshes.Add(mesh);
        }
        
        static public void AddFlatMesh(FlatMesh mesh)
        {
            _flatMeshes.Add(mesh);
        }

        static private void ProjectPoints(ref Vertex[] projectedPs, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 point = projectedPs[i].WorldPoint;
                float x = _projMatrix.elements[0, 0] * (point.x - _camera.CameraX);
                float y = _projMatrix.elements[1, 1] * (point.y - _camera.CameraY);
                
                float z = _projMatrix.elements[2, 2] * (point.z - _camera.CameraZ) + _projMatrix.elements[3, 2];
                float w = _projMatrix.elements[2, 3] * (point.z - _camera.CameraZ);
                projectedPs[i].SetHomougnesPoint(x, y, z, w);
            };
            _cacheTotalPoints = count;
        }

        static public void SetProjectionMatrix(float far, float near, float fov)
        {
            //[S,0,0,0]
            //[0,S,0,0]
            //[0,0,-f/(f-n),-1]
            //[0,0,-f*n/(f-n),0]

            //[1,0,0,0]
            //[0,1,0,0]
            //[0,0,1,0]
            //[-x,-y,-z,1]
            _projMatrix = new Matrix44();
            float S = (float)(1 / Math.Tan((fov / 2) * (Math.PI / 180)));

            _projMatrix.elements[0, 0] = S;
            _projMatrix.elements[1, 1] = S;
            _projMatrix.elements[2, 2] = -far / (far - near);
            _projMatrix.elements[2, 3] = -1;
            _projMatrix.elements[3, 2] = (-far * near) / (far - near);
        }

        static private unsafe void SetPixel(byte* buffer, int stride, int x, int y, Vector3 color)
        {
            byte* pixel = buffer + (stride * y) + (x * 4);
            pixel[0] = (byte)(color.z);
            pixel[1] = (byte)(color.y);
            pixel[2] = (byte)(color.x);
            pixel[3] = 255;
        }

        static public void Bilinear(float x, float y, ref Vector3 target)
        {
            int x0= (int)x;
            float x1Weight = x - x0;
            float x0Weight = 1-x1Weight;

            int y0 = (int)y;
            float y1Weight = y - y0;
            float y0Weight = 1 - y1Weight;

            int pos = x0 + y0 * _textureWidth;

            Vector3[] colors = [_texturePng[pos], _texturePng[pos+1], _texturePng[pos+_textureWidth] , _texturePng[pos + _textureWidth + 1]];
            Vector3 topPoint = new Vector3(colors[0].x * x0Weight + colors[1].x * x1Weight, colors[0].y * x0Weight + colors[1].y * x1Weight, colors[0].z * x0Weight + colors[1].z * x1Weight);
            Vector3 bottomPoint = new Vector3(colors[2].x * x0Weight + colors[3].x * x1Weight, colors[2].y * x0Weight + colors[3].y * x1Weight, colors[2].z * x0Weight + colors[3].z * x1Weight);

            target.x = topPoint.x * y0Weight + bottomPoint.x * y1Weight;
            target.y = topPoint.y * y0Weight + bottomPoint.y * y1Weight;
            target.z = topPoint.z * y0Weight + bottomPoint.z * y1Weight;
            //int y0 = (int)x;
            //int y1 = x0 + 1;
            //float dify = y - y0;
            //target.x  = (_texturePng[pos].x + _texturePng[pos+1].x + _texturePng[pos + _textureWidth].x + _texturePng[pos + _textureWidth + 1].x) / 4;
            //target.y = (_texturePng[pos].y + _texturePng[pos + 1].y + _texturePng[pos + _textureWidth].y + _texturePng[pos + _textureWidth + 1].y) / 4;
            //target.z = (_texturePng[pos].z + _texturePng[pos + 1].z + _texturePng[pos + _textureWidth].z + _texturePng[pos + _textureWidth + 1].z) / 4;
        }

        //to:do map appropriately to ensure weights dont exceed 1.
        static public unsafe void FillTriangle(byte* buffer, int stride, Vertex v1, Vertex v2, Vertex v3)
        {
            Vector3 p1 = v1.ScreenSpacePoint;
            Vector3 p2 = v2.ScreenSpacePoint;
            Vector3 p3 = v3.ScreenSpacePoint;

            float area = EdgeFunction(p1, p2, p3);

            int x0 = (int)Math.Min(p1.x, p2.x);
            x0 = (int)Math.Min(x0, p3.x);

            int x1 = (int)Math.Max(p1.x, p2.x);
            x1 = (int)Math.Max(x1, p3.x);

            int y0 = (int)Math.Min(p1.y, p2.y);
            y0 = (int)Math.Min(y0, p3.y);

            int y1 = (int)Math.Max(p1.y, p2.y);
            y1 = (int)Math.Max(y1, p3.y);

            float dW1 = (p3.y - p2.y) / area;
            float dW2 = (p1.y - p3.y) / area;
            float dW3 = (p2.y - p1.y) / area;

            Vertex temp = new Vertex();
            Vector3 color = new Vector3(255,255,255);

            for (int y = y0; y <= y1; y++)
            {
                float w1 = EdgeFunction(p2, p3, x0, y) / area;
                float w2 = EdgeFunction(p3, p1, x0, y) / area;
                float w3 = EdgeFunction(p1, p2, x0, y) / area;
                for (int x = x0; x <= x1; x++)
                {
                    if (w1 >= 0 && w2 >= 0 && w3 >= 0)
                    {
                        float z = p1.z * w1 + p2.z * w2 + p3.z * w3;
                        if (_zBuffer[y * _width + x] == z)
                        {
                            Vertex.BaryCentrePoint(v1, v2, v3, ref temp, w1, w2, w3);
                            float textx = temp.U * _textureWidth / temp.InverseW;
                            float texty = temp.V * _textureHeigth / temp.InverseW;
                            //Bilinear(textx, texty,ref color);

                            color.x = temp.Color.x;
                            color.y = temp.Color.y;
                            color.z = temp.Color.z;

                            _lightManager.GetColorWithLighting(temp,ref color);
                            SetPixel(buffer, stride, x, y, color);
                        }
                    }
                    w1 += dW1;
                    w2 += dW2;
                    w3 += dW3;
                }
            }
        }

        //to:do map appropriately to ensure weights dont exceed 1.
        static public unsafe void FillZBuffer(Vertex v1, Vertex v2, Vertex v3)
        {
            Vector3 p1 = v1.ScreenSpacePoint;
            Vector3 p2 = v2.ScreenSpacePoint;
            Vector3 p3 = v3.ScreenSpacePoint;

            float area = EdgeFunction(p1, p2, p3);

            int x0 = (int)Math.Min(p1.x, p2.x);
            x0 = (int)Math.Min(x0, p3.x);

            int x1 = (int)Math.Max(p1.x, p2.x);
            x1 = (int)Math.Max(x1, p3.x);

            int y0 = (int)Math.Min(p1.y, p2.y);
            y0 = (int)Math.Min(y0, p3.y);

            int y1 = (int)Math.Max(p1.y, p2.y);
            y1 = (int)Math.Max(y1, p3.y);

            float dxW1 = (p3.y - p2.y) /area ;
            float dxW2 = (p1.y - p3.y) / area;
            float dxW3 = (p2.y - p1.y) / area;

            for (int y = y0; y <= y1; y++)
            {
                float w1 = EdgeFunction(p2, p3, x0, y) / area;
                float w2 = EdgeFunction(p3, p1, x0, y) / area;
                float w3 = EdgeFunction(p1, p2, x0, y) / area;
                for (int x = x0; x <= x1; x++)
                {
                    if (w1 >= 0 && w2 >= 0 && w3 >= 0)
                    {
                        float z = (p1.z * w1 + p2.z * w2 + p3.z * w3);
                        int zPos = y * _width + x;
                        if (_zBuffer[zPos] > z)
                        {
                            _zBuffer[zPos] = z;
                        }
                    }
                    w1 += dxW1;
                    w2 += dxW2;
                    w3 += dxW3;
                }
            }
        }


        static public float EdgeFunction(Vector3 p1, Vector3 p2, float x, float y)
        {
            return (x - p1.x) * (p2.y - p1.y) - (y - p1.y) * (p2.x - p1.x);
        }

        static public float EdgeFunction(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x);
        }

        static private unsafe void getScreenSpaceFlatMesh(ref Vertex[] vertexBuffer, ref int count)
        {
            int len = count;
            count = 0;
            for (int i = 0; i < len; i += 3)
            {
                if (vertexBuffer[i].InFrustrum() && vertexBuffer[i + 1].InFrustrum() && vertexBuffer[i + 2].InFrustrum())
                {
                    //update when doing camera rotation, adding viewspace
                    if (vertexBuffer[i].Normal.z > 0)
                    {
                        vertexBuffer[count] = vertexBuffer[i];
                        vertexBuffer[count + 1] = vertexBuffer[i + 1];
                        vertexBuffer[count + 2] = vertexBuffer[i + 2];

                        vertexBuffer[count].SetToScreenSpace(_toScreenSpaceXMod, _toScreenSpaceYMod);
                        vertexBuffer[count + 1].SetToScreenSpace(_toScreenSpaceXMod, _toScreenSpaceYMod);
                        vertexBuffer[count + 2].SetToScreenSpace(_toScreenSpaceXMod, _toScreenSpaceYMod);
                        count += 3;
                    }
                }
            }
        }

        static private unsafe void ClearScreen(byte* buffer, int stride)
        {
            uint length = (uint)(stride*_height);
            System.Runtime.CompilerServices.Unsafe.InitBlock(buffer, 0, length);
            new Span<float>(_zBuffer).Fill(1);
        }

        static public void DrawToScreenSpace(Bitmap _bmp)
        {
            BitmapData bmpData = _bmp.LockBits(_rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;

            unsafe {
                byte* pixelBuffer = (byte*)ptr;
                ClearScreen(pixelBuffer, stride);

                foreach (FlatMesh mesh in _flatMeshes)
                {
                    _cacheTotalPoints = 0;
                    
                    mesh.CopyDataToVertexBuffer(ref _cachedVertexBuffer,ref _cacheTotalPoints);
                    
                    ProjectPoints(ref _cachedVertexBuffer, _cacheTotalPoints);
                    getScreenSpaceFlatMesh(ref _cachedVertexBuffer, ref _cacheTotalPoints);

                    
                    for (int i = 0; i < _cacheTotalPoints; i += 3)
                    {
                        FillZBuffer(_cachedVertexBuffer[i], _cachedVertexBuffer[i+1], _cachedVertexBuffer[i+2]);
                    }

                    for (int i = 0; i < _cacheTotalPoints; i += 3)
                    {
                        FillTriangle(pixelBuffer, stride, _cachedVertexBuffer[i], _cachedVertexBuffer[i + 1], _cachedVertexBuffer[i + 2]);
                    };
                }
            }
            _bmp.UnlockBits(bmpData);
        }
    };
}
