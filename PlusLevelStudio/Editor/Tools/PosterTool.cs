using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PosterTool : PlaceAndRotateTool
    {
        public string type;
        public override string id => "poster_" + LevelLoaderPlugin.Instance.posterAliases[type]; // ????
        public override string titleKey => "Ed_Poster_Title_" + type;
        public override string descKey => "Ed_Poster_Desc_" + type;
        // not internalizing this one because auto generated poster sprites work regardless of the mod origin assuming they have been added to the loader
        public PosterTool(string type) : this(type, LevelStudioPlugin.Instance.GenerateOrGetSmallPosterSprite(LevelLoaderPlugin.PosterFromAlias(type)))
        {
        }

        public PosterTool(string type, Sprite sprite)
        {
            this.type = type;
            this.sprite = sprite;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            if (EditorController.Instance.levelData.WallFree(position, dir, false))
            {
                EditorController.Instance.AddUndo();
                PosterPlacement poster = new PosterPlacement();
                poster.position = position;
                poster.direction = dir;
                poster.type = type;
                EditorController.Instance.levelData.posters.Add(poster);
                EditorController.Instance.AddVisual(poster);
                SoundPlayOneshot("Slap");
                return true;
            }
            return false;
        }

        public override bool ValidLocation(IntVector2 position)
        {
            if (!base.ValidLocation(position)) return false;
            for (int i = 0; i < 4; i++)
            {
                if (EditorController.Instance.levelData.WallFree(position, (Direction)i, false)) return true;
            }
            return false;
        }
    }
}
