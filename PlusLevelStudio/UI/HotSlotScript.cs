using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class HotSlotScript : MonoBehaviour
    {
        public int slotIndex = 0;
        private EditorTool _currentTool;
        public Image iconImage;
        public EditorTool currentTool {
            get
            {
                return _currentTool;
            }
            set
            {
                _currentTool = value;
                if (_currentTool == null)
                {
                    iconImage.sprite = null;
                    return;
                }
                iconImage.sprite = _currentTool.sprite;
            }
        }
        public StandardMenuButton button;

        void Start()
        {
            button.OnPress.AddListener(ButtonPressed);
        }

        void ButtonPressed()
        {
            if (currentTool == null) return;
            EditorController.Instance.SwitchToTool(currentTool);
        }
    }
}
