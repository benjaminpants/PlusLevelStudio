using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class SettingsWorldButton : MonoBehaviour, IEditorInteractable
    {
        public Action clickedAction;

        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            Debug.Log("Settings Gear Clicked!");
            if (clickedAction != null)
            {
                clickedAction();
            }
            return false;
        }

        public bool OnHeld()
        {
            throw new NotImplementedException();
        }

        public void OnReleased()
        {
            throw new NotImplementedException();
        }
    }
}
