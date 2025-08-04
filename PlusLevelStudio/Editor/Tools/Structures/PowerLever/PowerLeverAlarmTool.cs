using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PowerLeverAlarmTool : EditorTool
    {
        public override string id => "structure_powerlever_alarm";

        public PowerLeverAlarmTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
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
                PowerLeverStructureLocation structure = (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
                PointLocation light = structure.CreateAlarmLight();
                light.position = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.AddVisual(light);
                structure.alarmLights.Add(light);
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
