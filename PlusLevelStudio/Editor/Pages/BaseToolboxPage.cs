using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Pages
{
    public class BaseToolboxPage : AbstractToolboxSubPage
    {
        public List<AbstractToolboxSubPage> subPages = new List<AbstractToolboxSubPage>();

        protected string _name;

        public BaseToolboxPage(string name)
        {
            AddSubpage(new AllToolsToolboxPage());
            _name = name;
        }

        /// <summary>
        /// Called when the editor is left and the page state needs to be cleaned up.
        /// </summary>
        public override void ResetState()
        {
            subPages.ForEach(page => page.ResetState());
        }

        public virtual void AddSubpage(AbstractToolboxSubPage page)
        {
            subPages.Add(page);
            page.AssignParent(this);
        }

        public EditorTool[] GetAllToolsIncludingSubpages(EditorController controller)
        {
            List<EditorTool> tools = new List<EditorTool>();
            tools.AddRange(GetTools(controller));
            subPages.ForEach(x => {
                if (!x.IncludeInAll()) return;
                tools.AddRange(x.GetTools(controller));
            });
            return tools.ToArray();
        }

        public List<EditorTool> tools = new List<EditorTool>();
        public override bool Add(EditorTool tool)
        {
            tools.Add(tool);
            return true;
        }

        public override bool AddToStart(EditorTool tool)
        {
            tools.Insert(0, tool);
            return true;
        }

        public override int GetCount(EditorController controller)
        {
            return tools.Count;
        }

        public override EditorTool[] GetTools(EditorController controller)
        {
            return tools.ToArray();
        }

        public override bool InsertRangeAfterId(string id, IEnumerable<EditorTool> toolArray)
        {
            int indexOf = tools.FindIndex(x => x.id == id);
            if (indexOf == -1) return false;
            tools.InsertRange(indexOf + 1, toolArray);
            return true;
        }

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new BaseToolboxPage(_name)
            {
                tools = new List<EditorTool>(tools)
            };
        }

        public override bool Remove(EditorTool tool)
        {
            return tools.Remove(tool);
        }

        public override bool RemoveById(string toolId)
        {
            int index = tools.FindIndex(x => x.id == toolId);
            if (index == -1) return false;
            tools.RemoveAt(index);
            return true;
        }

        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_Category_" + _name);
        }

        public override bool IncludeInAll()
        {
            return true;
        }
    }


    /// <summary>
    /// A page that returns all tools in it's parent, excluded from GetAllToolsIncludingSubpages
    /// </summary>
    public class AllToolsToolboxPage : AbstractToolboxSubPage
    {
        public override bool Add(EditorTool tool)
        {
            return parent.Add(tool);
        }

        public override bool AddToStart(EditorTool tool)
        {
            return parent.AddToStart(tool);
        }

        public override int GetCount(EditorController controller)
        {
            int count = parent.GetCount(controller);
            parent.subPages.ForEach(x => {
                if (!x.IncludeInAll()) return;
                count += x.GetCount(controller);
            });
            return count;
        }

        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_SubCategory_All");
        }

        public override EditorTool[] GetTools(EditorController controller)
        {
            return parent.GetAllToolsIncludingSubpages(controller);
        }

        public override bool InsertRangeAfterId(string id, IEnumerable<EditorTool> tools)
        {
            return parent.InsertRangeAfterId(id, tools);
        }

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new AllToolsToolboxPage();
        }

        public override bool Remove(EditorTool tool)
        {
            return parent.Remove(tool);
        }

        public override bool RemoveById(string toolId)
        {
            return parent.RemoveById(toolId);
        }

        public override void ResetState()
        {

        }
    }
}
