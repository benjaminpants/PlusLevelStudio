using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    // TODO: make it so this accepts a prefab ID, which would be used to set the light transform used for the visual
    public class LightTool : EditorTool
    {
        public override string id => "light";

        public LightTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/light");
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
                LightPlacement lightPlace = new LightPlacement();
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
