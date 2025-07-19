using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlusLevelStudio.Editor.Tools
{
    public class PosterTool : DoorTool
    {
        public override string id => "poster_" + LevelLoaderPlugin.Instance.posterAliases[type];
        public override string titleKey => "Ed_Poster_Title_" + type;
        public override string descKey => "Ed_Poster_Desc_" + type;
        public PosterTool(string type) : base(type, LevelStudioPlugin.Instance.GenerateOrGetSmallPosterSprite(LevelLoaderPlugin.Instance.posterAliases[type]))
        {
        }

        public override void OnPlaced(Direction dir)
        {
            if (EditorController.Instance.levelData.WallFree(pos.Value, dir, false))
            {
                EditorController.Instance.AddUndo();
                PosterPlacement poster = new PosterPlacement();
                poster.position = pos.Value;
                poster.direction = dir;
                poster.type = type;
                EditorController.Instance.levelData.posters.Add(poster);
                EditorController.Instance.AddVisual(poster);
                EditorController.Instance.SwitchToTool(null);
            }
        }
    }
}
