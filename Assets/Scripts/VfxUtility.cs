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
            SpawnExplosionLight(position, 55f, 36f, 0.65f);
            SpawnCoreFlash(position);
            SpawnFireballCore(position);
            SpawnRisingFireColumn(position);
            SpawnVolumetricSmoke(position);
            SpawnSparks(position);
            SpawnShockwaveSystem(position);
            SpawnGrenadeDebris(position);
            SpawnGroundScorch(position);
        }

        private static void SpawnExplosionLight(Vector3 position, float peakIntensity, float range, float lifetime)
        {
            GameObject lightObject = new GameObject("Grenade Dynamic Blast Light");
            lightObject.transform.position = position + Vector3.up * 0.4f;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.88f, 0.55f);
            light.intensity = peakIntensity;
            light.range = range;
            light.shadows = LightShadows.None;
            lightObject.AddComponent<ExplosionDynamicLight>().Initialize(
                light,
                peakIntensity,
                lifetime,
                new Color(1f, 0.88f, 0.55f),
                new Color(1f, 0.38f, 0.05f));
        }

        private static void SpawnCoreFlash(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Blast Flash Core");
            effect.transform.position = position + Vector3.up * 0.3f;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.09f;
            main.startSpeed = 0.5f;
            main.startSize = 5.5f;
            main.startColor = new Color(1f, 0.98f, 0.85f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateAdditiveMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(1);
            particles.Play();
        }

        private static void SpawnFireballCore(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Fireball Core");
            effect.transform.position = position + Vector3.up * 0.4f;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 18f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.2f, 4.2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(0.25f, 1.4f);
            sizeCurve.AddKey(1f, 1.8f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient fireGradient = new Gradient();
            fireGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.75f), 0f),
                    new GradientColorKey(new Color(1f, 0.65f, 0.08f), 0.20f),
                    new GradientColorKey(new Color(1f, 0.28f, 0.02f), 0.55f),
                    new GradientColorKey(new Color(0.6f, 0.08f, 0.01f), 0.85f),
                    new GradientColorKey(new Color(0.12f, 0.04f, 0.02f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.95f, 0.6f),
                    new GradientAlphaKey(0.6f, 0.85f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(fireGradient);

            ParticleSystem.LimitVelocityOverLifetimeModule limit = particles.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.35f;
            limit.limit = new ParticleSystem.MinMaxCurve(3.5f);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.95f;
            noise.frequency = 0.4f;
            noise.scrollSpeed = 0.3f;

            ParticleSystem.RotationOverLifetimeModule rol = particles.rotationOverLifetime;
            rol.enabled = true;
            rol.z = new ParticleSystem.MinMaxCurve(-2.5f, 2.5f);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateAdditiveMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(200);
            particles.Play();
        }

        private static void SpawnRisingFireColumn(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Rising Fire Pillar");
            effect.transform.position = position + Vector3.up * 0.2f;
            effect.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(14f, 24f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.4f;

            ParticleSystem.VelocityOverLifetimeModule vol = particles.velocityOverLifetime;
            vol.enabled = true;
            vol.space = ParticleSystemSimulationSpace.World;
            vol.x = new ParticleSystem.MinMaxCurve(0f);
            vol.y = new ParticleSystem.MinMaxCurve(2.4f);
            vol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.6f);
            curve.AddKey(0.35f, 1.3f);
            curve.AddKey(1f, 1.6f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient fireGradient = new Gradient();
            fireGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0f),
                    new GradientColorKey(new Color(1f, 0.55f, 0.05f), 0.35f),
                    new GradientColorKey(new Color(0.85f, 0.18f, 0.02f), 0.75f),
                    new GradientColorKey(new Color(0.15f, 0.05f, 0.02f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(fireGradient);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.85f;
            noise.frequency = 0.35f;

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateAdditiveMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(85);
            particles.Play();
        }

        private static void SpawnVolumetricSmoke(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Volumetric Smoke Plume");
            effect.transform.position = position + Vector3.up * 0.3f;

            // 1. Towering Mushroom Smoke Plume
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5.0f, 8.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 16f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.8f, 5.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.55f;

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve smokeCurve = new AnimationCurve();
            smokeCurve.AddKey(0f, 0.45f);
            smokeCurve.AddKey(0.25f, 1.6f);
            smokeCurve.AddKey(0.6f, 2.6f);
            smokeCurve.AddKey(1f, 3.4f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

            ParticleSystem.VelocityOverLifetimeModule vol = particles.velocityOverLifetime;
            vol.enabled = true;
            vol.space = ParticleSystemSimulationSpace.World;
            vol.x = new ParticleSystem.MinMaxCurve(0f);
            vol.y = new ParticleSystem.MinMaxCurve(2.8f);
            vol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystem.LimitVelocityOverLifetimeModule limit = particles.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.42f;
            limit.limit = new ParticleSystem.MinMaxCurve(2.2f);

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient smokeGradient = new Gradient();
            smokeGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.12f, 0.11f, 0.11f), 0f),
                    new GradientColorKey(new Color(0.20f, 0.17f, 0.14f), 0.25f),
                    new GradientColorKey(new Color(0.30f, 0.29f, 0.28f), 0.60f),
                    new GradientColorKey(new Color(0.44f, 0.43f, 0.42f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.92f, 0f),
                    new GradientAlphaKey(0.86f, 0.35f),
                    new GradientAlphaKey(0.55f, 0.70f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(smokeGradient);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 1.25f;
            noise.frequency = 0.22f;
            noise.scrollSpeed = 0.2f;

            ParticleSystem.RotationOverLifetimeModule rol = particles.rotationOverLifetime;
            rol.enabled = true;
            rol.z = new ParticleSystem.MinMaxCurve(-0.9f, 0.9f);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateSmokeMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(180);
            particles.Play();

            // 2. Rolling Blast Perimeter Smoke Ring (sweeping along the floor)
            GameObject ringObj = new GameObject("Grenade Blast Perimeter Smoke");
            ringObj.transform.SetParent(effect.transform, false);
            ringObj.transform.localPosition = Vector3.zero;

            ParticleSystem ringParticles = ringObj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule rMain = ringParticles.main;
            rMain.loop = false;
            rMain.playOnAwake = false;
            rMain.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 5.5f);
            rMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 18f);
            rMain.startSize = new ParticleSystem.MinMaxCurve(1.8f, 3.6f);
            rMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            rMain.startColor = Color.white;
            rMain.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule rShape = ringParticles.shape;
            rShape.shapeType = ParticleSystemShapeType.Circle;
            rShape.radius = 0.8f;
            rShape.rotation = new Vector3(90f, 0f, 0f);

            ParticleSystem.SizeOverLifetimeModule rSol = ringParticles.sizeOverLifetime;
            rSol.enabled = true;
            AnimationCurve ringCurve = new AnimationCurve();
            ringCurve.AddKey(0f, 0.4f);
            ringCurve.AddKey(0.3f, 1.4f);
            ringCurve.AddKey(1f, 2.4f);
            rSol.size = new ParticleSystem.MinMaxCurve(1f, ringCurve);

            ParticleSystem.LimitVelocityOverLifetimeModule rLimit = ringParticles.limitVelocityOverLifetime;
            rLimit.enabled = true;
            rLimit.dampen = 0.45f;
            rLimit.limit = new ParticleSystem.MinMaxCurve(1.5f);

            ParticleSystem.ColorOverLifetimeModule rCol = ringParticles.colorOverLifetime;
            rCol.enabled = true;
            Gradient ringGradient = new Gradient();
            ringGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.18f, 0.16f, 0.15f), 0f),
                    new GradientColorKey(new Color(0.32f, 0.30f, 0.28f), 0.4f),
                    new GradientColorKey(new Color(0.42f, 0.40f, 0.38f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0.65f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });
            rCol.color = new ParticleSystem.MinMaxGradient(ringGradient);

            ParticleSystem.RotationOverLifetimeModule rRol = ringParticles.rotationOverLifetime;
            rRol.enabled = true;
            rRol.z = new ParticleSystem.MinMaxCurve(-1.1f, 1.1f);

            ParticleSystemRenderer rRenderer = ringParticles.GetComponent<ParticleSystemRenderer>();
            rRenderer.material = CreateSmokeMaterial(Color.white);
            rRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            ringParticles.Emit(80);
            ringParticles.Play();
        }

        private static void SpawnSparks(Vector3 position)
        {
            GameObject effect = new GameObject("Grenade Explosion Sparks");
            effect.transform.position = position + Vector3.up * 0.2f;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(20f, 36f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.26f);
            main.gravityModifier = 1.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient sparkGradient = new Gradient();
            sparkGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.65f), 0f),
                    new GradientColorKey(new Color(1f, 0.65f, 0.15f), 0.45f),
                    new GradientColorKey(new Color(0.9f, 0.25f, 0.05f), 0.85f),
                    new GradientColorKey(new Color(0.2f, 0.05f, 0.02f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(sparkGradient);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateAdditiveMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.042f;
            renderer.lengthScale = 2.6f;

            particles.Emit(180);
            particles.Play();
        }

        private static void SpawnShockwaveSystem(Vector3 position)
        {
            // 1. Primary Fiery Blast Shockwave Ring
            GameObject primary = new GameObject("Grenade Primary Fire Shockwave");
            primary.transform.position = position + Vector3.up * 0.06f;
            LineRenderer line1 = primary.AddComponent<LineRenderer>();
            line1.useWorldSpace = false;
            line1.loop = true;
            line1.positionCount = 80;
            line1.material = CreateAdditiveMaterial(new Color(1f, 0.65f, 0.15f, 1f));
            primary.AddComponent<ShockwaveRing>().Initialize(line1, 17.5f, 1.1f, 0.48f, new Color(1f, 0.65f, 0.15f, 1f));

            // 2. Secondary Supersonic Air Compression Ring
            GameObject secondary = new GameObject("Grenade Supersonic Compression Wave");
            secondary.transform.position = position + Vector3.up * 0.08f;
            LineRenderer line2 = secondary.AddComponent<LineRenderer>();
            line2.useWorldSpace = false;
            line2.loop = true;
            line2.positionCount = 80;
            line2.material = CreateAdditiveMaterial(new Color(0.8f, 0.96f, 1f, 0.85f));
            secondary.AddComponent<ShockwaveRing>().Initialize(line2, 23.0f, 0.6f, 0.38f, new Color(0.8f, 0.96f, 1f, 0.85f));

            // 3. Vertical 3D Shockwave Ring
            GameObject vertical = new GameObject("Grenade Vertical 3D Shockwave");
            vertical.transform.position = position + Vector3.up * 0.4f;
            vertical.transform.rotation = Quaternion.Euler(70f, 35f, 0f);
            LineRenderer line3 = vertical.AddComponent<LineRenderer>();
            line3.useWorldSpace = false;
            line3.loop = true;
            line3.positionCount = 80;
            line3.material = CreateAdditiveMaterial(new Color(1f, 0.85f, 0.4f, 0.75f));
            vertical.AddComponent<ShockwaveRing>().Initialize(line3, 15.0f, 0.75f, 0.44f, new Color(1f, 0.85f, 0.4f, 0.75f));

            // 4. Ground Dust Shockwave
            GameObject dustObj = new GameObject("Grenade Ground Dust Shockwave");
            dustObj.transform.position = position + Vector3.up * 0.04f;
            dustObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            ParticleSystem particles = dustObj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(18f, 28f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            shape.arc = 360f;

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve dustCurve = new AnimationCurve();
            dustCurve.AddKey(0f, 0.5f);
            dustCurve.AddKey(1f, 2.5f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, dustCurve);

            ParticleSystem.LimitVelocityOverLifetimeModule limit = particles.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.4f;
            limit.limit = new ParticleSystem.MinMaxCurve(2f);

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient dustGradient = new Gradient();
            dustGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.38f, 0.34f, 0.28f), 0f),
                    new GradientColorKey(new Color(0.32f, 0.28f, 0.24f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.65f, 0f),
                    new GradientAlphaKey(0.55f, 0.4f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(dustGradient);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateSmokeMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(180);
            particles.Play();
        }

        private static void SpawnGrenadeDebris(Vector3 position)
        {
            Color charredColor = new Color(0.12f, 0.10f, 0.08f);
            Material debrisMat = CreateLitMaterial(charredColor);

            for (int i = 0; i < 16; i++)
            {
                GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fragment.name = "Grenade Blast Shrapnel";
                fragment.transform.position = position + Vector3.up * 0.2f + Random.insideUnitSphere * 0.45f;
                fragment.transform.localScale = Vector3.one * Random.Range(0.12f, 0.32f);
                fragment.GetComponent<Renderer>().material = debrisMat;

                Rigidbody body = fragment.AddComponent<Rigidbody>();
                body.mass = Random.Range(0.15f, 0.45f);
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.AddExplosionForce(850f, position, 12f, 1.6f, ForceMode.Impulse);
                body.AddTorque(Random.insideUnitSphere * 40f, ForceMode.Impulse);

                fragment.AddComponent<TimedDestroy>().Lifetime = 2.6f;
            }
        }

        private static void SpawnGroundScorch(Vector3 position)
        {
            GameObject scorch = GameObject.CreatePrimitive(PrimitiveType.Quad);
            scorch.name = "Grenade Blast Scorch Mark";

            Collider col = scorch.GetComponent<Collider>();
            if (col != null)
            {
                SafeDestroy(col);
            }

            scorch.transform.position = position + Vector3.up * 0.025f;
            scorch.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            scorch.transform.localScale = new Vector3(7.0f, 7.0f, 1f);

            Renderer rend = scorch.GetComponent<Renderer>();
            Material mat = CreateScorchMaterial();
            rend.material = mat;

            scorch.AddComponent<ScorchFader>().Initialize(mat, 8f);
        }

        private static void SpawnShockwave(Vector3 position)
        {
            SpawnShockwaveSystem(position);
        }

        public static void SpawnSmokeCloud(Vector3 position)
        {
            GameObject effect = new GameObject("Smoke Grenade Cloud");
            effect.transform.position = position + Vector3.up * 0.25f;

            // 1. Initial Pyrotechnic Burst Flash & Ignition Puff
            SpawnTimedPointLight("Smoke Ignition Light", position + Vector3.up * 0.4f, new Color(1f, 0.75f, 0.45f), 4.5f, 6.5f, 0.18f);

            // 2. Primary Volumetric Billowing Smoke Body
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5.5f, 8.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.68f, 0.70f, 0.73f, 1f), new Color(0.85f, 0.88f, 0.90f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.65f;

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve coreSizeCurve = new AnimationCurve();
            coreSizeCurve.AddKey(0f, 0.5f);
            coreSizeCurve.AddKey(0.25f, 1.4f);
            coreSizeCurve.AddKey(1f, 2.0f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, coreSizeCurve);

            ParticleSystem.VelocityOverLifetimeModule vol = particles.velocityOverLifetime;
            vol.enabled = true;
            vol.space = ParticleSystemSimulationSpace.World;
            vol.x = new ParticleSystem.MinMaxCurve(0f);
            vol.y = new ParticleSystem.MinMaxCurve(0.22f);
            vol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystem.LimitVelocityOverLifetimeModule limit = particles.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.45f;
            limit.limit = new ParticleSystem.MinMaxCurve(0.6f);

            ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
            col.enabled = true;
            Gradient smokeGradient = new Gradient();
            smokeGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.65f, 0.68f, 0.72f), 0f),
                    new GradientColorKey(new Color(0.78f, 0.80f, 0.83f), 0.35f),
                    new GradientColorKey(new Color(0.85f, 0.87f, 0.89f), 0.75f),
                    new GradientColorKey(new Color(0.72f, 0.75f, 0.78f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.65f, 0.10f),
                    new GradientAlphaKey(0.55f, 0.50f),
                    new GradientAlphaKey(0.30f, 0.80f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(smokeGradient);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.85f;
            noise.frequency = 0.20f;
            noise.scrollSpeed = 0.18f;

            ParticleSystem.RotationOverLifetimeModule rol = particles.rotationOverLifetime;
            rol.enabled = true;
            rol.z = new ParticleSystem.MinMaxCurve(-0.65f, 0.65f);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateSmokeMaterial(Color.white);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particles.Emit(105);
            particles.Play();

            // 3. Ground Creeping Carpet Layer (dense horizontal mist hugging the floor)
            GameObject groundCarpet = new GameObject("Smoke Ground Creeping Carpet");
            groundCarpet.transform.SetParent(effect.transform, false);
            groundCarpet.transform.localPosition = Vector3.down * 0.18f;

            ParticleSystem carpetParticles = groundCarpet.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule cMain = carpetParticles.main;
            cMain.loop = false;
            cMain.playOnAwake = false;
            cMain.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 7.0f);
            cMain.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.8f);
            cMain.startSize = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
            cMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            cMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0.60f, 0.63f, 0.66f, 1f), new Color(0.75f, 0.78f, 0.81f, 1f));
            cMain.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule cShape = carpetParticles.shape;
            cShape.shapeType = ParticleSystemShapeType.Circle;
            cShape.radius = 0.85f;
            cShape.rotation = new Vector3(90f, 0f, 0f);

            ParticleSystem.SizeOverLifetimeModule cSol = carpetParticles.sizeOverLifetime;
            cSol.enabled = true;
            AnimationCurve carpetCurve = new AnimationCurve();
            carpetCurve.AddKey(0f, 0.5f);
            carpetCurve.AddKey(0.3f, 1.4f);
            carpetCurve.AddKey(1f, 1.9f);
            cSol.size = new ParticleSystem.MinMaxCurve(1f, carpetCurve);

            ParticleSystem.VelocityOverLifetimeModule cVol = carpetParticles.velocityOverLifetime;
            cVol.enabled = true;
            cVol.space = ParticleSystemSimulationSpace.World;
            cVol.x = new ParticleSystem.MinMaxCurve(0f);
            cVol.y = new ParticleSystem.MinMaxCurve(0.08f);
            cVol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystem.LimitVelocityOverLifetimeModule cLimit = carpetParticles.limitVelocityOverLifetime;
            cLimit.enabled = true;
            cLimit.dampen = 0.45f;
            cLimit.limit = new ParticleSystem.MinMaxCurve(0.5f);

            ParticleSystem.ColorOverLifetimeModule cCol = carpetParticles.colorOverLifetime;
            cCol.enabled = true;
            Gradient carpetGradient = new Gradient();
            carpetGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.60f, 0.63f, 0.66f), 0f),
                    new GradientColorKey(new Color(0.72f, 0.75f, 0.78f), 0.5f),
                    new GradientColorKey(new Color(0.80f, 0.82f, 0.84f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.48f, 0.12f),
                    new GradientAlphaKey(0.36f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            cCol.color = new ParticleSystem.MinMaxGradient(carpetGradient);

            ParticleSystem.NoiseModule cNoise = carpetParticles.noise;
            cNoise.enabled = true;
            cNoise.strength = 0.65f;
            cNoise.frequency = 0.25f;

            ParticleSystem.RotationOverLifetimeModule cRol = carpetParticles.rotationOverLifetime;
            cRol.enabled = true;
            cRol.z = new ParticleSystem.MinMaxCurve(-0.55f, 0.55f);

            ParticleSystemRenderer cRenderer = carpetParticles.GetComponent<ParticleSystemRenderer>();
            cRenderer.material = CreateSmokeMaterial(Color.white);
            cRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            carpetParticles.Emit(55);
            carpetParticles.Play();

            // 4. Soft Atmospheric Wisps (lingering, ambient volumetric depth)
            GameObject wispsObj = new GameObject("Smoke Atmospheric Wisps");
            wispsObj.transform.SetParent(effect.transform, false);
            wispsObj.transform.localPosition = Vector3.up * 0.4f;

            ParticleSystem wispParticles = wispsObj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule wMain = wispParticles.main;
            wMain.loop = false;
            wMain.playOnAwake = false;
            wMain.startLifetime = new ParticleSystem.MinMaxCurve(6.0f, 8.5f);
            wMain.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            wMain.startSize = new ParticleSystem.MinMaxCurve(1.8f, 3.0f);
            wMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            wMain.startColor = new Color(0.75f, 0.78f, 0.81f, 1f);
            wMain.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule wShape = wispParticles.shape;
            wShape.shapeType = ParticleSystemShapeType.Sphere;
            wShape.radius = 0.95f;

            ParticleSystem.SizeOverLifetimeModule wSol = wispParticles.sizeOverLifetime;
            wSol.enabled = true;
            AnimationCurve wispCurve = new AnimationCurve();
            wispCurve.AddKey(0f, 0.6f);
            wispCurve.AddKey(0.4f, 1.3f);
            wispCurve.AddKey(1f, 1.7f);
            wSol.size = new ParticleSystem.MinMaxCurve(1f, wispCurve);

            ParticleSystem.VelocityOverLifetimeModule wVol = wispParticles.velocityOverLifetime;
            wVol.enabled = true;
            wVol.space = ParticleSystemSimulationSpace.World;
            wVol.x = new ParticleSystem.MinMaxCurve(0f);
            wVol.y = new ParticleSystem.MinMaxCurve(0.35f);
            wVol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystem.ColorOverLifetimeModule wCol = wispParticles.colorOverLifetime;
            wCol.enabled = true;
            Gradient wispGradient = new Gradient();
            wispGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.70f, 0.73f, 0.76f), 0f),
                    new GradientColorKey(new Color(0.82f, 0.85f, 0.88f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.22f, 0.18f),
                    new GradientAlphaKey(0.16f, 0.65f),
                    new GradientAlphaKey(0f, 1f)
                });
            wCol.color = new ParticleSystem.MinMaxGradient(wispGradient);

            ParticleSystemRenderer wRenderer = wispParticles.GetComponent<ParticleSystemRenderer>();
            wRenderer.material = CreateSmokeMaterial(Color.white);
            wRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            wispParticles.Emit(30);
            wispParticles.Play();
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

        private static Texture2D softParticleTexture;
        private static Texture2D scorchDecalTexture;
        private static Texture2D smokeParticleTexture;

        public static void SafeDestroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static Texture2D GetParticleTexture()
        {
            if (softParticleTexture != null)
            {
                return softParticleTexture;
            }

            const int size = 64;
            softParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            softParticleTexture.name = "ProceduralSoftParticle";
            softParticleTexture.wrapMode = TextureWrapMode.Clamp;
            softParticleTexture.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxRadius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                    if (dist >= 1f)
                    {
                        softParticleTexture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float falloff = 0.5f * (1f + Mathf.Cos(dist * Mathf.PI));
                        falloff = Mathf.Pow(falloff, 1.35f);
                        softParticleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, falloff));
                    }
                }
            }

            softParticleTexture.Apply();
            return softParticleTexture;
        }

        private static Texture2D GetScorchTexture()
        {
            if (scorchDecalTexture != null)
            {
                return scorchDecalTexture;
            }

            const int size = 128;
            scorchDecalTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            scorchDecalTexture.name = "ProceduralScorch";
            scorchDecalTexture.wrapMode = TextureWrapMode.Clamp;
            scorchDecalTexture.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxRadius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                    if (dist >= 1f)
                    {
                        scorchDecalTexture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float falloff = Mathf.Pow(1f - dist, 1.8f);
                        float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.45f + 0.55f;
                        float alpha = Mathf.Clamp01(falloff * noise * 0.88f);
                        scorchDecalTexture.SetPixel(x, y, new Color(0.04f, 0.035f, 0.03f, alpha));
                    }
                }
            }

            scorchDecalTexture.Apply();
            return scorchDecalTexture;
        }

        private static Material CreateAdditiveMaterial(Color color)
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
            material.mainTexture = GetParticleTexture();
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
            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")
                ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
            material.mainTexture = GetParticleTexture();
            return material;
        }

        private static Material CreateScorchMaterial()
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")
                ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            Color c = new Color(0.04f, 0.035f, 0.03f, 0.85f);
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", c);
            }
            if (material.HasProperty("_Color"))
            {
                material.color = c;
            }
            material.mainTexture = GetScorchTexture();
            return material;
        }

        private static Texture2D GetSmokeParticleTexture()
        {
            if (smokeParticleTexture != null)
            {
                return smokeParticleTexture;
            }

            const int size = 128;
            smokeParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            smokeParticleTexture.name = "ProceduralOrganicSmoke";
            smokeParticleTexture.wrapMode = TextureWrapMode.Clamp;
            smokeParticleTexture.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxRadius = size * 0.47f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                    if (dist >= 1f)
                    {
                        smokeParticleTexture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float u = x * 0.065f;
                        float v = y * 0.065f;
                        float n1 = Mathf.PerlinNoise(u, v);
                        float n2 = Mathf.PerlinNoise(u * 2.4f + 6.1f, v * 2.4f + 2.7f) * 0.5f;
                        float n3 = Mathf.PerlinNoise(u * 5.0f + 14.2f, v * 5.0f + 9.5f) * 0.25f;
                        float cloudNoise = (n1 + n2 + n3) / 1.75f;

                        // Solid billowing core that feathers organically along the outer perimeter
                        float radialFade = Mathf.Clamp01((1f - dist) / 0.65f);
                        float falloff = Mathf.SmoothStep(0f, 1f, radialFade);

                        float density = falloff * Mathf.Lerp(0.7f, 1.3f, cloudNoise);
                        density = Mathf.Clamp01(density);
                        density = Mathf.SmoothStep(0f, 1f, density);

                        float edgeFade = Mathf.Clamp01((1f - dist) * 12f);
                        density *= edgeFade;

                        // Give each procedural smoke billow internal micro-shading / depth
                        float shading = Mathf.Clamp01(Mathf.Lerp(0.78f, 1.0f, cloudNoise));
                        smokeParticleTexture.SetPixel(x, y, new Color(shading, shading, shading, density));
                    }
                }
            }

            smokeParticleTexture.Apply();
            return smokeParticleTexture;
        }

        private static Material CreateSmokeMaterial(Color color)
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")
                ?? Shader.Find("Particles/Standard Unlit");
            Material material = new Material(shader);
            Color tint = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, color.a * 0.5f);
            if (material.HasProperty("_TintColor"))
            {
                material.SetColor("_TintColor", tint);
            }
            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
            material.mainTexture = GetSmokeParticleTexture();
            return material;
        }
    }

    public sealed class TimedDestroy : MonoBehaviour
    {
        public float Lifetime { get; set; } = 1f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(Lifetime);
            VfxUtility.SafeDestroy(gameObject);
        }
    }

    public sealed class ExplosionDynamicLight : MonoBehaviour
    {
        private Light lightSource;
        private float peakIntensity;
        private float duration;
        private float elapsed;
        private Color peakColor;
        private Color fireColor;

        public void Initialize(Light source, float peak, float life, Color initialColor, Color lingeringColor)
        {
            lightSource = source;
            peakIntensity = peak;
            duration = life;
            peakColor = initialColor;
            fireColor = lingeringColor;
        }

        public void ManualUpdate(float dt)
        {
            elapsed += dt;
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

            if (lightSource != null)
            {
                if (progress < 0.15f)
                {
                    float flashT = progress / 0.15f;
                    lightSource.intensity = Mathf.Lerp(peakIntensity, peakIntensity * 0.45f, flashT);
                    lightSource.color = Color.Lerp(peakColor, fireColor, flashT);
                }
                else
                {
                    float decayT = (progress - 0.15f) / 0.85f;
                    lightSource.intensity = Mathf.Lerp(peakIntensity * 0.45f, 0f, decayT * decayT);
                    lightSource.color = fireColor;
                }
            }

            if (progress >= 1f)
            {
                VfxUtility.SafeDestroy(gameObject);
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                ManualUpdate(Time.deltaTime);
            }
        }
    }

    public sealed class ShockwaveRing : MonoBehaviour
    {
        private LineRenderer line;
        private float maxRadius;
        private float initialWidth;
        private float duration;
        private float elapsed;
        private Color baseColor;

        public void Initialize(LineRenderer targetLine, float radius, float startWidth, float lifetime, Color color)
        {
            line = targetLine;
            maxRadius = radius;
            initialWidth = startWidth;
            duration = lifetime;
            baseColor = color;
            UpdateRing(0.35f, 1f, 0f);
        }

        public void ManualUpdate(float dt)
        {
            elapsed += dt;
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            float radius = Mathf.Lerp(0.35f, maxRadius, eased);
            float alpha = Mathf.Pow(1f - progress, 1.4f);
            UpdateRing(radius, alpha, progress);

            if (progress >= 1f)
            {
                VfxUtility.SafeDestroy(gameObject);
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                ManualUpdate(Time.deltaTime);
            }
        }

        private void UpdateRing(float radius, float alpha, float progress)
        {
            if (line == null)
            {
                return;
            }

            Color c = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha);
            line.startColor = c;
            line.endColor = c;
            float currentWidth = Mathf.Lerp(initialWidth, 0.04f, progress);
            line.startWidth = currentWidth;
            line.endWidth = currentWidth;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }
    }

    public sealed class ScorchFader : MonoBehaviour
    {
        private Material material;
        private float duration;
        private float elapsed;
        private Color initialColor;

        public void Initialize(Material mat, float lifetime)
        {
            material = mat;
            duration = lifetime;
            initialColor = new Color(0.04f, 0.035f, 0.03f, 0.85f);
        }

        public void ManualUpdate(float dt)
        {
            elapsed += dt;
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float fadeProgress = Mathf.Clamp01((elapsed - 3.5f) / (duration - 3.5f));
            float alpha = Mathf.Lerp(initialColor.a, 0f, fadeProgress);

            if (material != null)
            {
                Color c = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                if (material.HasProperty("_TintColor"))
                {
                    material.SetColor("_TintColor", c);
                }
                if (material.HasProperty("_Color"))
                {
                    material.color = c;
                }
            }

            if (progress >= 1f)
            {
                VfxUtility.SafeDestroy(gameObject);
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                ManualUpdate(Time.deltaTime);
            }
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
