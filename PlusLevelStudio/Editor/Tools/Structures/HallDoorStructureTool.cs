using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    public class HallDoorStructureTool : DoorTool
    {
        public string doorType;
        public override string id => "structure_" + type;

        public override string titleKey => "Ed_Tool_structure_" + doorType + "_Title";
        public override string descKey => "Ed_Tool_structure_" + doorType + "_Desc";

        internal HallDoorStructureTool(string type) : this(type, type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
        }

        internal HallDoorStructureTool(string type, string doorType) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + doorType))
        {
            this.doorType = doorType;
        }

        public HallDoorStructureTool(string type, string doorType, Sprite sprite) : base(type, sprite)
        {
            this.doorType = doorType;
        }

        public HallDoorStructureTool(string type, Sprite sprite) : this(type, type, sprite)
        {
        }

        public override void OnPlaced(Direction dir)
        {
            PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(pos.Value);
            if (cell == null) return; // cell doesn't exist
            if (cell.type == 16) return; // the cell is empty
            EditorController.Instance.AddUndo();
            HallDoorStructureLocation structure = (HallDoorStructureLocation)EditorController.Instance.AddOrGetStructureToData(type, true);
            SimpleLocation local = structure.CreateNewChild();
            local.position = pos.Value;
            local.direction = dir;
            local.prefab = doorType;
            structure.myChildren.Add(local);
            EditorController.Instance.UpdateVisual(structure);
            EditorController.Instance.SwitchToTool(null);
            //EditorController.Instance.RefreshCells();
        }
    }
}
