using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.UI
{
    public class MouseDragDetector : MenuButton
    {
        Transform containingCanvas;
        bool beingDragged = false;
        Vector3 lastDragInvoke = Vector3.negativeInfinity;
        public Action<Vector3> onDrag;

        Vector3 PositionRelativeToCanvas()
        {
            return containingCanvas.InverseTransformPoint(transform.position);
        }

        void Start()
        {
            containingCanvas = transform.parent;
            while (containingCanvas.GetComponent<Canvas>() == null)
            {
                containingCanvas = containingCanvas.parent;
            }
        }

        void Update()
        {
            if (containingCanvas == null) return;
            if (beingDragged)
            {
                Vector3 currentDrag = (CursorController.Instance.cursorTransform.localPosition + CursorController.Instance.rectTransform.localPosition) - PositionRelativeToCanvas();
                if (lastDragInvoke != currentDrag)
                {
                    onDrag.Invoke(currentDrag);
                    lastDragInvoke = currentDrag;
                }
            }
            else
            {
                lastDragInvoke = Vector3.negativeInfinity;
            }
        }

        public override void Press()
        {
            beingDragged = true;
        }

        public override void UnHold()
        {
            beingDragged = false;
        }
    }
}
