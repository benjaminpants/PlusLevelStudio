using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class TileBasedObjectTool : PlaceAndRotateTool
    {
        public string type;
        public override string id => "tileobject_" + type;

        internal TileBasedObjectTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/tileobject_" + type))
        {

        }

        public TileBasedObjectTool(string type, Sprite sprite)
        {
            this.sprite = sprite;
            this.type = type;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.AddUndo();
            TileBasedObjectPlacement tileOb = new TileBasedObjectPlacement()
            {
                type = type,
                position = position,
                direction = dir
            };
            EditorController.Instance.levelData.tileBasedObjects.Add(tileOb);
            EditorController.Instance.AddVisual(tileOb);
            return true;
        }
    }
}
