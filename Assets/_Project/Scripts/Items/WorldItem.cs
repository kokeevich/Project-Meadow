using UnityEngine;
using ProjectMeadow.Interaction;
using ProjectMeadow.Inventory;
namespace ProjectMeadow.Items
{
    [RequireComponent(typeof(Collider))]
    public sealed class WorldItem : MonoBehaviour, IInteractable
    {
        private ItemData data;
        private InventoryController inventory;
        private bool collected;
        private float elapsed;
        private Vector3 startPosition;
        private Vector3 startScale;
        public void Initialize(ItemData itemData, InventoryController destination)
        { data = itemData; inventory = destination; }
        public void Interact()
        {
            if (collected || data == null || inventory == null) return;
            // Inventory commits first. A full warehouse leaves the item in the world.
            if (!inventory.TryCollect(data.id)) return;
            collected = true;
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            var body = GetComponent<Rigidbody>();
            if (body != null) body.isKinematic = true;
            startPosition = transform.position;
            startScale = transform.localScale;
        }
        private void Update()
        {
            if (!collected) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / .3f);
            transform.position = startPosition + Vector3.up * t * .8f;
            transform.localScale = startScale * (1 - t);
            if (t >= 1) Destroy(gameObject);
        }
    }
}
