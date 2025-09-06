using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class TeleporterTool : EditorTool
    {
        public override string id => "structure_teleporter";
        TeleporterMachineLocation machine;
        bool successfullyPlaced = false;
        IntVector2? currentMachinePos;
        IntVector2? currentButtonsPos;

        public TeleporterTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_teleporter");
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            if (currentButtonsPos != null)
            {
                EditorController.Instance.selector.DisableSelection();
                currentButtonsPos = null;
                return false;
            }
            if (machine != null)
            {
                EditorController.Instance.RemoveVisual(machine);
                machine = null;
                currentMachinePos = null;
                return false;
            }
            if (currentMachinePos != null)
            {
                EditorController.Instance.selector.DisableSelection();
                currentMachinePos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            if (!successfullyPlaced)
            {
                if (machine != null)
                {
                    EditorController.Instance.RemoveVisual(machine);
                }
            }
            machine = null;
            currentMachinePos = null;
            currentButtonsPos = null;
            successfullyPlaced = false;
        }

        void MachineDirectionClicked(Direction dir)
        {
            machine = new TeleporterMachineLocation();
            Vector3 worldPos = currentMachinePos.Value.ToWorld();
            machine.position = new Vector2(worldPos.x, worldPos.z);
            machine.direction = dir.GetOpposite().ToRotation().eulerAngles.y;
            EditorController.Instance.AddVisual(machine);
        }

        void ButtonsDirectionClicked(Direction dir)
        {
            EditorController.Instance.AddUndo();
            TeleporterStructureLocation structure = (TeleporterStructureLocation)EditorController.Instance.AddOrGetStructureToData("teleporters", true);
            TeleporterLocation location = new TeleporterLocation();
            Vector3 worldPos = currentButtonsPos.Value.ToWorld();
            location.position = new Vector2(worldPos.x, worldPos.z);
            location.direction = dir.ToRotation().eulerAngles.y;
            location.machine = machine;
            machine.myTeleporter = location;
            location.myStructure = structure;
            structure.teleporters.Add(location);
            EditorController.Instance.AddVisual(location);
            successfullyPlaced = true;
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (currentMachinePos == null)
            {
                currentMachinePos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(currentMachinePos.Value, MachineDirectionClicked);
                return false;
            }
            if (currentButtonsPos == null)
            {
                currentButtonsPos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(currentButtonsPos.Value, ButtonsDirectionClicked);
                return false;
            }
            return true;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if ((currentMachinePos == null) || ((machine != null) && (currentButtonsPos == null)))
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
