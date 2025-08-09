using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ActivityTool : PlaceAndRotateTool
    {
        public string type;
        protected float height = 0f;
        public override string id => "activity_" + type;
        internal ActivityTool(string type, float heightOffset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/activity_" + type), heightOffset)
        {
        }
        public ActivityTool(string type, Sprite sprite, float heightOffset)
        {
            this.type = type;
            this.sprite = sprite;
            height = heightOffset;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.HoldUndo();
            ActivityLocation activity = new ActivityLocation();
            activity.position = position.ToWorld();
            activity.position += Vector3.up * height;
            activity.direction = dir;
            activity.type = type;
            if (!activity.ValidatePosition(EditorController.Instance.levelData))
            {
                EditorController.Instance.CancelHeldUndo();
                return false;
            }
            EditorController.Instance.AddHeldUndo();
            activity.Setup(EditorController.Instance.levelData);
            EditorController.Instance.AddVisual(activity);
            return true;
        }
    }
}
