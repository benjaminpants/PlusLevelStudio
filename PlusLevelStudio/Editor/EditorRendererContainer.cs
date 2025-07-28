using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorRendererContainer : MonoBehaviour
    {
        public List<string> defaultHighlights = new List<string>();
        /// <summary>
        /// Used for changing the material's LightMap for the highlight effect.
        /// </summary>
        public List<Renderer> myRenderers = new List<Renderer>();
        public void AddRendererRange(IEnumerable<Renderer> renders, string defaultHighlight)
        {
            int count = -myRenderers.Count;
            myRenderers.AddRange(renders);
            count += myRenderers.Count;
            for (int i = 0; i < count; i++)
            {
                defaultHighlights.Add(defaultHighlight);
            }
        }

        public void AddRenderer(Renderer render, string defaultHighlight)
        {
            myRenderers.Add(render);
            defaultHighlights.Add(defaultHighlight);
        }

        public void Highlight(string highlight)
        {
            for (int i = 0; i < myRenderers.Count; i++)
            {
                for (int j = 0; j < myRenderers[i].materials.Length; j++)
                {
                    myRenderers[i].materials[j].SetTexture("_LightMap", LevelStudioPlugin.Instance.lightmaps[highlight == "none" ? defaultHighlights[i] : highlight]);
                }
            }
        }
    }
}
