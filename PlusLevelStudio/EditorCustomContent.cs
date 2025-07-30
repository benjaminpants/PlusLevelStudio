using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    /// <summary>
    /// A class containg all custom content for the specified floor.
    /// </summary>
    public class EditorCustomContent
    {
        public BaseGameManager gameManagerPre;

        public void CleanupContent()
        {
            if (gameManagerPre != null)
            {
                GameObject.Destroy(gameManagerPre.gameObject);
            }
        }

        public static EditorCustomContent ReadFrom(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public static void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
