using UnityEngine;
using ProjectMeadow.Interaction;

namespace ProjectMeadow.Core
{
    public sealed class InteractionPrototype : MonoBehaviour
    {
        private ProjectMeadow.Inventory.InventoryController warehouse;
        private PrototypeCube cube;
        private ProjectMeadow.Farming.FoodPlot plot;
        private void Awake()
        {
            warehouse = gameObject.AddComponent<ProjectMeadow.Inventory.InventoryController>();
            var camera = UnityEngine.Camera.main;
            if (camera == null) { Debug.LogError("Prototype requires a Main Camera.", this); return; }
            camera.gameObject.AddComponent<ProjectMeadow.Camera.CameraZoom>();
            gameObject.AddComponent<InteractionSystem>().Initialize(camera);
            cube = FindFirstObjectByType<PrototypeCube>();
            plot = FindFirstObjectByType<ProjectMeadow.Farming.FoodPlot>();
        }
        private ProjectMeadow.Animals.ChickenController[] chickens;
        private void Start() => Invoke(nameof(FindChickens), 0.1f);
        private void FindChickens() => chickens = FindObjectsByType<ProjectMeadow.Animals.ChickenController>(FindObjectsSortMode.None);
        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 320, 275), "Collection prototype");
            GUI.Label(new Rect(20, 35, 300, 25), "Click / tap the cube. Scroll / pinch to zoom.");
            GUI.Label(new Rect(20, 65, 300, 25), "Interactions: " + (cube == null ? 0 : cube.ClickCount));
            if (chickens != null)
                for (int i = 0; i < chickens.Length; i++)
                    if (chickens[i] != null)
                        GUI.Label(new Rect(20, 155 + i * 25, 300, 25), "Chicken " + (i + 1) + ": " + chickens[i].State + " | Eggs: " + chickens[i].EggsProduced);
            if (warehouse != null)
            {
                var storage = warehouse.Inventory;
                GUI.Label(new Rect(20, 215, 300, 25), "Eggs: " + storage.Count("egg") + " | Storage: " + storage.Used + "/" + storage.Capacity);
                GUI.Label(new Rect(20, 245, 300, 25), warehouse.FullFeedback ? "Storage full - egg remains on the ground" : "Click / tap eggs to collect");
            }
            if (plot != null)
            {
                GUI.Label(new Rect(20, 95, 300, 25), "Food: " + plot.State + " " + Mathf.RoundToInt(plot.Progress * 100) + "%");
                if (GUI.Button(new Rect(20, 125, 290, 25), "Consume ready food (prototype)")) plot.TryConsume();
            }
        }
    }
}
