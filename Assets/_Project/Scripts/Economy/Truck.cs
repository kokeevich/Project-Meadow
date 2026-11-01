using UnityEngine;
using ProjectMeadow.Interaction;
namespace ProjectMeadow.Economy
{
    public sealed class Truck : MonoBehaviour, IInteractable
    {
        public EconomyService Economy { private get; set; }
        public bool Selected { get; set; }
        private Vector3 parked;
        private void Start() => parked = transform.position;
        public void Interact() => Selected = true;
        private void Update()
        {
            if (Economy == null) return;
            float progress = Economy.Busy ? 1 - Economy.Remaining / EconomyService.TripSeconds : 0;
            transform.position = parked + Vector3.right * (Mathf.Sin(progress * Mathf.PI) * 7);
        }
    }
}
