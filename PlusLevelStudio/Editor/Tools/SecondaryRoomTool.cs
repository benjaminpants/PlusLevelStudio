using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class SecondaryRoomTool : RoomTool
    {
        public override string id => base.id + "_secondary";

        public SecondaryRoomTool(string roomId, Sprite sprite) : base(roomId, sprite)
        {
        }

        internal SecondaryRoomTool(string roomId) : base(roomId, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/room_" + roomId + "_secondary"))
        {
        }
    }
}
