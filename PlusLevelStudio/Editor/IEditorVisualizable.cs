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

        void InitializeVisual(GameObject visualObject);

        void UpdateVisual(GameObject visualObject);

        void CleanupVisual(GameObject visualObject);
    }
}
