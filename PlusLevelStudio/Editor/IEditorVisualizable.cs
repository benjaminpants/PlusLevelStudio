using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public interface IEditorVisualizable
    {
        /// <summary>
        /// Gets the prefab for the visual we want.
        /// </summary>
        /// <returns>The prefab. Return null to be given an empty object</returns>
        GameObject GetVisualPrefab();
        
        /// <summary>
        /// Initializes the visual for the specified visualObject. Called when this visual is first added.
        /// </summary>
        /// <param name="visualObject"></param>
        void InitializeVisual(GameObject visualObject);

        /// <summary>
        /// Updates the visual for the specified visualObject.
        /// </summary>
        /// <param name="visualObject"></param>
        void UpdateVisual(GameObject visualObject);

        /// <summary>
        /// Cleans up the visual for the specified visualObject. Called right before visualObject is destroyed.
        /// Use this to clean up any potential child IEditorVisualizables or stray objects you might've made.
        /// </summary>
        /// <param name="visualObject"></param>
        void CleanupVisual(GameObject visualObject);
    }
}
