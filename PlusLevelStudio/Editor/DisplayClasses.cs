using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class DoorDisplay : MonoBehaviour
    {
        public MeshRenderer sideA;
        public MeshRenderer sideB;

        public virtual void UpdateSides(IntVector2 position, Direction dir)
        {
            IntVector2 posA = position + Directions.ToIntVector2(dir);
            IntVector2 posB = position - Directions.ToIntVector2(dir);
            Texture2D texAtA = EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posA.x, posA.z].roomId).wallTex;
            Texture2D texAtB = EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posB.x, posB.z].roomId).wallTex;
        }
    }
}
