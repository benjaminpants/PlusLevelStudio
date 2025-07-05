using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public interface IEditorCellModifier
    {
        void ModifyCells(EditorLevelData data, bool forEditor);
    }
}
