using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class VentTool : EditorTool
    {
        public string type;
        public override string id => "structure_" + type;
        internal VentTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
        }

        public VentTool(string type, Sprite sprite)
        {
            this.sprite = sprite;
            this.type = type;
        }

        protected IntVector2? pos;
        protected VentLocation currentVent;

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            if (currentVent != null)
            {
                currentVent.positions.RemoveAt(currentVent.positions.Count - 1);
                if (currentVent.positions.Count == 0)
                {
                    EditorController.Instance.RemoveVisual(currentVent);
                    EditorController.Instance.RefreshCells(true);
                    currentVent = null;
                    EditorController.Instance.gridManager.Height = 0f;
                    EditorController.Instance.selector.SelectRotation(pos.Value, OnDirectionSelected);
                }
                else
                {
                    GetCurrentVentVisual().BuildModel(currentVent.positions, currentVent.direction); // rebuild model with the position now removed
                }
                return false;
            }    
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            EditorController.Instance.gridManager.Height = 0f;
            pos = null;
            if (currentVent != null)
            {
                EditorController.Instance.RemoveVisual(currentVent);
                EditorController.Instance.RefreshCells(true);
                currentVent = null;
            }
        }

        protected void OnDirectionSelected(Direction dir)
        {
            if (EditorController.Instance.levelData.GetCellSafe(pos.Value + dir.ToIntVector2()) == null) return;
            EditorController.Instance.selector.DisableSelection(); // to prevent a brief flicker when we change the grid height
            currentVent = new VentLocation();
            currentVent.positions.Add(pos.Value + dir.ToIntVector2());
            currentVent.direction = dir;
            EditorController.Instance.AddVisual(currentVent);
            currentVent.ModifyCells(EditorController.Instance.levelData, true); // hacky? maybe? nothing should refresh the cells while we have the tool here
            EditorController.Instance.RefreshCells(true,false);
            EditorController.Instance.gridManager.Height = VentVisualManager.height - 5f;
            GetCurrentVentVisual().BuildModel(currentVent.positions, currentVent.direction);
            SoundPlayOneshot("Vent_Vacuum");
        }

        protected VentVisualManager GetCurrentVentVisual()
        {
            if (currentVent == null) return null;
            return EditorController.Instance.GetVisual(currentVent).GetComponent<VentVisualManager>();
        }

        public bool AttemptToAddPosition(IntVector2 position)
        {
            IntVector2 lastPosition = currentVent.positions[currentVent.positions.Count - 1];
            IntVector2? secondToLastPosition = null;
            if ((currentVent.positions.Count - 2) >= 0)
            {
                secondToLastPosition = currentVent.positions[currentVent.positions.Count - 2];
            }
            if (position == lastPosition) return false; // cant place at same location as previous
            if ((position.x != lastPosition.x) && (position.z != lastPosition.z)) return false; // too many axis changes
            if (EditorController.Instance.levelData.GetCellSafe(position) == null) return false; // OUT OF BOUNDS
            if ((secondToLastPosition == null) || ((position.x != secondToLastPosition.Value.x) && (position.z != secondToLastPosition.Value.z)))
            {
                currentVent.positions.Add(position);
            }
            else
            {
                currentVent.positions[currentVent.positions.Count - 1] = position;
            }
            return true;
        }

        public override bool MousePressed()
        {
            if (currentVent != null)
            {
                if ((EditorController.Instance.mouseGridPosition == currentVent.positions[currentVent.positions.Count - 1]) && (EditorController.Instance.mouseGridPosition != currentVent.positions[0]))
                {
                    EditorController.Instance.AddUndo();
                    VentStructureLocation ventStructure = (VentStructureLocation)EditorController.Instance.AddOrGetStructureToData("vent", true);
                    currentVent.owner = ventStructure;
                    ventStructure.ventLocations.Add(currentVent);
                    GetCurrentVentVisual().CleanupInsideVisuals();
                    EditorController.Instance.RefreshCells();
                    EditorController.Instance.UpdateVisual(ventStructure);
                    currentVent = null;
                    SoundPlayOneshot("Doors_Locker");
                    return true;
                }
                else
                {
                    if (AttemptToAddPosition(EditorController.Instance.mouseGridPosition))
                    {
                        SoundPlayOneshot("VentHit_" + UnityEngine.Random.Range(0, 4));
                    }
                    else
                    {
                        SoundPlayOneshot("Activity_Incorrect");
                    }
                    GetCurrentVentVisual().BuildModel(currentVent.positions, currentVent.direction);
                }
                return false;
            }
            if (pos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                pos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(pos.Value, OnDirectionSelected);
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
            if ((pos == null) || (currentVent != null))
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
