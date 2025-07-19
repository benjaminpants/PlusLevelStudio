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

    public class SettingsComponent : MonoBehaviour, IEditorInteractable
    {
        public IEditorSettingsable activateSettingsOn;
        public Vector3 offset = Vector3.up * 7f;
        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            EditorController.Instance.selector.ShowSettingsSelect(transform.TransformPoint(offset), activateSettingsOn.SettingsClicked);
            return false;
        }

        public bool OnHeld()
        {
            throw new NotImplementedException();
        }

        public void OnReleased()
        {
            throw new NotImplementedException();
        }
    }

    public class LeverVisual : MonoBehaviour
    {
        public Material leverDownMaterial;
        public Material leverUpMaterial;
        public Renderer target;

        public void SetDirection(bool down)
        {
            Texture2D lightMap = (Texture2D)target.material.GetTexture("_LightMap");
            target.material = down ? leverDownMaterial : leverUpMaterial;
            target.material.SetTexture("_LightMap", lightMap);
        }
    }
}
