using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public interface IEditorInteractable
    {
        /// <summary>
        /// Gets called when this EditorInteractable is clicked
        /// </summary>
        /// <returns>If this object can be held. If false, then OnHeld and OnReleased won't be called.</returns>
        bool OnClicked();

        /// <summary>
        /// Gets called when this EditorInteractable is continually held.
        /// </summary>
        /// <returns>Whether or not the held action should be called again.</returns>
        bool OnHeld();

        /// <summary>
        /// Gets called when this EditorInteractable is released
        /// </summary>
        /// <returns></returns>
        void OnReleased();

        /// <summary>
        /// Gets called when this EditorInteractable is clicked on when a tool is active.
        /// </summary>
        /// <returns>Whether this interactable can be clicked on when a tool is active.</returns>
        bool InteractableByTool(EditorTool tool);
    }
}
