using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PowerLeverResetButtonTool : PlaceAndRotateTool
    {
        public override string id => "structure_powerlever_resetbutton";

        public PowerLeverResetButtonTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            if (!EditorController.Instance.levelData.WallFree(position, dir, false))
            {
                return false;
            }
            EditorController.Instance.AddUndo();
            PowerLeverStructureLocation structure = (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
            SimpleButtonLocation resetButton = structure.CreateResetButton();
            resetButton.position = position;
            resetButton.direction = dir;
            structure.powerResetButtons.Add(resetButton);
            EditorController.Instance.AddVisual(resetButton);
            EditorController.Instance.UpdateVisual(structure);
            SoundPlayOneshot("Sfx_Button_Press");
            return true;
        }

        public override void Begin()
        {
            PowerLeverStructureLocation powerlevers = EditorController.Instance.GetStructureData<PowerLeverStructureLocation>("powerlever");
            if (powerlevers == null) { EditorController.Instance.SwitchToTool(null); return; }
            if (powerlevers.breakers.Count <= 0) { EditorController.Instance.SwitchToTool(null); return; }
            base.Begin();
        }
    }
}
