using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ActivityTool : DoorTool
    {
        protected float height = 0f;
        public override string id => "activity_" + type;
        public ActivityTool(string type, float heightOffset) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/activity_" + type))
        {
            height = heightOffset;
        }
        public ActivityTool(string type, Sprite sprite, float heightOffset) : base(type, sprite)
        {
            height = heightOffset;
        }

        public override void OnPlaced(Direction dir)
        {
            EditorController.Instance.HoldUndo();
            ActivityLocation activity = new ActivityLocation();
            activity.position = pos.Value.ToWorld();
            activity.position += Vector3.up * height;
            activity.direction = dir;
            activity.type = type;
            if (!activity.ValidatePosition(EditorController.Instance.levelData))
            {
                EditorController.Instance.CancelHeldUndo();
                return;
            }
            EditorController.Instance.AddHeldUndo();
            activity.Setup(EditorController.Instance.levelData);
            EditorController.Instance.AddVisual(activity);
            EditorController.Instance.SwitchToTool(null);
        }
    }
}
