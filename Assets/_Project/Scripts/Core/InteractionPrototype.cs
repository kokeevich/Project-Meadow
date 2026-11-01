using UnityEngine;
using ProjectMeadow.Interaction;
using ProjectMeadow.Economy;
using ProjectMeadow.Animals;

namespace ProjectMeadow.Core
{
    public sealed class InteractionPrototype : MonoBehaviour
    {
        private ProjectMeadow.Inventory.InventoryController warehouse;
        private ProjectMeadow.Farming.FoodPlot plot;
        private ChickenPrototype chickens;
        private EconomyService economy;
        private Truck truck;
        private int quantity = 1;
        private string message = "Grow food, collect eggs, then send the truck.";
        private void Awake()
        {
            warehouse = gameObject.AddComponent<ProjectMeadow.Inventory.InventoryController>();
            economy = new EconomyService(warehouse.Inventory);
            chickens = GetComponent<ChickenPrototype>();
            plot = FindFirstObjectByType<ProjectMeadow.Farming.FoodPlot>();
            var camera = UnityEngine.Camera.main;
            if (camera == null) { Debug.LogError("Main Camera missing.", this); return; }
            camera.gameObject.AddComponent<ProjectMeadow.Camera.CameraZoom>();
            gameObject.AddComponent<InteractionSystem>().Initialize(camera);
            CreateTruck();
        }
        private void CreateTruck()
        {
            var root = new GameObject("Market Truck");
            root.transform.position = new Vector3(0, 0, 4);
            truck = root.AddComponent<Truck>();
            truck.Economy = economy;
            // Temporary geometry; visual design follows Phase 5.
            var material = GameObject.Find("Clickable Prototype Cube").GetComponent<Renderer>().sharedMaterial;
            Part(root.transform, "Cargo bed", new Vector3(-.35f, .55f, 0), new Vector3(1.5f, .5f, .85f), material);
            Part(root.transform, "Cab", new Vector3(.7f, .65f, 0), new Vector3(.65f, .9f, .85f), material);
            for (int side = -1; side <= 1; side += 2)
                for (int axle = -1; axle <= 1; axle += 2)
                    Part(root.transform, "Wheel", new Vector3(axle * .65f, .2f, side * .45f), new Vector3(.35f, .35f, .2f), material);
        }
        private static void Part(Transform parent, string label, Vector3 pos, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = label;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = pos;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
        private void Update() => economy?.Tick(Time.deltaTime);
        private void OnGUI()
        {
            if (economy == null) return;
            var inventory = warehouse.Inventory;
            GUI.Box(new Rect(10, 10, 340, 340), "Project Meadow - Farm & Market");
            GUI.Label(new Rect(20, 35, 320, 25), "Coins: " + economy.Currency.Balance + " | Chickens: " + chickens.ChickenCount + "/" + ChickenPrototype.MaximumChickens);
            GUI.Label(new Rect(20, 65, 320, 25), "Eggs: " + inventory.Count("egg") + " | Storage: " + inventory.Used + "/" + inventory.Capacity);
            GUI.Label(new Rect(20, 95, 320, 25), "Food: " + (plot == null ? "-" : plot.State.ToString()));
            if (GUI.Button(new Rect(20, 125, 320, 30), "Market: Eggs (5 coins each)")) truck.Selected = !truck.Selected;
            if (truck.Selected)
            {
                if (GUI.Button(new Rect(20, 165, 40, 30), "-")) quantity = Mathf.Max(1, quantity - 1);
                GUI.Label(new Rect(70, 170, 180, 25), "Eggs to sell: " + quantity);
                if (GUI.Button(new Rect(300, 165, 40, 30), "+")) quantity = Mathf.Min(inventory.Capacity, quantity + 1);
                GUI.enabled = !economy.Busy && inventory.Count("egg") >= quantity;
                if (GUI.Button(new Rect(20, 205, 320, 30), "Send truck (+" + quantity * EconomyService.EggPrice + " coins)"))
                    message = economy.TryDispatch("egg", quantity) ? "Truck dispatched. Payment on return." : "Cannot dispatch.";
                GUI.enabled = true;
            }
            else GUI.Label(new Rect(20, 170, 320, 25), "Click the truck or Market to sell eggs.");
            GUI.enabled = economy.Currency.Balance >= EconomyService.ChickenPrice && chickens.ChickenCount < ChickenPrototype.MaximumChickens;
            if (GUI.Button(new Rect(20, 245, 320, 30), "Buy chicken - 25 coins"))
                message = economy.TryBuyChicken(chickens.TrySpawnPurchasedChicken) ? "A new chicken joined the farm!" : "No space to spawn a chicken.";
            GUI.enabled = true;
            GUI.Label(new Rect(20, 280, 320, 25), economy.Busy ? "Truck returns in " + Mathf.CeilToInt(economy.Remaining) + "s | Cargo: " + economy.Cargo : "Truck ready | Eggs sold: " + economy.EggsSold);
            GUI.Label(new Rect(20, 310, 320, 30), warehouse.FullFeedback ? "Storage full. Sell eggs to make room." : message);
        }
    }
}
