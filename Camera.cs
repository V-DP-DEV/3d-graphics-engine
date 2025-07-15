using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_based
{
    class Camera
    {
        public float _cameraX { get; set; } = 0;
        public float _cameraY { get; set; } = 0;
        public float _cameraZ { get; set; } = 0;

        public Camera(float cameraX, float cameraY, float cameraZ)
        {
            _cameraX = cameraX;
            _cameraY = cameraY;
            _cameraZ = cameraZ;
        }
        public void moveCamerayTo(float cameraX, float cameraY, float cameraZ)
        {
            _cameraX = cameraX;
            _cameraY = cameraY;
            _cameraZ = cameraZ;
        }
        public void moveCamerayBy(float cameraXInc, float cameraYInc, float cameraZInc)
        {
            _cameraX += cameraXInc;
            _cameraY += cameraYInc;
            _cameraZ += cameraZInc;
        }
    }
}
