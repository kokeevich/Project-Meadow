using UnityEngine;
using ProjectMeadow.Interaction;

namespace ProjectMeadow.Core
{
    public sealed class InteractionPrototype : MonoBehaviour
    {
        private PrototypeCube cube;
        private Material groundMaterial;
        private Material cubeMaterial;
        private void Awake()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null) { Debug.LogError("Prototype requires a Main Camera.", this); return; }
            camera.gameObject.AddComponent<ProjectMeadow.Camera.CameraZoom>();
            gameObject.AddComponent<InteractionSystem>().Initialize(camera);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            groundMaterial = new Material(shader);
            groundMaterial.SetColor("_BaseColor", new Color(0.42f, 0.65f, 0.32f));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.SetParent(transform, false);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
            cubeMaterial = new Material(shader);
            cubeMaterial.SetColor("_BaseColor", new Color(0.95f, 0.65f, 0.25f));
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Clickable Prototype Cube";
            target.transform.SetParent(transform, false);
            target.transform.localPosition = new Vector3(0, 0.5f, 0);
            target.GetComponent<Renderer>().sharedMaterial = cubeMaterial;
            cube = target.AddComponent<PrototypeCube>();
        }
        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 320, 90), "Interaction prototype");
            GUI.Label(new Rect(20, 35, 300, 25), "Click / tap the cube. Scroll / pinch to zoom.");
            GUI.Label(new Rect(20, 65, 300, 25), "Interactions: " + (cube == null ? 0 : cube.ClickCount));
        }
        private void OnDestroy()
        {
            if (groundMaterial != null) Destroy(groundMaterial);
            if (cubeMaterial != null) Destroy(cubeMaterial);
        }
    }
}
