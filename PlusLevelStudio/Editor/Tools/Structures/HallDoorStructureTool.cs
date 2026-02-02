using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    public class HallDoorStructureTool : PlaceAndRotateTool
    {
        public string doorType;
        public string type;
        public override string id => "structure_" + type;

        public override string titleKey => "Ed_Tool_structure_" + doorType + "_Title";
        public override string descKey => "Ed_Tool_structure_" + doorType + "_Desc";

        internal HallDoorStructureTool(string type) : this(type, type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
        }

        internal HallDoorStructureTool(string type, string doorType) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + doorType))
        {
            this.doorType = doorType;
        }

        public HallDoorStructureTool(string type, string doorType, Sprite sprite)
        {
            this.type = type;
            this.sprite = sprite;
            this.doorType = doorType;
        }

        public HallDoorStructureTool(string type, Sprite sprite) : this(type, type, sprite)
        {
        }

        protected virtual void PlayPlaceSound()
        {
            SoundPlayOneshot("LockDoorStop");
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(position);
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            EditorController.Instance.AddUndo();
            HallDoorStructureLocation structure = (HallDoorStructureLocation)EditorController.Instance.AddOrGetStructureToData(type, true);
            SimpleLocation local = structure.CreateNewChild();
            local.position = position;
            local.direction = dir;
            local.prefab = doorType;
            structure.myChildren.Add(local);
            EditorController.Instance.UpdateVisual(structure);
            PlayPlaceSound();
            return true;
        }
    }
}
