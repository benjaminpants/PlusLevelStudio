using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.UI
{
    public class MouseDragDetector : MenuButton
    {
        bool beingDragged = false;
        Vector3 lastDragInvoke = Vector3.negativeInfinity;
        public Action<Vector3> onDrag;

        void Update()
        {
            if (beingDragged)
            {
                Vector3 currentDrag = (CursorController.Instance.cursorTransform.localPosition + CursorController.Instance.rectTransform.localPosition) - transform.localPosition;
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
