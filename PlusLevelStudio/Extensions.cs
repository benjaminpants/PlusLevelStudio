using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorExtensions
    {

        /// <summary>
        /// Converts a given position to an int vector corresponding to a cell position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static IntVector2 ToCellVector(this Vector3 position)
        {
            float x = (position.x - 5f) / 10f;
            float y = (position.z - 5f) / 10f;
            return new IntVector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        /// <summary>
        /// Converts a given IntVector to a world position
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 ToWorld(this IntVector2 vector)
        {
            return new Vector3((vector.x * 10f + 5f), 0f, (vector.z * 10f + 5f));
        }
    }
}
