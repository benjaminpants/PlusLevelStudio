using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;

namespace PlusLevelStudio.Editor
{
    public class DoorDisplay : MonoBehaviour
    {
        public MeshRenderer sideA;
        public MeshRenderer sideB;

        public virtual void UpdateSides(IntVector2 position, Direction dir)
        {
            IntVector2 posB = position + Directions.ToIntVector2(dir);
            IntVector2 posA = position;
            Texture2D texAtA = EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posA.x, posA.z].roomId).wallTex;
            Texture2D texAtB = EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posB.x, posB.z].roomId).wallTex;
            sideA.material.SetMainTexture(texAtA);
            sideB.material.SetMainTexture(texAtB);
        }
    }

    public class StandardDoorDisplay : DoorDisplay
    {
        public override void UpdateSides(IntVector2 position, Direction dir)
        {
            base.UpdateSides(position, dir);
            IntVector2 posB = position + Directions.ToIntVector2(dir);
            IntVector2 posA = position;
            StandardDoorMats doorMatA = LevelLoaderPlugin.Instance.roomSettings[EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posA.x, posA.z].roomId).roomType].doorMat;
            StandardDoorMats doorMatB = LevelLoaderPlugin.Instance.roomSettings[EditorController.Instance.levelData.RoomFromId(EditorController.Instance.levelData.cells[posB.x, posB.z].roomId).roomType].doorMat;
            MaterialModifier.ChangeOverlay(sideB, doorMatA.shut);
            MaterialModifier.ChangeOverlay(sideA, doorMatB.shut);
        }
    }
}
