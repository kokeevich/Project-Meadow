using UnityEngine;
using ProjectMeadow.Interaction;
namespace ProjectMeadow.Economy
{
    public sealed class Truck : MonoBehaviour, IInteractable
    {
        public EconomyService Economy { private get; set; }
        public bool Selected { get; set; }
        private Vector3 parked;
        private Renderer[] visuals;
        private Collider hitbox;
        private void Start() { parked = transform.position; visuals = GetComponentsInChildren<Renderer>(); hitbox = GetComponent<Collider>(); }
        public void Interact()
        {
            Selected = true;
            foreach (var building in FindObjectsByType<ProjectMeadow.Production.ProductionBuilding>(FindObjectsSortMode.None)) building.Selected = false;
        }
        private void Update()
        {
            if (Economy == null) return;
            float progress = Economy.Busy ? 1 - Economy.Remaining / EconomyService.TripSeconds : 0;
            transform.position = parked + Vector3.right * TruckRoute.Offset(progress);
            bool visible = !Economy.Busy || TruckRoute.Visible(progress);
            foreach (var visual in visuals) visual.enabled = visible;
            if (hitbox != null) hitbox.enabled = visible;
            float facing = progress > .7f ? 180 * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.94f, 1f, progress))) : 0;
            transform.rotation = Quaternion.Euler(0, facing, 0);
        }
    }
}
