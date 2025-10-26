using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public interface IEditorPositionVerifyable
    {
        bool ValidatePosition(EditorLevelData data);
    }
}
