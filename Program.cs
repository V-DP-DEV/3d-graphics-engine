using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Console_based
{
    internal class Program
    {
        static float _far = 400;
        static float _near = 0.1f;
        static float _fov = 90;
        static float _x, _y, _z = 0;
        static int _width = 1980;
        static int _height = 1080;

        static Vector3[] __vertexBuffer = [new Vector3(5,-5,30), new Vector3(-15, -5, 30),
                new Vector3(-5, -5, 20), new Vector3(-5, -5, 40),new Vector3(-5,10,30)];

        //static public int[] _indexBuffer = [3, 0, 4, 3, 4, 1, 1, 4, 2, 2, 4, 0, 0, 3, 2, 1, 2, 3];

        static public int[] _indexBuffer = [1, 3, 4, 3, 0, 4];

        static public float[] u = [0, 0.8f, 0.4f, 0, 0.8f, 0.4f];
        static public float[] v = [0.8f, 0.8f, 0f, 0.8f, 0.8f, 0];

        //static public float[] u = [0, 0.99f, 0.99f, 0, 0.99f, 0.99f, 0, 0.99f, 0.99f, 0, 0.99f, 0.99f, 0, 0.99f, 0.99f, 0, 0.99f, 0.99f];
        //static public float[] v = [0, 0, 0.99f, 0, 0, 0.99f, 0, 0, 0.99f, 0, 0, 0.99f, 0, 0, 0.99f, 0, 0, 0.99f];

        //static public float[] u = [0, 0.50f, 0.50f, 0, 0.50f, 0.50f, 0, 0.50f, 0.50f, 0, 0.50f, 0.50f, 0, 0.50f, 0.50f, 0, 0.50f, 0.50f];
        //static public float[] v = [0, 0, 0.50f, 0, 0, 0.50f, 0, 0, 0.50f, 0, 0, 0.50f, 0, 0, 0.50f, 0, 0, 0.50f];

        //static public float[] u = [0, 0.25f, 0.50f, 0, 0.25f, 0.50f, 0, 0.25f, 0.50f, 0, 0.25f, 0.50f, 0, 0.25f, 0.50f, 0, 0.25f, 0.50f];
        //static public float[] v = [0.5f, 0, 0.50f, 0.5f, 0, 0.50f, 0.5f, 0, 0.50f, 0.5f, 0, 0.50f, 0.5f, 0, 0.50f, 0.5f, 0, 0.50f];

        //static public float[] u = [0, 0.40f, 0.80f, 0, 0.40f, 0.80f, 0, 0.40f, 0.80f, 0, 0.40f, 0.80f, 0, 0.40f, 0.80f, 0, 0.40f, 0.80f];
        //static public float[] v = [0.8f, 0, 0.80f, 0.8f, 0, 0.80f, 0.8f, 0, 0.80f, 0.8f, 0, 0.80f, 0.8f, 0, 0.80f, 0.8f, 0, 0.80f];

        //static public float[] u = [0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f];
        //static public float[] v = [0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f];

        //static public float[] u = [0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f, 0.40f, 0, 0.80f];
        //static public float[] v = [0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f, 0f, 0.8f, 0.80f];

        //static public float[] u = [0, 0.125f, 0.25f, 0, 0.125f, 0.25f, 0, 0.125f, 0.25f, 0, 0.125f, 0.25f, 0, 0.125f, 0.25f, 0, 0.125f, 0.25f];
        //static public float[] v = [0.25f, 0, 0.25f, 0.25f, 0, 0.25f, 0.25f, 0, 0.25f, 0.25f, 0, 0.25f, 0.25f, 0, 0.25f, 0.25f, 0, 0.25f];

        static public Vector3[] _colourBuffer = [new Vector3(0f,0f,255f), new Vector3(0f, 255f, 255f), new Vector3(255f, 0f, 0f), new Vector3(255f, 255f, 0f), new Vector3(0f, 255f, 0f), new Vector3(0f, 255f, 0f)];
        static FlatMesh _myMesh;
        static FlatMesh _myMesh2 = new FlatMesh();
        static public Camera _camera = new Camera(0,50,200);
        //static public Camera _camera = new Camera(0, 0,67);

        static public DirectLight _sun = new DirectLight(new Vector3(1, 0.6f, 0.6f), new Vector3(0, 1, 0));
        static public DirectLight _noLight = new DirectLight(new Vector3(0, 0,0), new Vector3(0, 0, 0));
        static SphereLight[] _lights = [new SphereLight(0.1f, 0.1f, 0.001f, 200f, new Vector3(5f, -5f, 40), new Vector3(1f, 1f, 0.8f))];
        static SphereLight _currentLight = _lights[0];
        static LightManager _lightManager = new(new(0.2f, 0.2f, 0.2f), _noLight, _lights, 3);

        static GameWindow window = new GameWindow(_width, _height);

        static void OnKeyPress(HashSet<Keys> keyPressed)
        {
            if (keyPressed.Contains(Keys.Up))
            {
                _camera.moveCamerayBy(0, 0, -1f);
            }
            if (keyPressed.Contains(Keys.Down))
            {
                _camera.moveCamerayBy(0, 0f, 1f);
            }
            if (keyPressed.Contains(Keys.K))
            {
                _camera.moveCamerayBy(0, -1f, 0f);
            }
            if (keyPressed.Contains(Keys.I))
            {
                _camera.moveCamerayBy(0, 1f, 0f);
            }
            if (keyPressed.Contains(Keys.J))
            {
                _camera.moveCamerayBy(-1f, 0f, 0f);
            }
            if (keyPressed.Contains(Keys.L))
            {
                _camera.moveCamerayBy(1f, 0f, 0f);
            }
            if (keyPressed.Contains(Keys.A))
            {
                _currentLight.MoveBy(-0.9f, 0, 0);
            }
            if (keyPressed.Contains(Keys.D))
            {
                _currentLight.MoveBy(0.9f, 0, 0);
            }
            if (keyPressed.Contains(Keys.S))
            {
                _currentLight.MoveBy(0, -0.9f, 0);
            }
            if (keyPressed.Contains(Keys.W))
            {
                _currentLight.MoveBy(0, 0.9f, 0);
            }
            if (keyPressed.Contains(Keys.Escape))
            {
                Application.Exit();
            }
            if (keyPressed.Contains(Keys.F1))
            {
                _currentLight = _lights[0];
            }
            if (keyPressed.Contains(Keys.F2))
            {
                _currentLight = _lights[1];
            }
            if (keyPressed.Contains(Keys.F3))
            {
                _currentLight = _lights[2];
            }
        }
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Renderer.SetProjectionMatrix(_far, _near, _fov);
            Renderer.setDrawRules(_width, _height);
            Renderer.setCamera(_camera);

            _myMesh2.LoadMeshFromFile("model.obj");

            //_myMesh = new FlatMesh(__vertexBuffer, _indexBuffer, _colourBuffer);
           // _myMesh = new FlatMesh(__vertexBuffer, _indexBuffer, u, v);
            Renderer.AddFlatMesh(_myMesh2);
            //Renderer.AddFlatMesh(_myMesh);
            Renderer.setTexture("Sandstone Bricks.jfif");
            Renderer.setLightManager(_lightManager);

            window.startGameLoop();
            window.OnKeyDown += OnKeyPress;

            Application.Run(window);
        }
    }
}
