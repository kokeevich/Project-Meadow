using UnityEngine;
namespace ProjectMeadow.UI
{
    public sealed class WorldLabel : MonoBehaviour
    {
        private Transform label;
        private UnityEngine.Camera view;
        public void Initialize(string caption, float height)
        {
            var node = new GameObject("World label"); node.transform.SetParent(transform, false);
            node.transform.localPosition = Vector3.up * height;
            var text = node.AddComponent<TextMesh>(); text.text = caption;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48; text.characterSize = .065f; text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center; text.color = new Color(.17f, .24f, .18f);
            node.GetComponent<Renderer>().sharedMaterial = text.font.material;
            label = node.transform; view = UnityEngine.Camera.main;
        }
        private void LateUpdate() { if (view != null && label != null) label.rotation = view.transform.rotation; }
    }
}
