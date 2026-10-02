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

        public static void SpawnGrenadeExplosion(Vector3 position)
        {
            SpawnBurst("Grenade Explosion Core", position, Vector3.up, new Color(1f, 0.3f, 0.035f), 150, 1f, 12f, 0.48f, true);
            SpawnBurst("Grenade Explosion Sparks", position, Vector3.up, new Color(1f, 0.82f, 0.2f), 90, 0.75f, 16f, 0.11f, true);
            SpawnBurst("Grenade Explosion Smoke", position, Vector3.up, new Color(0.2f, 0.22f, 0.25f), 65, 2f, 4.5f, 0.9f, true);
            SpawnTimedPointLight("Grenade Explosion Light", position, new Color(1f, 0.25f, 0.035f), 18f, 18f, 0.3f);
            SpawnShockwave(position);
        }

        public static void SpawnSmokeCloud(Vector3 position)
        {
            GameObject effect = new GameObject("Smoke Grenade Cloud");
            effect.transform.position = position + Vector3.up * 0.25f;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.45f, 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.8f, 3.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.52f, 0.58f, 0.62f, 0.78f),
                new Color(0.12f, 0.16f, 0.2f, 0.7f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.65f;

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.65f;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.2f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = 0.35f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateTransparentParticleMaterial(new Color(0.38f, 0.43f, 0.47f, 0.75f));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(120);
            particles.Play();
        }

        private static void SpawnShockwave(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Shockwave");
            effect.transform.position = position + Vector3.up * 0.06f;
            LineRenderer line = effect.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 64;
            line.material = CreateAdditiveMaterial(new Color(1f, 0.58f, 0.12f));
            effect.AddComponent<ShockwaveEffect>().Initialize(line, 9f, 0.48f);
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

        private static Material CreateTransparentParticleMaterial(Color color)
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")
                ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            material.color = color;
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

    public sealed class ShockwaveEffect : MonoBehaviour
    {
        private LineRenderer line;
        private float maxRadius;
        private float duration;
        private float elapsed;

        public void Initialize(LineRenderer targetLine, float radius, float lifetime)
        {
            line = targetLine;
            maxRadius = radius;
            duration = lifetime;
            line.startWidth = 0.24f;
            line.endWidth = 0.24f;
            UpdateRing(0.35f, 1f);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            UpdateRing(Mathf.Lerp(0.35f, maxRadius, progress), 1f - progress);
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateRing(float radius, float alpha)
        {
            if (line == null)
            {
                return;
            }

            Color color = new Color(1f, 0.55f, 0.12f, alpha);
            line.startColor = color;
            line.endColor = color;
            line.startWidth = Mathf.Lerp(0.24f, 0.035f, 1f - alpha);
            line.endWidth = line.startWidth;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }
    }
}
