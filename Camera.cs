using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_based
{
    class Camera
    {
        public float CameraX { get; set; } = 0;
        public float CameraY { get; set; } = 0;
        public float CameraZ { get; set; } = 0;

        public Camera(float cameraX, float cameraY, float cameraZ)
        {
            this.CameraX = cameraX;
            CameraY = cameraY;
            CameraZ = cameraZ;
        }
        public void moveCamerayTo(float cameraX, float cameraY, float cameraZ)
        {
            this.CameraX = cameraX;
            CameraY = cameraY;
            CameraZ = cameraZ;
        }
        public void moveCamerayBy(float cameraXInc, float cameraYInc, float cameraZInc)
        {
            CameraX += cameraXInc;
            CameraY += cameraYInc;
            CameraZ += cameraZInc;
        }
    }
}
