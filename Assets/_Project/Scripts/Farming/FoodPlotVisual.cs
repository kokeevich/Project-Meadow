using UnityEngine;
namespace ProjectMeadow.Farming
{
    public sealed class FoodPlotVisual : MonoBehaviour
    {
        private FoodPlot plot;
        private Transform[] shoots;
        private Renderer[] surfaces;
        private Material green, gold;
        public void Initialize(FoodPlot food, Material growing, Material ready)
        {
            plot = food; green = growing; gold = ready;
            shoots = new Transform[12]; surfaces = new Renderer[12];
            for (int i = 0; i < shoots.Length; i++)
            {
                var shoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shoot.name = "Growing feed";
                shoot.transform.SetParent(transform, false);
                shoot.transform.position = plot.transform.position + new Vector3(-.8f + (i % 4) * .53f, .15f, -.7f + (i / 4) * .7f);
                var collider = shoot.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
                shoots[i] = shoot.transform; surfaces[i] = shoot.GetComponent<Renderer>();
            }
        }
        private void Update()
        {
            if (plot == null || shoots == null) return;
            bool visible = plot.State != FoodPlot.GrowthState.Empty;
            for (int i = 0; i < shoots.Length; i++)
            {
                shoots[i].gameObject.SetActive(visible);
                if (!visible) continue;
                float height = .12f + plot.Progress * .55f;
                var p = shoots[i].position; p.y = .31f + height * .5f; shoots[i].position = p;
                shoots[i].localScale = new Vector3(.22f, height, .22f);
                shoots[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 1.8f + i) * 5f);
                surfaces[i].sharedMaterial = plot.State == FoodPlot.GrowthState.Ready ? gold : green;
            }
        }
    }
}
