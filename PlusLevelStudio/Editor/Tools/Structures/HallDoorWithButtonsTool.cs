using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    public class HallDoorWithButtonsTool : EditorTool
    {
        protected string type;
        protected string prefabType;
        public override string id => "structure_" + (string.IsNullOrEmpty(prefabType) ? type : prefabType);
        protected IntVector2? firstPos;
        protected bool firstPlaced = false;
        public SimpleLocation first;
        protected IntVector2? buttonPos;

        internal HallDoorWithButtonsTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {

        }

        internal HallDoorWithButtonsTool(string structureType, string prefabType) : this(structureType, prefabType, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + prefabType))
        {

        }

        public HallDoorWithButtonsTool(string type, Sprite sprite)
        {
            this.type = type;
            prefabType = string.Empty;
            this.sprite = sprite;
        }

        public HallDoorWithButtonsTool(string structureType, string prefabType, Sprite sprite)
        {
            type = structureType;
            this.sprite = sprite;
            this.prefabType = prefabType;
        }

        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            if (buttonPos != null)
            {
                buttonPos = null;
                return false;
            }
            if (firstPlaced)
            {
                EditorController.Instance.RemoveVisual(first);
                first = null;
                firstPlaced = false;
                firstPos = null;
                return false;
            }
            if (firstPos != null)
            {
                firstPos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            firstPos = null;
            buttonPos = null;
            if (first != null)
            {
                EditorController.Instance.RemoveVisual(first);
                first = null;
            }
            firstPlaced = false;
        }

        public virtual void PlaceFirst(Direction dir)
        {
            PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(firstPos.Value);
            if (cell == null) return; // cell doesn't exist
            if (cell.type == 16) return; // the cell is empty
            HallDoorStructureLocationWithButtons structure = (HallDoorStructureLocationWithButtons)EditorController.Instance.AddOrGetStructureToData(type, true);
            SimpleLocation local = structure.CreateNewChild();
            local.position = firstPos.Value;
            local.direction = dir;
            ModifyChild(local);
            EditorController.Instance.AddVisual(local);
            first = local;
            firstPlaced = true;
            EditorController.Instance.selector.DisableSelection();
            PlayPlaceSound();
        }

        protected virtual void PlayPlaceSound()
        {
            SoundPlayOneshot("LockDoorStop");
        }

        public virtual void ModifyChild(SimpleLocation local)
        {
            if (string.IsNullOrEmpty(prefabType)) return;
            local.prefab = prefabType;
        }

        public virtual void PlaceButton(Direction dir)
        {
            PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(firstPos.Value);
            if (cell == null) return; // cell doesn't exist
            if (cell.type == 16) return; // the cell is empty
            if (!EditorController.Instance.levelData.WallFree(buttonPos.Value, dir, false))
            {
                return;
            }
            EditorController.Instance.AddUndo();
            HallDoorStructureLocationWithButtons structure = (HallDoorStructureLocationWithButtons)EditorController.Instance.AddOrGetStructureToData(type, true);
            SimpleButtonLocation button = structure.CreateNewButton();
            button.position = buttonPos.Value;
            button.direction = dir;
            structure.myChildren.Add(first);
            structure.buttons.Add(button);
            EditorController.Instance.UpdateVisual(structure);
            first = null; // so we dont accidentally destroy the visual our first object
            EditorController.Instance.SwitchToTool(null);
            PlayButtonSound();
        }

        protected virtual void PlayButtonSound()
        {
            SoundPlayOneshot("Sfx_Button_Press");
        }

        public override bool MousePressed()
        {
            if (firstPlaced)
            {
                if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
                {
                    buttonPos = EditorController.Instance.mouseGridPosition;
                    EditorController.Instance.selector.SelectRotation(buttonPos.Value, PlaceButton);
                    return false;
                }
                return false;
            }
            if (firstPos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                firstPos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(firstPos.Value, PlaceFirst);
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
            if (firstPlaced)
            {
                if (buttonPos == null)
                {
                    EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
                }
                return;
            }
            if (firstPos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
