using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectToolNoRotation : EditorTool
    {
        public string type;
        public override string id => "object_" + type;
        public float verticalOffset = 0f;
        public ObjectToolNoRotation(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), 0f)
        {
        }

        public ObjectToolNoRotation(string type, float offset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), offset)
        {
        }

        public ObjectToolNoRotation(string type, Sprite sprite, float offset)
        {
            this.sprite = sprite;
            this.type = type;
            verticalOffset = offset;
        }

        public ObjectToolNoRotation(string type, Sprite sprite) : this(type, sprite, 0f)
        {
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
                BasicObjectLocation local = new BasicObjectLocation();
                local.prefab = type;
                local.position = EditorController.Instance.mouseGridPosition.ToWorld();
                local.position += Vector3.up * verticalOffset;
                EditorController.Instance.levelData.objects.Add(local);
                EditorController.Instance.AddVisual(local);
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
