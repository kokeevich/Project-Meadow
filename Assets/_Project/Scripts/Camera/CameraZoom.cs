using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMeadow.Camera
{
    public sealed class CameraZoom : MonoBehaviour
    {
        [SerializeField] private float minimumDistance = 14f;
        [SerializeField] private float maximumDistance = 25f;
        private ProjectMeadow.UI.FarmHud hud;
        public void Initialize(ProjectMeadow.UI.FarmHud interfaceView) => hud = interfaceView;
        private Vector3 focus = Vector3.zero;
        private float previousPinch;
        private bool pinching;
        private void Update()
        {
            float delta = 0f;
            var touch = Touchscreen.current;
            if (touch != null && touch.touches.Count >= 2 &&
                touch.touches[0].press.isPressed && touch.touches[1].press.isPressed)
            {
                if (hud != null && (hud.BlocksScreenPoint(touch.touches[0].position.ReadValue()) || hud.BlocksScreenPoint(touch.touches[1].position.ReadValue())))
                { pinching = false; return; }
                float pinch = Vector2.Distance(touch.touches[0].position.ReadValue(),
                    touch.touches[1].position.ReadValue());
                if (pinching) delta = (pinch - previousPinch) * 0.02f;
                previousPinch = pinch;
                pinching = true;
            }
            else
            {
                pinching = false;
                if (Mouse.current != null && (hud == null || !hud.BlocksScreenPoint(Mouse.current.position.ReadValue()))) delta = Mouse.current.scroll.ReadValue().y * 0.01f;
            }
            if (Mathf.Approximately(delta, 0f)) return;
            Vector3 offset = transform.position - focus;
            transform.position = focus + offset.normalized *
                Mathf.Clamp(offset.magnitude - delta, minimumDistance, maximumDistance);
        }
    }
}
