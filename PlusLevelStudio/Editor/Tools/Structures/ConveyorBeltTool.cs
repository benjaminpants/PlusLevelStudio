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
        bool successfullyPlaced = false;
        ConveyorBeltStructureLocation currentStructure;

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
            SoundStopLooping();
            startingPos = null;
            if ((currentBelt != null) && !successfullyPlaced)
            {
                EditorController.Instance.RemoveVisual(currentBelt);
            }
            successfullyPlaced = false;
            holdingBelt = false;
            currentBelt = null;
            placingButton = false;
            buttonPos = null;
            if (currentStructure != null)
            {
                EditorController.Instance.levelData.ValidatePlacements(true); // hack kind of, should probably only force a revalidation on the one we are targetting
            }
            currentStructure = null;
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
                SoundStopLooping();
                holdingBelt = false;
                EditorController.Instance.RemoveVisual(currentBelt);
                currentBelt = null;
                startingPos = null;
                if (currentStructure != null)
                {
                    EditorController.Instance.levelData.ValidatePlacements(true); // hack kind of, should probably only force a revalidation on the one we are targetting
                }
                currentStructure = null;
                EditorController.Instance.CancelHeldUndo();
                return false;
            }
            return true;
        }

        public void PlaceButton(Direction dir)
        {
            SimpleButtonLocation button = currentStructure.CreateButton();
            button.position = buttonPos.Value;
            button.direction = dir;
            if (!button.ValidatePosition(EditorController.Instance.levelData, false))
            {
                //EditorController.Instance.RemoveVisual(currentStructure);
                //EditorController.Instance.levelData.structures.Remove(currentStructure);
                EditorController.Instance.selector.SelectRotation(buttonPos.Value, PlaceButton);
                return;
            }
            currentStructure.buttons.Add(button);
            currentBelt.buttonIndex = currentStructure.buttons.IndexOf(button);
            currentStructure.belts.Add(currentBelt);
            EditorController.Instance.AddVisual(button);
            EditorController.Instance.AddHeldUndo();
            successfullyPlaced = true;
            EditorController.Instance.SwitchToTool(null);
            SoundPlayOneshot("Sfx_Button_Press");
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
                SoundPlayLooping("ConveyorBeltLoop");
                EditorController.Instance.HoldUndo();
                startingPos = EditorController.Instance.mouseGridPosition;
                currentStructure = (ConveyorBeltStructureLocation)EditorController.Instance.AddOrGetStructureToData("conveyorbelt", false);
                currentBelt = currentStructure.CreateBelt();
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
                SoundStopLooping();
                if (!placesButton)
                {
                    currentStructure.belts.Add(currentBelt);
                    EditorController.Instance.AddHeldUndo();
                    successfullyPlaced = true;
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
                SoundPitchLooping(distance / 10f);
                EditorController.Instance.UpdateVisual(currentBelt);
            }
        }
    }
}
