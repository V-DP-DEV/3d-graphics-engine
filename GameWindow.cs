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
        private readonly HashSet<Keys> keyPressed = new HashSet<Keys>();
        private Bitmap bmpDisplay;
        private Bitmap bmpTarget;
        private int framesCompleted = 0;
        private int totalTime = 0;
        readonly object swapLock = new object();
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
            bmpDisplay = new Bitmap(width, height);
            bmpTarget = new Bitmap(width, height);
            this.KeyDown += (s, e) => keyPressed.Add(e.KeyCode);
            this.KeyUp += (s, e) => keyPressed.Remove(e.KeyCode);
        }
        public void OnPaint(object sender, PaintEventArgs e)
        {
            lock (swapLock) {
                e.Graphics.DrawImageUnscaled(bmpDisplay, 0, 0);
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
            totalTime += deltaTime;
            framesCompleted++;
            Console.WriteLine("Fps:average " + totalTime / framesCompleted);
        }

        private void gameLoop()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                long currentTime = watch.ElapsedMilliseconds;
                OnKeyDown?.Invoke(keyPressed);
                
                Renderer.DrawToScreenSpace(bmpTarget);
                lock (swapLock)
                {
                    Bitmap temp = bmpDisplay;
                    bmpDisplay = bmpTarget;
                    bmpTarget = temp;
                }
                //Console.WriteLine("Render " + (watch.ElapsedMilliseconds - currentTime));
                //currentTime = watch.ElapsedMilliseconds;

                this.Invalidate();
                //Console.WriteLine("Refresh "+(watch.ElapsedMilliseconds - currentTime));

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
