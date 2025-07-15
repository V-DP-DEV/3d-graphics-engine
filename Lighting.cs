using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_based
{
    public class DirectLight
    {
        Vector3 _LightColor;
        Vector3 _LightDirection;
        public DirectLight(Vector3 lightColor, Vector3 lightDirection)
        {
            _LightColor = lightColor;
            _LightDirection = lightDirection;
        }
        public void MoveBy(float x, float y, float z)
        {
            _LightDirection.x += x;
            _LightDirection.y += y;
            _LightDirection.z += z;
        }
        public void addDirectLight(Vector3 normal, ref Vector3 target)
        {
            float intensity = Math.Max(0, Vector3.GetDotProduct(normal, _LightDirection));
            if (intensity == 0)
            {
                return;
            }
            target.x += _LightColor.x * intensity;
            target.y += _LightColor.y * intensity;
            target.z += _LightColor.z * intensity;
        }
    }

    public class SphereLight
    {
        float _constant;
        float _linear;
        float _quadtratic;
        float _maxDistance;
        Vector3 _lightPos;
        Vector3 _lightColor;
        public SphereLight(float constant, float linear, float quadratic, float maxDistance, Vector3 lightPos, Vector3 lightColor)
        {
            _constant = constant;
            _linear = linear;
            _quadtratic = quadratic;
            _maxDistance = maxDistance;
            _lightPos = lightPos;
            _lightColor = lightColor;
        }

        public float getAttenuation(float distance)
        {
            return 1 / (_constant + _linear * distance + _quadtratic * distance * distance);
        }
        public Vector3 getLightDirection(Vector3 point)
        {
            Vector3 result = new Vector3(_lightPos.x - point.x, _lightPos.y - point.y, _lightPos.z - point.z);
            return result;
            //return new Vector3(_lightPos.x - point.x,_lightPos.y - point.y, _lightPos.z - point.z);
        }
        public float getMaxDistance()
        {
            return _maxDistance;
        }
        public Vector3 getColor()
        {
            return _lightColor;
        }

        public void addSphereLight(Vector3 normal, Vector3 point, ref Vector3 target)
        {
            Vector3 lightDirection = getLightDirection(point);
            float distance = lightDirection.GetMagnitude();
            if (distance > _maxDistance)
            {
                return;
            }

            lightDirection.Normalize(distance);
            float intensity = Math.Max(0, Vector3.GetDotProduct(normal, lightDirection));
            if (intensity == 0)
            {
                return;
            }
            float attenuation = getAttenuation(distance);
            if (attenuation < 0.0001f)
            {
                return;
            }
            target.x += _lightColor.x * intensity * attenuation;
            target.y += _lightColor.y * intensity * attenuation;
            target.z += _lightColor.z * intensity * attenuation;
        }

        public void MoveBy(float x, float y, float z)
        {
            _lightPos.x += x;
            _lightPos.y += y;
            _lightPos.z += z;
        }
        public void PrintPos()
        {
            Console.WriteLine($"x : {_lightPos.x} y: {_lightPos.y} z: {_lightPos.z}");
        }
    }

    public class LightManager
    {
        public Vector3 ambientColor { get; private set; }
        public DirectLight DirectLigth { get; private set; }
        public SphereLight[] sphereLights { get; private set; }
        public int totalLights { get; private set; }

        public LightManager(Vector3 ambientColor,DirectLight directLight, SphereLight[] sphereLights, int totalLights)
        {
            this.ambientColor = ambientColor;
            DirectLigth = directLight;
            this.sphereLights = sphereLights;
            this.totalLights = totalLights;
        }

        public void ClampColor(ref Vector3 targetColor)
        {
            targetColor.x = Math.Clamp(targetColor.x, 0, 1);
            targetColor.y = Math.Clamp(targetColor.y, 0, 1);
            targetColor.z = Math.Clamp(targetColor.z, 0, 1);
        }

        public void setDirectLight(DirectLight light)
        {
            DirectLigth = light;
        }

        public void setLights(DirectLight light, SphereLight[] sphereLights,int totalLights)
        {
            this.sphereLights = sphereLights;
            this.totalLights = totalLights;
            DirectLigth = light;
        }

        public void addSphereLight(SphereLight light)
        {
            sphereLights[totalLights] = light;
            totalLights++;
        }
        public void SetSphereLights(SphereLight[] lights, int totalLights)
        {
            sphereLights = lights;
            this.totalLights = totalLights;
        }

        public Color GetColorWithLighting(Vertex v)
        {
            Vector3 lightingIntensity = ambientColor;
            //_sun.addDirectLight(v._normal, ref lightingIntensity);

            for (int i = 0; i < sphereLights.Length; i++)
            {
                sphereLights[i].addSphereLight(v._normal, v._worldPoint, ref lightingIntensity);
            }
            ClampColor(ref lightingIntensity);
            Color result = Color.FromArgb((int)(lightingIntensity.x * v._color.R), (int)(lightingIntensity.y * v._color.G), (int)(lightingIntensity.z * v._color.B));
            return result;
        }
    }
}
