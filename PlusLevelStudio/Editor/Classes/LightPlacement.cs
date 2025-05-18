using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class LightPlacement : IEditorVisualizable
    {
        public IntVector2 position;
        public ushort lightGroup;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return null;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            
        }

        public void UpdateVisual(GameObject visualObject)
        {
            
        }
    }

    public class LightGroup
    {
        public Color color = Color.white;
        public int strength = 10;
    }
}
