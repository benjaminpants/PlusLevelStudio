using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Editor.Pages
{
    public class TagToolboxSubPage : AbstractSortingToolboxSubPage
    {
        public TagToolboxSubPage(string[] toSearch)
        {
            tagsSearchingFor = toSearch;
        }

        public TagToolboxSubPage(string toSearch)
        {
            tagsSearchingFor = new string[] { toSearch };
        }
        public string[] tagsSearchingFor;

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new TagToolboxSubPage(tagsSearchingFor);
        }

        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_SubCategory_Tag_" + tagsSearchingFor[0]);
        }

        public override bool MatchesCondition(EditorTool tool)
        {
            for (int i = 0; i < tagsSearchingFor.Length; i++)
            {
                if (tool.tags.Contains(tagsSearchingFor[i])) return true;
            }
            return false;
        }

        public override bool TryMakeMatchCondition(EditorTool tool)
        {
            return tool.tags.Add(tagsSearchingFor[0]);
        }
    }
}
