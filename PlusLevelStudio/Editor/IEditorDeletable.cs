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
        /// <summary>
        /// Used for changing the material's LightMap for the highlight effect.
        /// </summary>
        public List<Renderer> myRenderers = new List<Renderer>();
        public bool OnDelete(EditorLevelData data)
        {
            return toDelete.OnDelete(data);
        }

        public void Highlight(string highlight)
        {
            for (int i = 0; i < myRenderers.Count; i++)
            {
                for (int j = 0; j < myRenderers[i].materials.Length; j++)
                {
                    myRenderers[i].materials[j].SetTexture("_LightMap", LevelStudioPlugin.Instance.lightmaps[highlight]);
                }
            }
        }
    }
}
