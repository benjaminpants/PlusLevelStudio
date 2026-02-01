using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelFormat;
using System.Linq;

namespace PlusLevelStudio.Editor.Tools
{
    public class OnlyOneRoomTool : EditorTool
    {
        protected string roomType;
        protected bool inScaleMode = false;
        protected IntVector2? startVector = null;
        public override string id => "room_" + roomType;

        internal OnlyOneRoomTool(string roomId) : this(roomId, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/room_" + roomId))
        {

        }

        public OnlyOneRoomTool(string roomId, Sprite sprite)
        {
            roomType = roomId;
            this.sprite = sprite;
        }

        public override void Begin()
        {
            startVector = null;
        }

        public override bool Cancelled()
        {
            if (inScaleMode)
            {
                SoundStopLooping();
                inScaleMode = false;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            inScaleMode = false;
            EditorController.Instance.selector.DisableSelection();
            SoundStopLooping();
        }

        public override bool MousePressed()
        {
            startVector = EditorController.Instance.mouseGridPosition;
            inScaleMode = true;
            SoundPlayLooping("LockdownDoor_Move");
            return false;
        }

        public override bool MouseReleased()
        {
            if (inScaleMode)
            {
                RectInt rect = startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector());
                CellArea areaToAdd;
                EditorRoom edRoomData = EditorController.Instance.levelData.rooms.FirstOrDefault(x => x.roomType == roomType);
                EditorController.Instance.AddUndo();
                bool addedNew = false;
                if (edRoomData == null)
                {
                    edRoomData = EditorController.Instance.levelData.CreateRoomWithDefaultSettings(roomType);
                    EditorController.Instance.SetupVisualsForRoom(edRoomData);
                    areaToAdd = new RectCellArea(rect.position.ToMystVector(), rect.size.ToMystVector(), (ushort)(EditorController.Instance.levelData.rooms.Count + 1));
                    addedNew = true;
                }
                else
                {
                    areaToAdd = new RectCellArea(rect.position.ToMystVector(), rect.size.ToMystVector(), (ushort)(EditorController.Instance.levelData.rooms.IndexOf(edRoomData) + 1));
                }
                if (EditorController.Instance.levelData.AreaValid(areaToAdd))
                {
                    if (addedNew)
                    {
                        EditorController.Instance.levelData.rooms.Add(edRoomData);
                    }
                    EditorController.Instance.levelData.areas.Add(areaToAdd);
                    EditorController.Instance.RefreshCells();
                    SoundPlayOneshot("GrappleClang",0.5f);
                    return true;
                }
                Cancelled(); // go back
                return false;
            }
            return false;
        }

        public override void Update()
        {
            if (inScaleMode)
            {
                if (startVector == null) throw new InvalidOperationException();
                RectInt rect = startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector());
                SoundPitchLooping((rect.width + rect.height) / 12f);
                EditorController.Instance.selector.SelectArea(rect, null);
            }
            else
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
