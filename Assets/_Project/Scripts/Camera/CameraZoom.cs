using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMeadow.Camera
{
    public sealed class CameraZoom : MonoBehaviour
    {
        [SerializeField] private float minimumDistance = 14f;
        [SerializeField] private float maximumDistance = 28f;
        private ProjectMeadow.UI.FarmHud hud;
        public void Initialize(ProjectMeadow.UI.FarmHud interfaceView) => hud = interfaceView;
        private Vector3 focus = Vector3.zero;
        private float previousPinch;
        private bool pinching;
        private Vector2 previousMidpoint;
        private void Update()
        {
            if (hud != null && hud.WorldBlocked) return;
            float delta = 0f;
            Vector2 drag = Vector2.zero;
            Vector2 keys = Vector2.zero;
            var touch = Touchscreen.current;
            if (touch != null && touch.touches.Count >= 2 &&
                touch.touches[0].press.isPressed && touch.touches[1].press.isPressed)
            {
                if (hud != null && (hud.BlocksScreenPoint(touch.touches[0].position.ReadValue()) || hud.BlocksScreenPoint(touch.touches[1].position.ReadValue())))
                { pinching = false; return; }
                float pinch = Vector2.Distance(touch.touches[0].position.ReadValue(),
                    touch.touches[1].position.ReadValue());
                Vector2 midpoint = (touch.touches[0].position.ReadValue() + touch.touches[1].position.ReadValue()) * .5f;
                if (pinching) { delta = (pinch - previousPinch) * .02f; drag = midpoint - previousMidpoint; }
                previousMidpoint = midpoint;
                previousPinch = pinch;
                pinching = true;
            }
            else
            {
                pinching = false;
                if (Mouse.current != null && (hud == null || !hud.BlocksScreenPoint(Mouse.current.position.ReadValue())))
                {
                    delta = Mouse.current.scroll.ReadValue().y * .01f;
                    if (Mouse.current.middleButton.isPressed) drag = Mouse.current.delta.ReadValue();
                }
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    keys.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                    keys.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
                }
            }
            Vector3 shift = new Vector3(keys.x, 0, keys.y).normalized * (6 * Time.deltaTime)
                - new Vector3(drag.x, 0, drag.y) * (.02f * Vector3.Distance(transform.position, focus) / 20f);
            Vector3 nextFocus = focus + shift;
            nextFocus.x = Mathf.Clamp(nextFocus.x, 0, 7);
            nextFocus.z = Mathf.Clamp(nextFocus.z, -1.5f, 2);
            transform.position += nextFocus - focus;
            focus = nextFocus;
            if (Mathf.Approximately(delta, 0f)) return;
            Vector3 offset = transform.position - focus;
            transform.position = focus + offset.normalized *
                Mathf.Clamp(offset.magnitude - delta, minimumDistance, maximumDistance);
        }
    }
}
