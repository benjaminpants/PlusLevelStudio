using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class RoomTool : EditorTool
    {
        protected string roomType;

        public override string id => "room_" + roomType;

        public RoomTool(string roomId)
        {
            roomType = roomId;
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override bool MousePressed()
        {
            return true;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            
        }
    }
}
