using UnityEngine;

namespace MiaShooter
{
    public static class GrenadeVisualFactory
    {
        public static GameObject Create(string name, GrenadeType grenadeType)
        {
            bool isSmoke = grenadeType == GrenadeType.Smoke;
            Color bodyColor = isSmoke ? new Color(0.1f, 0.15f, 0.2f) : new Color(0.12f, 0.18f, 0.13f);
            Color indicatorColor = isSmoke ? new Color(0.18f, 0.75f, 1f) : new Color(1f, 0.22f, 0.04f);

            GameObject grenade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            grenade.name = name;
            grenade.transform.localScale = new Vector3(0.28f, 0.36f, 0.28f);
            grenade.GetComponent<Renderer>().material = CreateMaterial(bodyColor, 0.78f, 0.45f);

            CreateDetail(PrimitiveType.Cylinder, "Grenade Band", grenade.transform,
                Vector3.zero, new Vector3(0.54f, 0.06f, 0.54f),
                CreateMaterial(new Color(0.035f, 0.045f, 0.05f), 0.9f, 0.65f));
            CreateDetail(PrimitiveType.Cube, "Grenade Fuse", grenade.transform,
                new Vector3(0f, 0.56f, 0f), new Vector3(0.28f, 0.18f, 0.24f),
                CreateMaterial(new Color(0.22f, 0.25f, 0.24f), 0.85f, 0.7f));
            CreateDetail(PrimitiveType.Sphere, "Grenade Indicator", grenade.transform,
                new Vector3(0f, 0.35f, -0.48f), Vector3.one * 0.12f,
                CreateEmissiveMaterial(indicatorColor, 3f));
            return grenade;
        }

        public static Material CreateMaterial(Color color, float metallic, float smoothness)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        public static Material CreateEmissiveMaterial(Color color, float intensity)
        {
            Material material = CreateMaterial(color, 0.3f, 0.8f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }

        public static void DisableCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider == null)
            {
                return;
            }

            collider.enabled = false;
            Object.Destroy(collider);
        }

        private static void CreateDetail(
            PrimitiveType primitiveType,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject detail = GameObject.CreatePrimitive(primitiveType);
            detail.name = name;
            detail.transform.SetParent(parent, false);
            detail.transform.localPosition = localPosition;
            detail.transform.localScale = localScale;
            detail.GetComponent<Renderer>().material = material;
            DisableCollider(detail);
        }
    }
}
