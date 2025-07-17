using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_based
{
    public class DirectLight
    {
        Vector3 _lightColor;
        Vector3 _lightDirection;
        public DirectLight(Vector3 lightColor, Vector3 lightDirection)
        {
            _lightColor = lightColor;
            _lightDirection = lightDirection;
        }
        public void MoveBy(float x, float y, float z)
        {
            _lightDirection.x += x;
            _lightDirection.y += y;
            _lightDirection.z += z;
        }
        public void addDirectLight(Vector3 normal, ref Vector3 target)
        {
            float intensity = Math.Max(0, Vector3.GetDotProduct(normal, _lightDirection));
            if (intensity == 0)
            {
                return;
            }
            target.x += _lightColor.x * intensity;
            target.y += _lightColor.y * intensity;
            target.z += _lightColor.z * intensity;
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
        }
        public void getLightDirection(Vector3 point, ref Vector3 lightDir)
        {
            lightDir.x = _lightPos.x - point.x;
            lightDir.y = _lightPos.y - point.y;
            lightDir.z = _lightPos.z - point.z;
        }

        public float getMaxDistance()
        {
            return _maxDistance;
        }
        public Vector3 getColor()
        {
            return _lightColor;
        }

        public void addSphereLight(Vector3 normal, Vector3 point, ref Vector3 target, ref Vector3 lightDirection)
        {
            getLightDirection(point, ref lightDirection);
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
        public Vector3 AmbientColor { get; private set; }
        public DirectLight DirectLight { get; private set; }
        public SphereLight[] SphereLights { get; private set; }
        public int TotalLights { get; private set; }

        public LightManager(Vector3 ambientColor,DirectLight directLight, SphereLight[] sphereLights, int totalLights)
        {
            this.AmbientColor = ambientColor;
            DirectLight = directLight;
            this.SphereLights = sphereLights;
            this.TotalLights = totalLights;
        }

        public void ClampColor(ref Vector3 targetColor)
        {
            targetColor.x = Math.Clamp(targetColor.x, 0, 1);
            targetColor.y = Math.Clamp(targetColor.y, 0, 1);
            targetColor.z = Math.Clamp(targetColor.z, 0, 1);
        }

        public void setDirectLight(DirectLight light)
        {
            DirectLight = light;
        }

        public void setLights(DirectLight light, SphereLight[] sphereLights,int totalLights)
        {
            this.SphereLights = sphereLights;
            this.TotalLights = totalLights;
            DirectLight = light;
        }

        public void addSphereLight(SphereLight light)
        {
            SphereLights[TotalLights] = light;
            TotalLights++;
        }
        public void SetSphereLights(SphereLight[] lights, int totalLights)
        {
            SphereLights = lights;
            this.TotalLights = totalLights;
        }

        public void GetColorWithLighting(Vertex v, ref Vector3 target)
        {
            Vector3 lightingIntensity = AmbientColor;
            if (DirectLight != null)
            {
                DirectLight.addDirectLight(v.Normal, ref lightingIntensity);
            }
            Vector3 LightDir = new Vector3();
            for (int i = 0; i < SphereLights.Length; i++)
            {
                SphereLights[i].addSphereLight(v.Normal, v.WorldPoint, ref lightingIntensity, ref LightDir);
            }

            ClampColor(ref lightingIntensity);
            target.x = (int)(target.x * lightingIntensity.x);
            target.y = (int)(target.y * lightingIntensity.y);
            target.z = (int)(target.z * lightingIntensity.z);
        }
    }
}
