using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_based
{
    struct Mesh
    {
        Vector3[] _vectorBuffer;
        public int _vectorCount = 0;
        int[] _indexBuffer;
        public int _indexCount = 0;
        Color[] _colorBuffer;
        public int _faceCount = 0;
        Vector3[] _normalVector;
        public Mesh(Vector3[] vectorBuffer, int[] indexBuffer, Color[] colorBuffer)
        {
            _vectorBuffer = vectorBuffer;
            _indexBuffer = indexBuffer;
            _colorBuffer = colorBuffer;
            _vectorCount = vectorBuffer.Length;
            _indexCount = indexBuffer.Length;
            _faceCount = colorBuffer.Length;
            _normalVector = new Vector3[2000];
        }
        public Mesh()
        {
            _vectorBuffer = new Vector3[2000];
            _indexBuffer = new int[2000];
            _colorBuffer = new Color[2000];
        }
        public void Clear()
        {
            _vectorCount = 0;
            _indexCount = 0;
            _faceCount = 0;
        }
        public Vector3[] getVectorBuffer()
        {
            return _vectorBuffer;
        }
        public int[] getIndexBuffer()
        {
            return _indexBuffer;
        }
        public void addVector(Vector3 vec)
        {
            _vectorBuffer[_vectorCount] = vec;
            _vectorCount++;
        }
        public Color[] getColorBuffer()
        {
            return _colorBuffer;
        }
        public Vector3[] getNormalBuffer()
        {
            return _normalVector;
        }
        public void calcAndSetNormal()
        {
            int j = 0;
            for(int i =0; i < _indexCount; i+=3)
            {
                Vector3 point0 = _vectorBuffer[_indexBuffer[i]];
                Vector3 point1 = _vectorBuffer[_indexBuffer[i+1]];
                Vector3 point2 = _vectorBuffer[_indexBuffer[i + 2]];
                _normalVector[j].x = (point0.y - point1.y) *(point0.z - point2.z) - (point0.z - point1.z) * (point0.y - point2.y);
                _normalVector[j].y = (point0.x - point1.x) * (point0.z - point2.z) - (point0.z - point1.z) * (point0.x - point2.x);
                _normalVector[j].z = (point0.x - point1.x) * (point0.y - point2.y) - (point0.y - point1.y) * (point0.x - point2.x);
                _normalVector[j].Normalize();

                j++;
            }
        }
        public void CopyDataToVertexBuffer(ref Vertex[] targetBuffer)
        {
            for(int i = 0; i < _vectorCount; i++)
            {
                targetBuffer[i].WorldPoint = _vectorBuffer[i];
            }
        }
    }

    class FlatMesh
    {
        Vector3[] _points = new Vector3[2000];
        float[] _u = new float[2000];
        float[] _v = new float[2000];
        Vector3[] _colors = new Vector3[2000];
        Vector3[] _normals = new Vector3[2000];
        int _totalPoints = 0;
        int _faceCount = 0;

        public FlatMesh() { }

        public FlatMesh(Vector3[] vectorBuffer, int[] indexBuffer, Vector3[] colorBuffer)
        {
            int j = 0;
            for(int i = 0; i < indexBuffer.Length; i+=3)
            {
                AddFace(vectorBuffer[indexBuffer[i]], vectorBuffer[indexBuffer[i + 1]], vectorBuffer[indexBuffer[i + 2]], colorBuffer[j]);
                j++;
            }
        }

        public FlatMesh(Vector3[] vectorBuffer, int[] indexBuffer, float[] u,float[] v)
        {
            int j = 0;
            for (int i = 0; i < indexBuffer.Length; i += 3)
            {
                AddFace(vectorBuffer[indexBuffer[i]], vectorBuffer[indexBuffer[i + 1]], vectorBuffer[indexBuffer[i + 2]], [u[i], u[i+1], u[i+2]], [v[i], v[i + 1], v[i + 2]]);
                j++;
            }
        }

        public FlatMesh(Vector3[] vectorBuffer, int[] indexBuffer, Vector3[] colorBuffer, Vector3[] normalBuffer)
        {
            int j = 0;
            for (int i = 0; i < indexBuffer.Length; i += 3)
            {
                AddFace(vectorBuffer[indexBuffer[i]], vectorBuffer[indexBuffer[i + 1]], vectorBuffer[indexBuffer[i + 2]], colorBuffer[j], normalBuffer[j]);
                j++;
            }
        }

        public void LoadMeshFromFile(string path)
        {
            Vector3[] points = new Vector3[2000];
            int pointCount = 0;
            Vector3[] normals = new Vector3[2000];
            int normalCount = 0;
            _totalPoints = 0;
            _faceCount = 0;

            StreamReader stream = new StreamReader(path);

            string line;
            while ((line = stream.ReadLine()) != null){
                string[] parts = line.Split(" ");
                if (parts.Length != 0)
                {
                    if (parts[0] == "v")
                    {
                        points[pointCount] = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                        pointCount++;
                    }
                    if (parts[0] == "vn")
                    {
                        normals[normalCount] = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                        normalCount++;
                    }

                    if (parts[0] == "f")
                    {
                        string[] faceIndex = parts[1].Split("//");
                        _normals[_faceCount] = normals[int.Parse(faceIndex[1])-1];
                        _colors[_faceCount] = new Vector3(255f,255f,255f);
                        _points[_totalPoints] = points[int.Parse(faceIndex[0]) - 1];
                        _totalPoints++;
                        for (int i = 1; i < 3; i++)
                        {
                            faceIndex = parts[i + 1].Split("//");
                            _points[_totalPoints] = points[int.Parse(faceIndex[0])-1];
                            _totalPoints++;
                        }
                        _faceCount++;
                    }
                }
            }
            stream.Close();
        }

        public void AddFace(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 color)
        {
            _points[_totalPoints] = p0;
            _points[_totalPoints+1] = p1;
            _points[_totalPoints+2] = p2;
            _colors[_faceCount] = color;

            _normals[_faceCount].x = (p0.y - p1.y) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.y - p2.y);
            _normals[_faceCount].y = ((p0.x - p1.x) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.x - p2.x));
            _normals[_faceCount].z = (p0.x - p1.x) * (p0.y - p2.y) - (p0.y - p1.y) * (p0.x - p2.x);
            _normals[_faceCount].Normalize();
            _totalPoints += 3;
            _faceCount += 1;
        }

        public void AddFace(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 color,Vector3 normal)
        {
            _points[_totalPoints] = p0;
            _points[_totalPoints + 1] = p1;
            _points[_totalPoints + 2] = p2;
            _colors[_faceCount] = color;

            _normals[_faceCount] = normal;

            _totalPoints += 3;
            _faceCount += 1;
        }

        public void AddFace(Vector3 p0, Vector3 p1, Vector3 p2, float[] u, float[] v)
        {
            _points[_totalPoints] = p0;
            _points[_totalPoints + 1] = p1;
            _points[_totalPoints + 2] = p2;

            _u[_totalPoints] = u[0];
            _u[_totalPoints + 1] = u[1];
            _u[_totalPoints + 2] = u[2];

            _v[_totalPoints] = v[0];
            _v[_totalPoints + 1] = v[1];
            _v[_totalPoints + 2] = v[2];

            _normals[_faceCount].x = (p0.y - p1.y) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.y - p2.y);
            _normals[_faceCount].y = ((p0.x - p1.x) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.x - p2.x));
            _normals[_faceCount].z = (p0.x - p1.x) * (p0.y - p2.y) - (p0.y - p1.y) * (p0.x - p2.x);

            _totalPoints += 3;
            _faceCount += 1;
        }

        public void CopyDataToVertexBuffer(ref Vertex[] targetBuffer,ref int total)
        {
            int j = 0;
            for (int i = 0; i < _totalPoints; i+=3)
            {
                targetBuffer[i] = new Vertex(_points[i], _u[i], _v[i], _normals[j], _colors[j]);
                targetBuffer[i+1] = new Vertex(_points[i+1], _u[i + 1], _v[i + 1], _normals[j], _colors[j]);
                targetBuffer[i+2] = new Vertex(_points[i+2], _u[i + 2], _v[i + 2], _normals[j], _colors[j]);
                j++;
                total+=3;
            }
        }
    }


    public struct Vertex
    {
        public Vector3 WorldPoint;
        public Vector4 HomogounesPoint;
        public float U;
        public float V;
        public float InverseW;
        public Vector3 ScreenSpacePoint; 
        public Vector3 Normal;
        public Vector3 Color;

        public Vertex(Vector3 worldPoint,float u, float v, Vector3 normal, Vector3 color)
        {
            WorldPoint = worldPoint;
            U = u;
            V = v;
            Normal = normal;
            Color = color;
        }

        public void SetHomougnesPoint(float x, float y, float z, float w)
        {
            HomogounesPoint = new Vector4(x,y,z,w);
        }

        public bool InFrustrum()
        {
            float w = (HomogounesPoint.w);
            if ( -w > HomogounesPoint.x || w < HomogounesPoint.x)
            {
                return false;
            }
            if (-w > HomogounesPoint.y || w < HomogounesPoint.y)
            {
                return false;
            }
            if (0 > HomogounesPoint.z || w < HomogounesPoint.z)
            {
                return false;
            }
            return true;
        }

        static public void BaryCentrePoint(Vertex v1, Vertex v2, Vertex v3,ref Vertex target, float w1, float w2, float w3)
        {
            
            Vector3 p1 = v1.ScreenSpacePoint;
            Vector3 p2 = v2.ScreenSpacePoint;
            Vector3 p3 = v3.ScreenSpacePoint;
            target.ScreenSpacePoint.x = p1.x * w1 + p2.x * w2 + p3.x * w3;
            target.ScreenSpacePoint.y = p1.y * w1 + p2.y * w2 + p3.y * w3;
            target.ScreenSpacePoint.z = p1.z * w1 + p2.z * w2 + p3.z * w3;

            target.HomogounesPoint.w = v1.HomogounesPoint.w * w1 + v2.HomogounesPoint.w * w2 + v3.HomogounesPoint.w * w3;
            target.InverseW = v1.InverseW * w1 + v2.InverseW * w2 + v3.InverseW * w3;

            p1 = v1.WorldPoint;
            p2 = v2.WorldPoint;
            p3 = v3.WorldPoint;

            target.WorldPoint.x = p1.x * w1 + p2.x * w2 + p3.x * w3;
            target.WorldPoint.y = p1.y * w1 + p2.y * w2 + p3.y * w3;
            target.WorldPoint.z = p1.z * w1 + p2.z * w2 + p3.z * w3;

            target.U = v1.U * w1 + v2.U * w2 + v3.U * w3;
            target.V = v1.V * w1 + v2.V * w2 + v3.V * w3;
            target.Normal = v1.Normal;
            target.Color = v1.Color;
        }

        public void SetToScreenSpace(float width, float height)
        {
            ScreenSpacePoint = new Vector3((int)((HomogounesPoint.x / HomogounesPoint.w + 1) * width), (int)((-HomogounesPoint.y / HomogounesPoint.w + 1) * height), HomogounesPoint.z / HomogounesPoint.w);
            //do additional divisions by w like for u v and color
            U /= HomogounesPoint.w;
            V /= HomogounesPoint.w;
            InverseW = 1 / HomogounesPoint.w;
        }
    }
}
