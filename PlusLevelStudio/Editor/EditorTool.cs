using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{

    public enum ToolCategory
    {
        Room,
        Character,
        Item,
        Light,
        Structure,
        Tool
    }

    /// <summary>
    /// The base class for all editor tools.
    /// </summary>
    public abstract class EditorTool
    {
        /// <summary>
        /// The ID for this tool, used during the save/load process.
        /// </summary>
        public abstract string id { get; }
        /// <summary>
        /// The sprite for this tool in the editor.
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// Called when the tool is selected/picked up
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Called when the tool is put away/closed
        /// </summary>
        public abstract void Exit();


        /// <summary>
        /// Called when the tool gets cancelled for any reason/the user attempts to cancel the tool.
        /// </summary>
        /// <returns>If the tool should be cancelled/returned when the cancel is performed.</returns>
        public abstract bool Cancelled();

        /// <summary>
        /// Called when the mouse is pressed.
        /// </summary>
        /// <returns>If the tool should be returned.</returns>
        public abstract bool MousePressed();
        /// <summary>
        /// Called when the mouse is released.
        /// </summary>
        /// <returns>If the tool should be returned.</returns>
        public abstract bool MouseReleased();

        /// <summary>
        /// Called when the controller does its standard update
        /// </summary>
        public abstract void Update();
    }
}
