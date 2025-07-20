using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class LightTool : EditorTool
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

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            
        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.AddUndo();
                LightPlacement lightPlace = new LightPlacement();
                lightPlace.type = lightType;
                lightPlace.position = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.AddVisual(lightPlace);
                EditorController.Instance.levelData.lights.Add(lightPlace);
                EditorController.Instance.RefreshLights();
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
            EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
        }
    }
}
