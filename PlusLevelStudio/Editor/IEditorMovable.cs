using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public interface IEditorMovable
    {
        /// <summary>
        /// When this object is selected.
        /// </summary>
        void Selected();
        /// <summary>
        /// When this object is unselected.
        /// </summary>
        void Unselected();

        /// <summary>
        /// Called when the arrow is moved.
        /// </summary>
        void MoveUpdate(Vector3? position, Quaternion? rotation);

        /// <summary>
        /// Gets the transform that the handle should go to.
        /// </summary>
        /// <returns></returns>
        Transform GetTransform();

    }

    public class MovableObjectInteraction : MonoBehaviour, IEditorInteractable
    {
        public IEditorMovable target;
        public MoveAxis allowedAxis = MoveAxis.None;
        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            EditorController.Instance.selector.SelectObject(target, allowedAxis);
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
