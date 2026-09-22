using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class MultiEditorAction : AbstractEditorAction
    {
        public MultiEditorAction(AbstractEditorAction[] actions)
        {
            this.actions = actions;
        }

        AbstractEditorAction[] actions;

        public override void Perform(EditorController controller)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Perform(controller);
            }
        }

        public override void Undo(EditorController controller)
        {
            for (int i = (actions.Length - 1); i >= 0; i--)
            {
                actions[i].Undo(controller);
            }
        }
    }
}
