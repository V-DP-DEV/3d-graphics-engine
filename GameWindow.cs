using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Console_based
{
    class GameWindow : Form
    {
        private readonly HashSet<Keys> _keyPressed = new HashSet<Keys>();
        private Bitmap _bmpDisplay;
        private Bitmap _bmpTarget;
        private int _framesCompleted = 0;
        private int _totalTime = 0;
        readonly object _swapLock = new object();
        public event Action<HashSet<Keys>> OnKeyDown;

        public GameWindow(int width, int height)
        {

            //optimization
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            //styling
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, false);
            this.SetStyle(ControlStyles.Opaque, true);
            this.UpdateStyles();

            this.AutoScaleMode = AutoScaleMode.Dpi;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;
            this.Text = "Game Window";
            this.Paint += OnPaint;
            _bmpDisplay = new Bitmap(width, height);
            _bmpTarget = new Bitmap(width, height);
            this.KeyDown += (s, e) => _keyPressed.Add(e.KeyCode);
            this.KeyUp += (s, e) => _keyPressed.Remove(e.KeyCode);
        }
        public void OnPaint(object sender, PaintEventArgs e)
        {
            lock (_swapLock) {
                e.Graphics.DrawImageUnscaled(_bmpDisplay, 0, 0);
            }
        }
        public void startGameLoop()
        {
            Thread timer = new Thread(gameLoop);
            timer.IsBackground = true;
            timer.Start();
        }
        private void UpdateFrameRateInfo(int deltaTime)
        {
            Console.WriteLine("FPS: " + deltaTime);
            _totalTime += deltaTime;
            _framesCompleted++;
            Console.WriteLine("Fps:average " + _totalTime / _framesCompleted);
        }

        private void gameLoop()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                long currentTime = watch.ElapsedMilliseconds;
                OnKeyDown?.Invoke(_keyPressed);
                
                Renderer.DrawToScreenSpace(_bmpTarget);
                lock (_swapLock)
                {
                    Bitmap temp = _bmpDisplay;
                    _bmpDisplay = _bmpTarget;
                    _bmpTarget = temp;
                }

                this.Invalidate();

                int deltaT = (int)(watch.ElapsedMilliseconds - currentTime);
                UpdateFrameRateInfo(deltaT);
                if (deltaT < 16)
                {
                    Thread.Sleep((16 - deltaT));
                }
            }
        }
    }
}
