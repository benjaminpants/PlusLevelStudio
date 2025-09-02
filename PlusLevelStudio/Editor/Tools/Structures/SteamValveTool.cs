using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class SteamValveTool : EditorTool
    {
        public override string id => "structure_steamvalves";
        public SteamValveLocation valveLocation;
        protected IntVector2? buttonPos;
        bool successfulyPlaced = false;

        public SteamValveTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_steamvalves");
        }

        public override void Begin()
        {
            
        }

        public override void Exit()
        {
            if ((valveLocation != null) && (!successfulyPlaced) && (EditorController.Instance != null))
            {
                if (EditorController.Instance.GetVisual(valveLocation))
                {
                    EditorController.Instance.RemoveVisual(valveLocation);
                }
            }
            successfulyPlaced = false;
            valveLocation = null;
            buttonPos = null;
        }

        public override bool Cancelled()
        {
            if (buttonPos != null)
            {
                buttonPos = null;
                EditorController.Instance.selector.DisableSelection();
                return false;
            }
            if (valveLocation != null)
            {
                EditorController.Instance.RemoveVisual(valveLocation);
                valveLocation = null;
                return false;
            }
            return true;
        }

        public void DirectionSelected(Direction dir)
        {
            valveLocation.valve.position = buttonPos.Value;
            valveLocation.valve.direction = dir;
            if (!valveLocation.valve.ValidatePosition(EditorController.Instance.levelData, false)) return;
            EditorController.Instance.AddUndo();
            EditorController.Instance.AddVisual(valveLocation.valve);
            SteamValveStructureLocation structure = (SteamValveStructureLocation)EditorController.Instance.AddOrGetStructureToData("steamvalves", true);
            structure.valves.Add(valveLocation);
            successfulyPlaced = true;
            valveLocation.deleteAction = structure.OnDeleteValve;
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) == 0) return false;
            if (buttonPos != null) return false;
            if (valveLocation != null) // we are placing the button
            {
                buttonPos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(buttonPos.Value, DirectionSelected);
                return false;
            }
            valveLocation = new SteamValveLocation();
            valveLocation.position = EditorController.Instance.mouseGridPosition;
            EditorController.Instance.AddVisual(valveLocation);
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if ((valveLocation == null) || (buttonPos == null))
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
