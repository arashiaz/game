using UnityEngine;
using UnityEngine.EventSystems;

namespace Wildlands
{
    public sealed class TouchControls : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public bool LookPad;
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        private int pointer = int.MinValue;
        private Vector2 origin;
        private RectTransform knob;
        public void SetKnob(RectTransform value) { knob = value; }
        public void OnPointerDown(PointerEventData data)
        {
            if (pointer != int.MinValue) return;
            pointer = data.pointerId;
            origin = data.position;
            Move = Vector2.zero;
            Look = Vector2.zero;
        }
        public void OnDrag(PointerEventData data)
        {
            if (data.pointerId != pointer) return;
            if (LookPad) Look += data.delta;
            else
            {
                float scale = GetComponentInParent<Canvas>().scaleFactor;
                Move = Vector2.ClampMagnitude((data.position - origin) / (65 * scale), 1);
                if (knob) knob.anchoredPosition = Move * 42;
            }
        }
        public void OnPointerUp(PointerEventData data)
        {
            if (data.pointerId == pointer) ResetInput();
        }
        public Vector2 ConsumeLook() { var delta = Look; Look = Vector2.zero; return delta; }
        private void OnDisable() { ResetInput(); }
        private void OnApplicationFocus(bool focus) { if (!focus) ResetInput(); }
        private void ResetInput()
        {
            Move = Look = Vector2.zero;
            pointer = int.MinValue;
            if (knob) knob.anchoredPosition = Vector2.zero;
        }
    }
}
