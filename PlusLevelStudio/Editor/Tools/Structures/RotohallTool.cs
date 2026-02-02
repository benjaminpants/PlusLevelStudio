using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class RotohallTool : EditorTool
    {
        public override string id => "structure_rotohall";

        IntVector2? positionChosen = null;
        List<Direction> chosenDirections = new List<Direction>();
        RotohallSimpleLocation rotohall;

        bool placingButton = false;
        IntVector2? buttonPositionChosen = null;

        public RotohallTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_rotohall");
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            if (placingButton)
            {
                if (buttonPositionChosen.HasValue)
                {
                    buttonPositionChosen = null;
                    return false;
                }
                placingButton = false;
                for (int i = 0; i < chosenDirections.Count; i++)
                {
                    EditorController.Instance.selector.SetArrowClickable(chosenDirections[i], false);
                }
                EditorController.Instance.selector.SelectRotation(positionChosen.Value, RotoDirectionClicked);
            }
            if (chosenDirections.Count > 0)
            {
                EditorController.Instance.selector.SetArrowClickable(chosenDirections[chosenDirections.Count - 1], true);
                chosenDirections.RemoveAt(chosenDirections.Count - 1);
                if (rotohall != null)
                {
                    EditorController.Instance.RemoveVisual(rotohall);
                    rotohall = null;
                }
                return false;
            }
            if (positionChosen.HasValue)
            {
                positionChosen = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            placingButton = false;
            if (rotohall != null)
            {
                EditorController.Instance.RemoveVisual(rotohall);
                rotohall = null;
            }
            chosenDirections.Clear();
            positionChosen = null;
            buttonPositionChosen = null;
            EditorController.Instance.selector.ResetAllArrowClickableStatus();
        }

        public void RotoDirectionClicked(Direction dir)
        {
            chosenDirections.Add(dir);
            EditorController.Instance.selector.SetArrowClickable(dir, false);
            EditorController.Instance.selector.SelectRotation(positionChosen.Value, RotoDirectionClicked);
            if (chosenDirections.Count == 2)
            {
                // time to calculate what type we'll be using
                string typeToSpawn = "rotohall_corner";
                if (chosenDirections[0].GetOpposite() == chosenDirections[1])
                {
                    typeToSpawn = "rotohall_straight";
                }
                // figure out what direction we want
                Direction desiredDirection = chosenDirections[0];
                if (typeToSpawn == "rotohall_corner")
                {
                    while (true)
                    {
                        if ((desiredDirection == chosenDirections[0]) && ((desiredDirection.RotatedRelativeToNorth(Direction.East) == chosenDirections[1]))) break;
                        if ((desiredDirection == chosenDirections[1]) && ((desiredDirection.RotatedRelativeToNorth(Direction.East) == chosenDirections[0]))) break;
                        desiredDirection = desiredDirection.RotatedRelativeToNorth(Direction.East);
                    }
                }
                rotohall = new RotohallSimpleLocation() {
                    position = positionChosen.Value,
                    direction = desiredDirection,
                    prefab = typeToSpawn,
                    clockwise = false
                };
                EditorController.Instance.AddVisual(rotohall);

                SoundPlayOneshot("ShrinkMachine_Door");

                // rotohalls literally do not care about clockwise vs counter clockwise apparently. did not notice
                /*Direction theoreticalClockwise = chosenDirections[0].RotatedRelativeToNorth(Direction.West);
                rotohall.clockwise = chosenDirections[2] == theoreticalClockwise;*/ // if the next direction clicked matches what the next direction would be clockwise, then we are indeed rotating clockwise. otherwise, counter clockwise.
                placingButton = true;
                EditorController.Instance.selector.ResetAllArrowClickableStatus();
            }
        }

        public void ButtonDirectionClicked(Direction dir)
        {
            if (!EditorController.Instance.levelData.WallFree(buttonPositionChosen.Value, dir, false)) return;
            EditorController.Instance.AddUndo();
            RotohallStructureLocation rotohallStr = (RotohallStructureLocation)EditorController.Instance.AddOrGetStructureToData("rotohall", true);
            rotohallStr.rotohalls.Add(rotohall);
            rotohall.deleteAction = rotohallStr.DeleteRotohall;
            EditorController.Instance.AddVisual(rotohall.SetButton(buttonPositionChosen.Value, dir));
            rotohall = null;
            EditorController.Instance.SwitchToTool(null);
            SoundPlayOneshot("Sfx_Button_Press");
        }

        public override bool MousePressed()
        {
            if (placingButton)
            {
                if (!buttonPositionChosen.HasValue)
                {
                    if ((EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) == 0)) return false;
                    buttonPositionChosen = EditorController.Instance.mouseGridPosition;
                    EditorController.Instance.selector.SelectRotation(buttonPositionChosen.Value, ButtonDirectionClicked);
                }
            }
            if (!positionChosen.HasValue)
            {
                if ((EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) == 0)) return false;
                positionChosen = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(positionChosen.Value, RotoDirectionClicked);
                return false;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if ((!positionChosen.HasValue) || (placingButton && (!buttonPositionChosen.HasValue)))
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
