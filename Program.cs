using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Console_based
{
    internal class Program
    {
        static float far = 400;
        static float near = 0.1f;
        static float fov = 90;
        static float x, y, z = 0;
        static int width = 1980;
        static int height = 1080;

        static Vector3[] vertexBuffer = [new Vector3(5,-5,30), new Vector3(-15, -5, 30),
                new Vector3(-5, -5, 20), new Vector3(-5, -5, 40),new Vector3(-5,10,30)];

        //static public int[] indexBuffer = [0, 3, 4, 3, 1, 4, 1, 2, 4, 2, 0, 4, 0, 2, 3, 1, 3, 2];
        static public int[] indexBuffer = [3, 0, 4, 3, 4, 1, 1, 4, 2, 2, 4, 0, 0, 3, 2, 1, 2, 3];
        //static public Color[] colourBuffer = [Color.Red,Color.Blue,Color.Aqua,Color.Pink,Color.Lime,Color.Lime];
        static public Color[] colourBuffer = [Color.White, Color.White, Color.White, Color.White, Color.White, Color.White];
        static Mesh myMesh;
        static FlatMesh myMesh2 = new();

        static public Camera _camera = new Camera(0,0,170);
        
        static public DirectLight sun = new DirectLight(new Vector3(1, 0.6f, 0.6f), new Vector3(0, 1, 0));
        static public DirectLight noLight = new DirectLight(new Vector3(0, 0,0), new Vector3(0, 0, 0));
        static SphereLight[] _lights = [new SphereLight(0.1f, 0.1f, 0.001f, 200f, new Vector3(5f, -5f, 20), new Vector3(1f, 1f, 0.8f))];
        static SphereLight _currentLight;
        static LightManager _lightManager = new(new(0.2f, 0.2f, 0.2f), sun, _lights, 3);

        static GameWindow window = new GameWindow(width, height);

        static void OnKeyPress(HashSet<Keys> keyPressed)
        {
            if (keyPressed.Contains(Keys.Up))
            {
                _currentLight.MoveBy(0, 0, 0.5f);
                //   _camera.moveCamerayBy(0, 0, 1);
            }
            if (keyPressed.Contains(Keys.Down))
            {
                //_camera.moveCamerayBy(0, 0, -1);
                _currentLight.MoveBy(0, 0, -0.5f);
            }
            if (keyPressed.Contains(Keys.A))
            {
                _currentLight.MoveBy(-0.9f, 0, 0);
                //_camera.moveCamerayBy(-1, 0, 0);
            }
            if (keyPressed.Contains(Keys.D))
            {
                _currentLight.MoveBy(0.9f, 0, 0);
                //_camera.moveCamerayBy(1, 0, 0);
            }
            if (keyPressed.Contains(Keys.S))
            {
                _currentLight.MoveBy(0, -0.9f, 0);
                //_camera.moveCamerayBy(0, 1, 0);
            }
            if (keyPressed.Contains(Keys.W))
            {
                //_camera.moveCamerayBy(0, -1, 0);
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
            //_light.PrintPos();
        }
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Renderer.SetProjectionMatrix(far, near, fov);
            Renderer.setDrawRules(width, height);
            Renderer.setCamera(_camera);

            myMesh2.LoadMeshFromFile("model.obj");

            Renderer.AddFlatMesh(myMesh2);
            Renderer.setLightManager(_lightManager);

            window.startGameLoop();
            window.OnKeyDown += OnKeyPress;

            Application.Run(window);
        }
    }
}
