using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    public static class VfxUtility
    {
        public static void SpawnTracer(Vector3 start, Vector3 end)
        {
            GameObject tracer = new GameObject("Tracer");
            LineRenderer line = tracer.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPositions(new[] { start, end });
            line.startWidth = 0.035f;
            line.endWidth = 0.008f;
            line.material = CreateAdditiveMaterial(new Color(1f, 0.75f, 0.18f));
            line.startColor = new Color(1f, 0.85f, 0.3f, 1f);
            line.endColor = new Color(0.15f, 0.8f, 1f, 0f);
            tracer.AddComponent<TimedDestroy>().Lifetime = 0.055f;
        }

        public static void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
        {
            SpawnBurst("Muzzle Flash", position, direction, new Color(1f, 0.55f, 0.08f), 12, 0.12f, 4.5f, 0.16f);
            SpawnTimedPointLight("Muzzle Light", position, new Color(1f, 0.45f, 0.08f), 5f, 5f, 0.06f);
        }

        public static void SpawnImpact(Vector3 position, Vector3 normal, Color color)
        {
            SpawnBurst("Impact Sparks", position + normal * 0.02f, normal, color, 18, 0.35f, 5f, 0.07f);
        }

        public static void SpawnExplosion(Vector3 position, Color color)
        {
            SpawnBurst("Target Explosion", position, Vector3.up, color, 55, 0.7f, 7f, 0.18f, true);
            SpawnBurst("Explosion Core", position, Vector3.up, new Color(1f, 0.5f, 0.08f), 24, 0.4f, 4f, 0.3f, true);

            SpawnTimedPointLight("Explosion Light", position, color, 8f, 9f, 0.16f);
        }

        public static void SpawnReloadPulse(Vector3 position)
        {
            Color color = new Color(0.08f, 0.85f, 1f);
            SpawnBurst("Reload Energy Pulse", position, Vector3.up, color, 20, 0.45f, 1.2f, 0.055f, true);
            SpawnTimedPointLight("Reload Light", position, color, 3.5f, 2.5f, 0.35f);
        }

        public static void SpawnFragments(Vector3 position, Color color)
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fragment.name = "Target Fragment";
                fragment.transform.position = position + Random.insideUnitSphere * 0.35f;
                fragment.transform.localScale = Vector3.one * Random.Range(0.08f, 0.2f);
                fragment.GetComponent<Renderer>().material = CreateLitMaterial(color);
                Rigidbody body = fragment.AddComponent<Rigidbody>();
                body.mass = 0.15f;
                body.AddExplosionForce(320f, position, 5f, 1.2f);
                fragment.AddComponent<TimedDestroy>().Lifetime = 1.8f;
            }
        }

        private static void SpawnBurst(
            string name,
            Vector3 position,
            Vector3 direction,
            Color color,
            int count,
            float lifetime,
            float speed,
            float size,
            bool spherical = false)
        {
            GameObject effect = new GameObject(name);
            effect.transform.position = position;
            effect.transform.rotation = spherical
                ? Quaternion.identity
                : Quaternion.LookRotation(direction.sqrMagnitude > 0.01f ? direction : Vector3.forward);

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = spherical ? ParticleSystemShapeType.Sphere : ParticleSystemShapeType.Cone;
            shape.radius = spherical ? 0.2f : 0.02f;
            shape.angle = spherical ? 25f : 22f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateAdditiveMaterial(color);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(count);
            particles.Play();
        }

        private static void SpawnTimedPointLight(
            string name,
            Vector3 position,
            Color color,
            float intensity,
            float range,
            float lifetime)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            lightObject.AddComponent<TimedDestroy>().Lifetime = lifetime;
        }

        private static Material CreateAdditiveMaterial(Color color)
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static Material CreateLitMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", 0.65f);
            material.SetFloat("_Glossiness", 0.75f);
            return material;
        }
    }

    public sealed class TimedDestroy : MonoBehaviour
    {
        public float Lifetime { get; set; } = 1f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(Lifetime);
            Destroy(gameObject);
        }
    }
}
