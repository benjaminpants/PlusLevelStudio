using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    public class HallDoorStructureTool : DoorTool
    {
        public override string id => "structure_" + type;

        public HallDoorStructureTool(string type) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
        }

        public HallDoorStructureTool(string type, Sprite sprite) : base(type, sprite)
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
            structure.myChildren.Add(local);
            EditorController.Instance.UpdateVisual(structure);
            EditorController.Instance.SwitchToTool(null);
            //EditorController.Instance.RefreshCells();
        }
    }
}
