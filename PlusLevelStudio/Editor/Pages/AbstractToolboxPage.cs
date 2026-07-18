using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Editor.Pages
{
    /// <summary>
    /// An abstract toolbox subPage
    /// </summary>
    public abstract class AbstractToolboxSubPage
    {
        public abstract string GetName();

        protected BaseToolboxPage parent;

        public virtual void AssignParent(BaseToolboxPage parent)
        {
            this.parent = parent;
        }


        /// <summary>
        /// Adds the specified tool to this page.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public abstract bool Add(EditorTool tool);

        /// <summary>
        /// Adds the specified tool to the start of this page.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public abstract bool AddToStart(EditorTool tool);

        public abstract bool Remove(EditorTool tool);

        public abstract bool RemoveById(string toolId);

        /// <summary>
        /// Adds the specified range of tools to this page.
        /// </summary>
        /// <param name="tools"></param>
        /// <returns></returns>
        public virtual bool AddRange(IEnumerable<EditorTool> tools)
        {
            bool allAdded = true;
            foreach (var tool in tools)
            {
                allAdded &= Add(tool);
            }
            return allAdded;
        }

        public abstract bool InsertRangeAfterId(string id, IEnumerable<EditorTool> tools);

        public virtual bool InsertAfterId(string id, EditorTool tool)
        {
            return InsertRangeAfterId(id, new EditorTool[] { tool });
        }

        public abstract int GetCount(EditorController controller);

        public abstract EditorTool[] GetTools(EditorController controller);

        public abstract AbstractToolboxSubPage MakeCopy();

        /// <summary>
        /// Called when the editor is left and the page state needs to be cleaned up.
        /// </summary>
        public abstract void ResetState();

        public virtual bool IncludeInAll()
        {
            return false;
        }
    }

    public abstract class AbstractSortingToolboxSubPage : AbstractToolboxSubPage
    {

        protected List<EditorTool> edToolCache = new List<EditorTool>();

        public abstract bool MatchesCondition(EditorTool tool);

        public abstract bool TryMakeMatchCondition(EditorTool tool);

        public override bool Add(EditorTool tool)
        {
            if (MatchesCondition(tool) || TryMakeMatchCondition(tool))
            {
                return parent.Add(tool);
            }
            return false;
        }

        public override bool AddToStart(EditorTool tool)
        {
            if (MatchesCondition(tool) || TryMakeMatchCondition(tool))
            {
                return parent.AddToStart(tool);
            }
            return false;
        }

        public override int GetCount(EditorController controller)
        {
            return parent.GetTools(controller).Count(x => MatchesCondition(x));
        }

        public override EditorTool[] GetTools(EditorController controller)
        {
            edToolCache.Clear();
            edToolCache.AddRange(parent.GetTools(controller).Where(x => MatchesCondition(x)));
            return edToolCache.ToArray();
        }

        public override bool InsertRangeAfterId(string id, IEnumerable<EditorTool> tools)
        {
            throw new NotImplementedException();
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
            edToolCache.Clear();
        }
    }
}
