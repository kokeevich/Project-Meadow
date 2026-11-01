using UnityEngine;
using ProjectMeadow.Interaction;
namespace ProjectMeadow.Farming
{
    public sealed class GrainPlot : MonoBehaviour, IInteractable
    {
        public bool Growing { get; private set; }
        public bool Ready { get; private set; }
        public float Progress { get; private set; }
        private ProjectMeadow.Inventory.InventoryController inventory;
        private Transform crop;
        public void Initialize(ProjectMeadow.Inventory.InventoryController warehouse) { inventory = warehouse; crop = transform.Find("Wheat"); }
        public void Interact()
        {
            if (inventory == null) return;
            if (Ready)
            {
                if (!inventory.TryStore("grain", 2)) return;
                Ready = false; Growing = false; Progress = 0;
            }
            else if (!Growing) Growing = true;
        }
        private void Update()
        {
            if (Growing && !Ready) { Progress = Mathf.Clamp01(Progress + Time.deltaTime / 5); if (Progress >= 1) Ready = true; }
            if (crop != null) { crop.gameObject.SetActive(Growing || Ready); crop.localScale = new Vector3(1, .15f + .85f * Progress, 1); }
        }
    }
}
