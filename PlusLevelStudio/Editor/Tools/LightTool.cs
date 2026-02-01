using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class LightTool : PointTool
    {
        public string lightType;
        public override string id => "light_" + lightType;

        internal LightTool(string lightType) : this(lightType, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/light_" + lightType))
        {
        }

        public LightTool(string lightType, Sprite sprite)
        {
            this.sprite = sprite;
            this.lightType = lightType;
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            LightPlacement lightPlace = new LightPlacement();
            lightPlace.type = lightType;
            lightPlace.position = EditorController.Instance.mouseGridPosition;
            EditorController.Instance.AddVisual(lightPlace);
            EditorController.Instance.levelData.lights.Add(lightPlace);
            EditorController.Instance.RefreshLights();
            SoundPlayOneshot("ComputerHum");
            return true;
        }
    }
}
