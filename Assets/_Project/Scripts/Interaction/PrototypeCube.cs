using UnityEngine;

namespace ProjectMeadow.Interaction
{
    public sealed class PrototypeCube : MonoBehaviour, IInteractable
    {
        public int ClickCount { get; private set; }
        private MaterialPropertyBlock properties;
        private Renderer meshRenderer;
        private void Awake()
        {
            meshRenderer = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
        }
        public void Interact()
        {
            ClickCount++;
            properties.SetColor("_BaseColor", ClickCount % 2 == 0
                ? new Color(0.95f, 0.65f, 0.25f) : new Color(0.3f, 0.75f, 0.9f));
            meshRenderer.SetPropertyBlock(properties);
        }
    }
}
