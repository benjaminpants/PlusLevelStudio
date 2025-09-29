using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio
{
    public class EditorCursorController : CursorController
    {
        public Image toolIcon;

        public void SetIcon(Sprite sprite)
        {
            if (sprite == null)
            {
                toolIcon.enabled = false;
                return;
            }
            toolIcon.enabled = true;
            toolIcon.sprite = sprite;
        }
    }
}
