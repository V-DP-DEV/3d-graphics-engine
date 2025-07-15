using System.Drawing;
using System;
using System.Drawing.Imaging;
using static System.Windows.Forms.DataFormats;
using System.Diagnostics;

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

        static List<Mesh> _meshes;
        static List<FlatMesh> _flatMeshes;
        static Camera _camera;
        static LightManager _lightManager;

        static Vertex[] _cachedVertexBuffer = new Vertex[2000];
        static int[] _cachedIndexBuffer = new int[2000];
        static int _cachedTotalIndices = 0;
        static bool[] _cacheBoolsInBox = new bool[2000];
        static int _cacheTotalPoints = 0;

        static public void setDrawRules(int width,int height)
        {
            _meshes = new List<Mesh>();
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

        static public void setLightManager(LightManager lightManager)
        {
            _lightManager = lightManager;
        }

        static public void AddMesh(Mesh mesh)
        {
            _meshes.Add(mesh);
        }
        
        static public void AddFlatMesh(FlatMesh mesh)
        {
            _flatMeshes.Add(mesh);
        }

        static private void ProjectPoints2(ref Vertex[] projectedPs, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 point = projectedPs[i].WorldPoint;
                float x = _projMatrix.elements[0, 0] * (point.x - _camera.CameraX);
                float y = _projMatrix.elements[1, 1] * (point.y - _camera.CameraY);
                
                float z = _projMatrix.elements[2, 2] * (point.z - _camera.CameraZ) + _projMatrix.elements[3, 2];
                float w = _projMatrix.elements[2, 3] * (point.z - _camera.CameraZ);
                //float w = _projMatrix.elements[2, 3] * (point.z + _camera.CameraZ);
                //float z = _projMatrix.elements[2, 2] * (point.z + _projMatrix.elements[3, 2] + _camera.CameraZ);
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

        static private unsafe void SetPixel(byte* buffer, int stride, int x, int y, Color color)
        {
            byte* pixel = buffer + (stride * y) + (x * 4);
            pixel[0] = (byte)(color.B);
            pixel[1] = (byte)(color.G);
            pixel[2] = (byte)(color.R);
            pixel[3] = color.A;
        }

        static private bool pixelInfront(int x, int y, float z)
        {
            if (_zBuffer[y * _width + x] > z)
            {
                return true;
            }
            ;
            return false;
        }
        static private void setZValue(int x, int y, float z)
        {
            _zBuffer[y * _width + x] = z;
        }

        static public unsafe void FillTriangle2(byte* buffer, int stride, Vertex v1, Vertex v2, Vertex v3)
        {
            Vector3 p1 = v1.ScreenSpacePoint;
            Vector3 p2 = v2.ScreenSpacePoint;
            Vector3 p3 = v3.ScreenSpacePoint;

            float area = EdgeFunction(p1,p2,p3);
            if (area <= 0)
            {
                return;
            }

            int x0 = (int)Math.Min(p1.x, p2.x);
            x0 = (int)Math.Min(x0, p3.x);

            int x1 = (int)Math.Max(p1.x, p2.x);
            x1 = (int)Math.Max(x1, p3.x);

            int y0 = (int)Math.Min(p1.y, p2.y);
            y0 = (int)Math.Min(y0, p3.y);

            int y1 = (int)Math.Max(p1.y, p2.y);
            y1 = (int)Math.Max(y1, p3.y);

            for(int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    double w1 = EdgeFunction(p2, p3, x, y) / area;
                    double w2 = EdgeFunction(p3, p1, x, y) /area;
                    double w3 = EdgeFunction(p1, p2, x, y) / area;

                    if (w1 >= 0 && w2 >= 0 && w3 >= 0)
                    {
                        Vertex temp = Vertex.BaryCentrePoint(v1, v2, v3, (float)w1, (float)w2, (float)w3);
                        if (pixelInfront(x,y,temp.ScreenSpacePoint.z))
                        {
                            Color color = _lightManager.GetColorWithLighting(temp);
                            SetPixel(buffer, stride, x, y, color);
                            setZValue(x, y, temp.ScreenSpacePoint.z);
                        }
                    }
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

        //to:do
        static private unsafe void getScreenSpaceFlatMesh(ref Vertex[] vertexBuffer, ref int count)
        {
            int len = count;
            count = 0;
            for (int i = 0; i < len; i += 3)
            {
                if (vertexBuffer[i].InFrustrum() && vertexBuffer[i+1].InFrustrum() && vertexBuffer[i+2].InFrustrum())
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
                    long memoryBfore = GC.GetTotalMemory(false);
                    _cacheTotalPoints = 0;
                    mesh.CopyDataToVertexBuffer(ref _cachedVertexBuffer,ref _cacheTotalPoints);
                    ProjectPoints2(ref _cachedVertexBuffer, _cacheTotalPoints);
                    getScreenSpaceFlatMesh(ref _cachedVertexBuffer, ref _cacheTotalPoints);
                    for (int i = 0; i < _cacheTotalPoints; i += 3)
                    {
                        FillTriangle2(pixelBuffer, stride, _cachedVertexBuffer[i], _cachedVertexBuffer[i+1], _cachedVertexBuffer[i+2]);
                    }
                }
            }
            _bmp.UnlockBits(bmpData);
        }
    };
}
