using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ConveyorBeltTool : EditorTool
    {
        public string type;
        public override string id => "structure_" + type + (placesButton ? "" : "_buttonless");

        IntVector2? startingPos;
        IntVector2? buttonPos;
        ConveyorBeltLocation currentBelt;
        bool holdingBelt = false;
        bool placesButton = false;
        bool placingButton = false;

        internal ConveyorBeltTool(string type, bool placesButton) : this(type, placesButton, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
            
        }

        public ConveyorBeltTool(string type, bool placesButton, Sprite sprite)
        {
            this.type = type;
            this.sprite = sprite;
            this.placesButton = placesButton;
        }

        public override void Begin()
        {
            
        }

        public override void Exit()
        {
            startingPos = null;
            if (holdingBelt)
            {
                EditorController.Instance.RemoveVisual(currentBelt);
            }
            holdingBelt = false;
            currentBelt = null;
            placingButton = false;
            buttonPos = null;
            EditorController.Instance.CancelHeldUndo();

        }

        public override bool Cancelled()
        {
            if (placingButton)
            {
                if (buttonPos.HasValue)
                {
                    buttonPos = null;
                }
                else
                {
                    holdingBelt = true;
                    placingButton = false;
                }
                return false;
            }
            if (holdingBelt)
            {
                holdingBelt = false;
                EditorController.Instance.RemoveVisual(currentBelt);
                currentBelt = null;
                startingPos = null;
                EditorController.Instance.CancelHeldUndo();
                return false;
            }
            return true;
        }

        public void PlaceButton(Direction dir)
        {
            ConveyorBeltStructureLocation belt = (ConveyorBeltStructureLocation)EditorController.Instance.AddOrGetStructureToData("conveyorbelt", false);
            SimpleButtonLocation button = belt.CreateButton();
            button.position = buttonPos.Value;
            button.direction = dir;
            if (!button.ValidatePosition(EditorController.Instance.levelData, false))
            {
                EditorController.Instance.RemoveVisual(belt);
                EditorController.Instance.levelData.structures.Remove(belt);
                EditorController.Instance.selector.SelectRotation(buttonPos.Value, PlaceButton);
                return;
            }
            belt.buttons.Add(button);
            currentBelt.buttonIndex = belt.buttons.IndexOf(button);
            belt.belts.Add(currentBelt);
            EditorController.Instance.AddVisual(button);
            EditorController.Instance.AddHeldUndo();
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (placingButton)
            {
                buttonPos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(buttonPos.Value, PlaceButton);
                return false;
            }
            if (currentBelt == null)
            {
                EditorController.Instance.HoldUndo();
                startingPos = EditorController.Instance.mouseGridPosition;
                ConveyorBeltStructureLocation belt = (ConveyorBeltStructureLocation)EditorController.Instance.AddOrGetStructureToData("conveyorbelt", false);
                currentBelt = belt.CreateBelt();
                currentBelt.direction = Direction.North;
                currentBelt.distance = 1;
                currentBelt.startPosition = startingPos.Value;
                EditorController.Instance.AddVisual(currentBelt);
                holdingBelt = true;
                return false;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            if (holdingBelt)
            {
                if (!currentBelt.ValidatePosition(EditorController.Instance.levelData))
                {
                    Cancelled();
                    return false; // nope
                }
                holdingBelt = false;
                if (!placesButton)
                {
                    ConveyorBeltStructureLocation belt = (ConveyorBeltStructureLocation)EditorController.Instance.AddOrGetStructureToData("conveyorbelt", true);
                    belt.belts.Add(currentBelt);
                    EditorController.Instance.AddHeldUndo();
                    return true;
                }
                placingButton = true;
                return false;
            }
            return false;
        }

        public override void Update()
        {
            if (placingButton)
            {
                if (!buttonPos.HasValue)
                {
                    EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
                }
                return;
            }
            if (startingPos.HasValue)
            {
                EditorController.Instance.selector.SelectTile(startingPos.Value);
            }
            else
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
            if (holdingBelt)
            {
                IntVector2 mousePos = EditorController.Instance.mouseGridPosition;
                Direction targetDirection = currentBelt.direction;
                if (mousePos != startingPos.Value)
                {
                    targetDirection = Directions.DirFromVector3(new Vector3(mousePos.x - startingPos.Value.x, 0f, mousePos.z - startingPos.Value.z), 45f);
                }

                IntVector2 finalOff = mousePos.LockAxis(startingPos.Value, targetDirection) - startingPos.Value;
                currentBelt.direction = targetDirection;
                byte distance = (byte)(Mathf.Abs(finalOff.x + finalOff.z) + 1);
                currentBelt.distance = distance;
                EditorController.Instance.UpdateVisual(currentBelt);
            }
        }
    }
}
