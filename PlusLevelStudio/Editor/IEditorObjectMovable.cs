using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    /// <summary>
    ///  An interface for objects that need to be moved.
    /// </summary>
    public interface IEditorObjectMovable
    {
        /// <summary>
        /// Called with true when first interacted with and called with false when unselected.
        /// </summary>
        /// <param name="highlight"></param>
        void MoveHighlight(bool highlight);
        /// <summary>
        /// Called when one of the move arrow handles is clicked.
        /// </summary>
        void MoveStart();
        /// <summary>
        /// Called whenever one of the move arrow handles is updated.
        /// </summary>
        /// <param name="moveBy"></param>
        void Move(Vector3 moveBy);
        /// <summary>
        /// Called when one of move arrow handles is released.
        /// </summary>
        void MoveEnd();

        /// <summary>
        /// Gets the axis aligned bounding box for this object
        /// </summary>
        /// <returns></returns>
        Bounds GetBounds();
    }

    public class MovableObjectInteraction : MonoBehaviour, IEditorInteractable
    {
        public IEditorObjectMovable target;
        public SelectorObjectFlags flags;
        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            EditorController.Instance.selector.SelectObject(target, flags);
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
