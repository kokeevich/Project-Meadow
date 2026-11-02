using UnityEngine;
using ProjectMeadow.Interaction;
namespace ProjectMeadow.Production
{
    public sealed class ProductionBuilding : MonoBehaviour, IInteractable
    {
        public enum BuildingKind { Mill, CheeseMaker }
        [SerializeField] private BuildingKind kind;
        public BuildingKind Kind => kind;
        public ProductionService Service { get; private set; }
        public bool Selected { get; set; }
        private Transform rotor;
        public void Initialize(ProjectMeadow.Inventory.InventoryService inventory, ProjectMeadow.Economy.CurrencyService currency, RecipeData recipe)
        {
            Service = new ProductionService(inventory, currency, recipe.inputId, recipe.inputCount, recipe.outputId, recipe.outputCount, recipe.seconds);
            rotor = transform.Find("Mill rotor");
        }
        public void Interact()
        {
            foreach (var building in FindObjectsByType<ProductionBuilding>(FindObjectsSortMode.None)) building.Selected = false;
            Selected = true;
            var truck = FindFirstObjectByType<ProjectMeadow.Economy.Truck>();
            if (truck != null) truck.Selected = false;
        }
        private void Update()
        {
            if (Service == null) return;
            Service.Tick(Time.deltaTime);
            if (rotor != null && Service.Queued > 0 && !Service.OutputBlocked) rotor.Rotate(0, 0, -50 * Time.deltaTime);
        }
    }
}
