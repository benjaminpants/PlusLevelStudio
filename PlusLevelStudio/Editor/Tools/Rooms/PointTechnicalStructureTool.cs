using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    public class PointTechnicalStructureTool : EditorTool
    {
        public string type;
        public override string id => "technical_" + type;

        internal PointTechnicalStructureTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/technical_" + type))
        {
        }

        public PointTechnicalStructureTool(string type, Sprite sprite)
        {
            this.sprite = sprite;
            this.type = type;
        }

        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {

        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.AddUndo();
                RoomTechnicalStructurePoint point = (RoomTechnicalStructurePoint)EditorController.Instance.AddOrGetStructureToData("technical_" + type, false);
                point.position = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.UpdateVisual(point);
                EditorController.Instance.RefreshLights();
                return true;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
        }
    }
}
