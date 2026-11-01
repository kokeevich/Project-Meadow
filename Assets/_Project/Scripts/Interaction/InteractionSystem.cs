using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMeadow.Interaction
{
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera viewCamera;
        private bool pendingTap;
        private Vector2 tapStart;
        private ProjectMeadow.UI.FarmHud hud;
        public void Initialize(UnityEngine.Camera camera, ProjectMeadow.UI.FarmHud interfaceView)
        { viewCamera = camera; hud = interfaceView; }

        private void Update()
        {
            Vector2 position;
            var touch = Touchscreen.current;
            int activeTouches = 0;
            if (touch != null) foreach (var finger in touch.touches) if (finger.press.isPressed) activeTouches++;
            if (activeTouches > 1) { pendingTap = false; return; }
            if (touch != null && (touch.primaryTouch.press.isPressed || touch.primaryTouch.press.wasReleasedThisFrame || pendingTap))
            {
                position = touch.primaryTouch.position.ReadValue();
                if (touch.primaryTouch.press.wasPressedThisFrame)
                {
                    tapStart = position;
                    pendingTap = hud == null || !hud.BlocksScreenPoint(position);
                }
                if (Vector2.Distance(position, tapStart) > 16) pendingTap = false;
                if (!touch.primaryTouch.press.wasReleasedThisFrame) return;
                bool tap = pendingTap; pendingTap = false;
                if (!tap) return;
            }
            else
            {
                var mouse = Mouse.current;
                if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
                position = mouse.position.ReadValue();
            }
            if (hud != null && hud.BlocksScreenPoint(position)) return;
            if (viewCamera == null) return;
            if (!Physics.Raycast(viewCamera.ScreenPointToRay(position), out var hit, 200f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return;
            var target = hit.collider.GetComponentInParent<IInteractable>();
            target?.Interact();
        }
    }
}
