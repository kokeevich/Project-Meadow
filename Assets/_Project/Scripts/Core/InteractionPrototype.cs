using UnityEngine;
using ProjectMeadow.Interaction;
using ProjectMeadow.Economy;
using ProjectMeadow.Animals;
using ProjectMeadow.UI;

namespace ProjectMeadow.Core
{
    public sealed class InteractionPrototype : MonoBehaviour
    {
        [SerializeField] private Material cream, teal, wood, dark, glass, gold, leaves;
        private EconomyService economy;
        private void Awake()
        {
            var warehouse = gameObject.AddComponent<ProjectMeadow.Inventory.InventoryController>();
            economy = new EconomyService(warehouse.Inventory);
            var chickens = GetComponent<ChickenPrototype>();
            var plot = FindFirstObjectByType<ProjectMeadow.Farming.FoodPlot>();
            var camera = UnityEngine.Camera.main;
            if (camera == null) { Debug.LogError("Main Camera missing.", this); return; }
            var truck = CreateTruck();
            var hud = gameObject.AddComponent<FarmHud>();
            hud.Initialize(warehouse, economy, chickens, plot, truck);
            camera.gameObject.AddComponent<ProjectMeadow.Camera.CameraZoom>().Initialize(hud);
            gameObject.AddComponent<InteractionSystem>().Initialize(camera, hud);
            if (plot != null)
            {
                var crops = new GameObject("Garden crops");
                crops.transform.SetParent(transform, false);
                crops.AddComponent<ProjectMeadow.Farming.FoodPlotVisual>().Initialize(plot, leaves, gold);
            }
        }
        private Truck CreateTruck()
        {
            var root = new GameObject("Market Truck");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(-1.5f, 0, -3.9f);
            var truck = root.AddComponent<Truck>(); truck.Economy = economy;
            // One collider on the vehicle; visual pieces do not block taps or navigation independently.
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, .6f, 0); collider.size = new Vector3(2.5f, 1.2f, 1.1f);
            Part(root.transform, "Chassis", PrimitiveType.Cube, new Vector3(0, .35f, 0), new Vector3(2.45f, .18f, 1), dark);
            Part(root.transform, "Flatbed", PrimitiveType.Cube, new Vector3(-.45f, .53f, 0), new Vector3(1.4f, .18f, 1), wood);
            Part(root.transform, "Cab", PrimitiveType.Cube, new Vector3(.72f, .77f, 0), new Vector3(.85f, .85f, .98f), teal);
            Part(root.transform, "Roof", PrimitiveType.Cube, new Vector3(.65f, 1.23f, 0), new Vector3(1.05f, .13f, 1.1f), cream);
            Part(root.transform, "Windshield", PrimitiveType.Cube, new Vector3(1.15f, .99f, 0), new Vector3(.025f, .32f, .75f), glass);
            Part(root.transform, "Bumper", PrimitiveType.Cube, new Vector3(1.24f, .4f, 0), new Vector3(.14f, .16f, 1.12f), cream);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(root.transform, "Side window", PrimitiveType.Cube, new Vector3(.7f, .98f, side * .5f), new Vector3(.56f, .32f, .02f), glass);
                Part(root.transform, "Lamp", PrimitiveType.Sphere, new Vector3(1.17f, .62f, side * .34f), Vector3.one * .18f, gold);
                for (int row = 0; row < 2; row++)
                    Part(root.transform, "Bed rail", PrimitiveType.Cube, new Vector3(-.45f, .73f + row * .22f, side * .48f), new Vector3(1.4f, .14f, .08f), wood);
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    var wheel = Part(root.transform, "Wheel", PrimitiveType.Cylinder, new Vector3(axle * .8f, .27f, side * .55f), new Vector3(.52f, .12f, .52f), dark);
                    wheel.localRotation = Quaternion.Euler(90, 0, 0);
                    var hub = Part(root.transform, "Hub", PrimitiveType.Cylinder, new Vector3(axle * .8f, .27f, side * .69f), new Vector3(.22f, .025f, .22f), cream);
                    hub.localRotation = Quaternion.Euler(90, 0, 0);
                }
            }
            return truck;
        }
        private static Transform Part(Transform parent, string label, PrimitiveType shape, Vector3 pos, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(shape); part.name = label;
            part.transform.SetParent(parent, false); part.transform.localPosition = pos; part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            return part.transform;
        }
        private void Update() => economy?.Tick(Time.deltaTime);
    }
}
