using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class SpawnpointTool : PlaceAndRotateTool
    {
        public override string id => "spawnpoint";
        public SpawnpointTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/spawnpoint_tool");
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.spawnPoint = pos.Value.ToWorld() + Vector3.up * 5f;
            EditorController.Instance.levelData.spawnDirection = dir;
            EditorController.Instance.UpdateSpawnVisual();
            return true;
        }
        public override bool ValidLocation(IntVector2 position)
        {
            return true;
        }
    }
}
