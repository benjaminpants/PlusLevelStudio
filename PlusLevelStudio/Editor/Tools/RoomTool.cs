using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class RoomTool : EditorTool
    {
        protected string roomType;
        protected bool inScaleMode = false;
        public override string id => "room_" + roomType;

        public RoomTool(string roomId) : this(roomId, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/room_" + roomId))
        {

        }

        public RoomTool(string roomId, Sprite sprite)
        {
            roomType = roomId;
            this.sprite = sprite;
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            Debug.Log("Cancelled!");
            if (inScaleMode)
            {
                inScaleMode = false;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            inScaleMode = false;
            Debug.Log("Exit!");
        }

        public override bool MousePressed()
        {
            inScaleMode = true;
            return false;
        }

        public override bool MouseReleased()
        {
            return inScaleMode; // if we are in scale mode, return, otherwise, don't
        }

        public override void Update()
        {
            if (inScaleMode)
            {
                Debug.Log("In scale mode!");
            }
            else
            {
                Debug.Log("In regular mode!");
            }
        }
    }
}
