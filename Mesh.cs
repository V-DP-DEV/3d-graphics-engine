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
        Color[] _ColorBuffer;
        public int _faceCount = 0;
        Vector3[] _normalVector;
        public Mesh(Vector3[] vectorBuffer, int[] indexBuffer, Color[] colorBuffer)
        {
            _vectorBuffer = vectorBuffer;
            _indexBuffer = indexBuffer;
            _ColorBuffer = colorBuffer;
            _vectorCount = vectorBuffer.Length;
            _indexCount = indexBuffer.Length;
            _faceCount = colorBuffer.Length;
            _normalVector = new Vector3[2000];
        }
        public Mesh()
        {
            _vectorBuffer = new Vector3[2000];
            _indexBuffer = new int[2000];
            _ColorBuffer = new Color[2000];
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
            return _ColorBuffer;
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
                targetBuffer[i]._worldPoint = _vectorBuffer[i];
            }
        }
    }

    class FlatMesh
    {
        Vector3[] _points = new Vector3[2000];
        Color[] _colors = new Color[2000];
        Vector3[] _normals = new Vector3[2000];
        int _totalPoints = 0;
        int _faceCount = 0;

        public FlatMesh() { }

        public FlatMesh(Vector3[] vectorBuffer, int[] indexBuffer, Color[] colorBuffer)
        {
            int j = 0;
            for(int i = 0; i < indexBuffer.Length; i+=3)
            {
                AddFace(vectorBuffer[indexBuffer[i]], vectorBuffer[indexBuffer[i + 1]], vectorBuffer[indexBuffer[i + 2]], colorBuffer[j]);
                j++;
            }
        }

        public FlatMesh(Vector3[] vectorBuffer, int[] indexBuffer, Color[] colorBuffer, Vector3[] normalBuffer)
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
                        _colors[_faceCount] = Color.White;
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

        public void AddFace(Vector3 p0, Vector3 p1, Vector3 p2, Color color)
        {
            _points[_totalPoints] = p0;
            _points[_totalPoints+1] = p1;
            _points[_totalPoints+2] = p2;
            _colors[_faceCount] = color;

            //_normals[_faceCount].x = (p2.y - p0.y) * (p1.z - p0.z) - (p2.z - p0.z) * (p1.y - p0.y);
            //_normals[_faceCount].y = (p2.x - p0.x) * (p1.z - p0.z) - (p2.z - p0.z) * (p1.x - p0.x);
            //_normals[_faceCount].z = ((p2.x - p0.x) * (p1.y - p0.y) - (p2.y - p0.y) * (p1.x - p0.x))*-1;

            _normals[_faceCount].x = (p0.y - p1.y) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.y - p2.y);
            _normals[_faceCount].y = ((p0.x - p1.x) * (p0.z - p2.z) - (p0.z - p1.z) * (p0.x - p2.x)) * -1;
            _normals[_faceCount].z = (p0.x - p1.x) * (p0.y - p2.y) - (p0.y - p1.y) * (p0.x - p2.x);
            _normals[_faceCount].Normalize();
            _totalPoints += 3;
            _faceCount += 1;
        }

        public void AddFace(Vector3 p0, Vector3 p1, Vector3 p2, Color color,Vector3 normal)
        {
            _points[_totalPoints] = p0;
            _points[_totalPoints + 1] = p1;
            _points[_totalPoints + 2] = p2;
            _colors[_faceCount] = color;

            _normals[_faceCount] = normal;

            _totalPoints += 3;
            _faceCount += 1;
        }

        public void CopyDataToVertexBuffer(ref Vertex[] targetBuffer,ref int total)
        {
            int j = 0;
            for (int i = 0; i < _totalPoints; i+=3)
            {
                targetBuffer[i] = new Vertex(_points[i], 0, 0, _normals[j], _colors[j]);
                targetBuffer[i+1] = new Vertex(_points[i+1], 0, 0, _normals[j], _colors[j]);
                targetBuffer[i+2] = new Vertex(_points[i+2], 0, 0, _normals[j], _colors[j]);
                j++;
                total+=3;
            }
        }
    }


    public struct Vertex
    {
        public Vector3 _worldPoint;
        public Vector4 _homogounesPoint;
        public float _u;
        public float _v;
        public Vector3 _screenSpacePoint; 
        public Vector3 _normal;
        public Color _color;

        public Vertex(Vector3 worldPoint,float u, float v, Vector3 normal, Color color)
        {
            _worldPoint = worldPoint;
            _u = u;
            _v = v;
            _normal = normal;
            _color = color;
        }

        public void SetHomougnesPoint(float x, float y, float z, float w)
        {
            _homogounesPoint = new Vector4(x,y,z,w);
        }

        public bool InFrustrum()
        {
            if ( -_homogounesPoint.w > _homogounesPoint.x || _homogounesPoint.w < _homogounesPoint.x)
            {
                return false;
            }
            if (-_homogounesPoint.w > _homogounesPoint.y || _homogounesPoint.w < _homogounesPoint.y)
            {
                return false;
            }
            if (0 > _homogounesPoint.z || _homogounesPoint.w < _homogounesPoint.z)
            {
                return false;
            }
            return true;
        }

        static public Vertex BaryCentrePoint(Vertex v1, Vertex v2, Vertex v3, float w1, float w2, float w3)
        {
            Vertex result = new Vertex();
            Vector3 p1 = v1._screenSpacePoint;
            Vector3 p2 = v2._screenSpacePoint;
            Vector3 p3 = v3._screenSpacePoint;
            result._screenSpacePoint = new Vector3(p1.x * w1 + p2.x * w2 + p3.x * w3, p1.y * w1 + p2.y * w2 + p3.y * w3, p1.z * w1 + p2.z * w2 + p3.z * w3);
            p1 = v1._worldPoint;
            p2 = v2._worldPoint;
            p3 = v3._worldPoint;

            result._worldPoint = new Vector3(p1.x * w1 + p2.x * w2 + p3.x * w3, p1.y * w1 + p2.y * w2 + p3.y * w3, p1.z * w1 + p2.z * w2 + p3.z * w3);

            result._normal = v1._normal;
            result._color = v1._color;
            return result;
        }

        public void SetToScreenSpace(float width, float height)
        {
            _screenSpacePoint = new Vector3((int)((_homogounesPoint.x / _homogounesPoint.w + 1) * width), (int)((_homogounesPoint.y / _homogounesPoint.w + 1) * height), _homogounesPoint.z / _homogounesPoint.w);
            //do additional divisions by w like for u v and color
        }
    }
}
