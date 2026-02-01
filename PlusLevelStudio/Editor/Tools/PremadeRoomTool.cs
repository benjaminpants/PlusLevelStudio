using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PremadeRoomTool : PlaceAndRotateTool
    {
        public override string id => "premaderoom_" + room;
        public string room;
        public int doorId = 0;

        internal PremadeRoomTool(string room) : this(room, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/premaderoom_" + room))
        {

        }

        public PremadeRoomTool(string room, Sprite sprite) : this(room, 0, sprite)
        {

        }

        public PremadeRoomTool(string room, int doorId, Sprite sprite)
        {
            this.room = room;
            this.doorId = doorId;
            this.sprite = sprite;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            PremadeRoomLocation prl = new PremadeRoomLocation()
            {
                position = position,
                direction = dir,
                doorId = doorId,
                room = room
            };
            if (!prl.ValidatePosition(EditorController.Instance.levelData)) return false;
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.premadeRooms.Add(prl);
            EditorController.Instance.RefreshCells();
            SoundPlayOneshot("GrappleClang",0.5f);
            return true;
        }

        public override bool ValidLocation(IntVector2 position)
        {
            return true;
        }
    }
}
