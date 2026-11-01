using UnityEngine;
using ProjectMeadow.Interaction;

namespace ProjectMeadow.Core
{
    public sealed class InteractionPrototype : MonoBehaviour
    {
        private PrototypeCube cube;
        private ProjectMeadow.Farming.FoodPlot plot;
        private void Awake()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null) { Debug.LogError("Prototype requires a Main Camera.", this); return; }
            camera.gameObject.AddComponent<ProjectMeadow.Camera.CameraZoom>();
            gameObject.AddComponent<InteractionSystem>().Initialize(camera);
            cube = FindFirstObjectByType<PrototypeCube>();
            plot = FindFirstObjectByType<ProjectMeadow.Farming.FoodPlot>();
        }
        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 320, 150), "Farming prototype");
            GUI.Label(new Rect(20, 35, 300, 25), "Click / tap the cube. Scroll / pinch to zoom.");
            GUI.Label(new Rect(20, 65, 300, 25), "Interactions: " + (cube == null ? 0 : cube.ClickCount));
            if (plot != null)
            {
                GUI.Label(new Rect(20, 95, 300, 25), "Food: " + plot.State + " " + Mathf.RoundToInt(plot.Progress * 100) + "%");
                if (GUI.Button(new Rect(20, 125, 290, 25), "Consume ready food (prototype)")) plot.TryConsume();
            }
        }
    }
}
