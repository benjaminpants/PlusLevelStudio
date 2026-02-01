using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ElevatorTool : PlaceAndRotateTool
    {
        public string type;
        public bool isSpawn = false;
        public override string id => "exit_" + type + (isSpawn ? "_start" : "");
        internal ElevatorTool(string type, bool isSpawn) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/exit_" + type + (isSpawn ? "_start" : "")), isSpawn)
        {
        }
        public ElevatorTool(string type, Sprite sprite, bool isSpawn)
        {
            this.type = type;
            this.sprite = sprite;
            this.isSpawn = isSpawn;
        }

        public override void Update()
        {
            // ACK HACK!
            if ((EditorController.Instance.levelData.exits.Find(x => x.isSpawn) != null) && isSpawn)
            {
                EditorController.Instance.SwitchToTool(null);
            }
            base.Update();
        }

        public override bool ValidLocation(IntVector2 position)
        {
            return EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) == 0;
        }

        protected override void ValidLocationClicked()
        {
            SoundPlayOneshot("Elv_Open_Real");
            base.ValidLocationClicked();
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            ExitLocation exitLocal = new ExitLocation();
            exitLocal.position = position;
            exitLocal.direction = dir;
            exitLocal.type = "elevator";
            exitLocal.isSpawn = isSpawn;
            if (!exitLocal.ValidatePosition(EditorController.Instance.levelData)) return false;
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.exits.Add(exitLocal);
            EditorController.Instance.AddVisual(exitLocal);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            SoundPlayOneshot("Elv_Close_Real");
            return true;
        }
    }
}
