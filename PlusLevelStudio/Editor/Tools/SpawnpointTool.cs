using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class SpawnpointTool : EditorTool
    {
        protected IntVector2? pos;

        public SpawnpointTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/spawnpoint_tool");
        }

        public override string id => "spawnpoint";

        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            pos = null;
        }

        public void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.spawnPoint = pos.Value.ToWorld();
            EditorController.Instance.levelData.spawnDirection = dir;
            EditorController.Instance.UpdateSpawnVisual();
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (pos != null) return false;
            pos = EditorController.Instance.mouseGridPosition;
            EditorController.Instance.selector.SelectRotation(pos.Value, OnPlaced);
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if (pos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
