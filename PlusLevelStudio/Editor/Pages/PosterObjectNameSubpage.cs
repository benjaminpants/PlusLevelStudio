using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Pages
{
    public class PosterObjectNameSubpage : AbstractSortingToolboxSubPage
    {
        public PosterObjectNameSubpage(string toSearch, string altTag)
        {
            nameToSearchFor = toSearch;
            tagToSearchFor = altTag;
        }

        public string nameToSearchFor;
        public string tagToSearchFor;
        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_SubCategory_PosterName_" + nameToSearchFor);
        }

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new PosterObjectNameSubpage(nameToSearchFor, tagToSearchFor);
        }

        public override bool MatchesCondition(EditorTool tool)
        {
            if (tool.tags.Contains(tagToSearchFor)) return true;
            if (!(tool is PosterTool posterTool)) return false;
            if (LevelLoaderPlugin.Instance.posterAliases[posterTool.type] == null) return false;
            return LevelLoaderPlugin.Instance.posterAliases[posterTool.type].name.Contains(nameToSearchFor);
        }

        public override bool TryMakeMatchCondition(EditorTool tool)
        {
            return false;
        }
    }
}
