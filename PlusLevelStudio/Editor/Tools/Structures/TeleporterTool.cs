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
        EditorRoom currentRoom = null;

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
                currentRoom = null;
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
            currentRoom = null;
        }

        void MachineDirectionClicked(Direction dir)
        {
            machine = new TeleporterMachineLocation();
            Vector3 worldPos = currentMachinePos.Value.ToWorld();
            machine.position = new Vector2(worldPos.x, worldPos.z);
            machine.direction = dir.GetOpposite().ToRotation().eulerAngles.y;
            EditorController.Instance.AddVisual(machine);
            currentRoom = EditorController.Instance.levelData.RoomFromPos(currentMachinePos.Value, true);
        }

        void ButtonsDirectionClicked(Direction dir)
        {
            EditorController.Instance.AddUndo();
            TeleporterStructureLocation structure = (TeleporterStructureLocation)EditorController.Instance.AddOrGetStructureToData("teleporters", true);
            TeleporterLocation location = new TeleporterLocation();
            Vector3 worldPos = currentButtonsPos.Value.ToWorld();
            location.position = new Vector2(worldPos.x, worldPos.z);
            location.direction = dir.GetOpposite().ToRotation().eulerAngles.y;
            location.machine = machine;
            machine.myTeleporter = location;
            location.myStructure = structure;
            structure.teleporters.Add(location);
            EditorController.Instance.AddVisual(location);
            successfullyPlaced = true;
            EditorController.Instance.SwitchToTool(null);
        }

        bool PositionValid(IntVector2 position)
        {
            EditorRoom room = EditorController.Instance.levelData.RoomFromPos(position, true);
            if (!TeleporterLocation.RoomValid(room)) return false;
            StructureLocation structure = EditorController.Instance.GetStructureData("teleporters");
            if (structure == null) return true;
            TeleporterStructureLocation teleStructure = (TeleporterStructureLocation)structure;
            for (int i = 0; i < teleStructure.teleporters.Count; i++)
            {
                IntVector2 telePos = new Vector3(teleStructure.teleporters[i].position.x, 0f, teleStructure.teleporters[i].position.y).ToCellVector();
                // we only need to check the first one, as if they were in different rooms they would've been destroyed.
                if (EditorController.Instance.levelData.RoomFromPos(telePos, true) == room)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool MousePressed()
        {
            if (currentMachinePos == null)
            {
                if (!PositionValid(EditorController.Instance.mouseGridPosition)) return false;
                currentMachinePos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(currentMachinePos.Value, MachineDirectionClicked);
                return false;
            }
            if (currentButtonsPos == null)
            {
                if (currentRoom != EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true)) return false;
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
