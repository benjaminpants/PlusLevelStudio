using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public abstract class AbstractEditorAction
    {
        public abstract void Perform(EditorController controller);

        public abstract void Undo(EditorController controller);
    }
}
