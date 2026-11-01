using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMeadow.Interaction
{
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera viewCamera;
        public void Initialize(UnityEngine.Camera camera) => viewCamera = camera;

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
            // Reserve the prototype HUD area; clicks there must not hit the world.
            if (position.y > Screen.height - 235 && position.x < 340) return;
            if (viewCamera == null) return;
            if (!Physics.Raycast(viewCamera.ScreenPointToRay(position), out var hit, 200f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return;
            var target = hit.collider.GetComponentInParent<IInteractable>();
            target?.Interact();
        }
    }
}
