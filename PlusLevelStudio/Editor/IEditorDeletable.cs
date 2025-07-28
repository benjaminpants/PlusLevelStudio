using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public interface IEditorDeletable
    {
        /// <summary>
        /// What should be done when this item is deleted by the delete tool.
        /// </summary>
        /// <returns>Whether the deletion was successful.</returns>
        bool OnDelete(EditorLevelData data);
    }

    /// <summary>
    /// The monobehavior that is searched for by the delete tool.
    /// </summary>
    public class EditorDeletableObject : MonoBehaviour
    {
        public IEditorDeletable toDelete;
        public EditorRendererContainer renderContainer;

        public bool OnDelete(EditorLevelData data)
        {
            return toDelete.OnDelete(data);
        }
        public void Highlight(string highlight)
        {
            renderContainer.Highlight(highlight);
        }
    }
}
