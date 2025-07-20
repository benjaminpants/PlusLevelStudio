using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelFormat;

namespace PlusLevelStudio.Editor.Tools
{
    public class RoomTool : EditorTool
    {
        protected string roomType;
        protected bool inScaleMode = false;
        protected IntVector2? startVector = null;
        public override string id => "room_" + roomType;

        internal RoomTool(string roomId) : this(roomId, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/room_" + roomId))
        {

        }

        public RoomTool(string roomId, Sprite sprite)
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
                inScaleMode = false;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            inScaleMode = false;
            EditorController.Instance.selector.DisableSelection();
        }

        public override bool MousePressed()
        {
            startVector = EditorController.Instance.mouseGridPosition;
            inScaleMode = true;
            return false;
        }

        public override bool MouseReleased()
        {
            if (inScaleMode)
            {
                RectInt rect = startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector());
                CellArea areaToAdd;
                EditorRoom edRoomData = null;
                EditorController.Instance.AddUndo();
                if (roomType == "hall")
                {
                    areaToAdd = new RectCellArea(rect.position.ToMystVector(), rect.size.ToMystVector(), 1);
                }
                else
                {
                    edRoomData = EditorController.Instance.levelData.CreateRoomWithDefaultSettings(roomType);
                    EditorController.Instance.SetupVisualsForRoom(edRoomData);
                    areaToAdd = new RectCellArea(rect.position.ToMystVector(), rect.size.ToMystVector(), (ushort)(EditorController.Instance.levelData.rooms.Count + 1));
                }
                if (EditorController.Instance.levelData.AreaValid(areaToAdd))
                {
                    if (edRoomData != null)
                    {
                        EditorController.Instance.levelData.rooms.Add(edRoomData);
                    }
                    EditorController.Instance.levelData.areas.Add(areaToAdd);
                    EditorController.Instance.RefreshCells();
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
                EditorController.Instance.selector.SelectArea(startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector()),null);
            }
            else
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
