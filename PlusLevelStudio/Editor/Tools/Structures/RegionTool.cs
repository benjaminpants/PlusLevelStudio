using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class RegionTool : EditorTool
    {
        public int type;
        public override string id => "structure_region_" + type;
        public override string titleKey => LocalizationManager.Instance.GetLocalizedText("Ed_Tool_structure_region_X_Title").Replace("X",type.ToString());
        public override string descKey => LocalizationManager.Instance.GetLocalizedText("Ed_Tool_structure_region_X_Desc").Replace("X", type.ToString());

        EditorRoom foundRoom;
        internal RegionTool(int type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_region_" + type))
        {
        }

        public RegionTool(int type, Sprite sprite)
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
            foundRoom = null;
        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.HoldUndo();
                foundRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
                RegionStructureLocation structure = (RegionStructureLocation)EditorController.Instance.AddOrGetStructureToData("region", true);
                if (structure.regions.Find(x => x.room == foundRoom) != null)
                {
                    EditorController.Instance.CancelHeldUndo();
                    structure.DeleteIfInvalid();
                    return false;
                }
                EditorController.Instance.AddHeldUndo();
                structure.regions.Add(new RegionLocation()
                {
                    id=type,
                    myStructure=structure,
                    room=foundRoom,
                });
                EditorController.Instance.UpdateVisual(structure);
                SoundPlayOneshot("Slap");
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
            EditorRoom oldRoom = foundRoom;
            foundRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
            StructureLocation structure = EditorController.Instance.GetStructureData("region");
            bool isValid = true;
            if (structure != null)
            {
                isValid = (((RegionStructureLocation)structure).regions.Find(x => x.room == foundRoom) == null);
            }
            if (oldRoom != foundRoom)
            {
                if (oldRoom != null)
                {
                    EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(oldRoom), "none");
                }
                EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(foundRoom), isValid ? "yellow" : "red");
            }
        }
    }
}
