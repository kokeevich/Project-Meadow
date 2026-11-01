using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMeadow.Interaction
{
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera viewCamera;
        private ProjectMeadow.UI.FarmHud hud;
        public void Initialize(UnityEngine.Camera camera, ProjectMeadow.UI.FarmHud interfaceView)
        { viewCamera = camera; hud = interfaceView; }

        private void Update()
        {
            Vector2 position;
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                int activeTouches = 0;
                foreach (var finger in touch.touches) if (finger.press.isPressed) activeTouches++;
                if (activeTouches > 1) return;
                if (!touch.primaryTouch.press.wasPressedThisFrame) return;
                position = touch.primaryTouch.position.ReadValue();
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
