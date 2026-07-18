using MTM101BaldAPI.Registers;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Pages
{
    public class ItemTagSubPage : AbstractSortingToolboxSubPage
    {
        public ItemTagSubPage(string[] toSearch)
        {
            tagsSearchingFor = toSearch;
        }

        public ItemTagSubPage(string toSearch)
        {
            tagsSearchingFor = new string[] { toSearch };
        }
        public string[] tagsSearchingFor;

        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_SubCategory_ItemTag_" + tagsSearchingFor[0]);
        }

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new ItemTagSubPage(tagsSearchingFor);
        }

        public override bool MatchesCondition(EditorTool tool)
        {
            if (!(tool is ItemTool itemTool)) return false;
            ItemObject itmObj = LevelLoaderPlugin.Instance.itemObjects[itemTool.item];
            if (itmObj == null) return false;
            ItemMetaData metaData = itmObj.GetMeta();
            if (metaData == null) return false;
            for (int i = 0; i < tagsSearchingFor.Length; i++)
            {
                if (metaData.tags.Contains(tagsSearchingFor[i])) return true;
            }
            return false;
        }

        public override bool TryMakeMatchCondition(EditorTool tool)
        {
            return false;
        }
    }
}
