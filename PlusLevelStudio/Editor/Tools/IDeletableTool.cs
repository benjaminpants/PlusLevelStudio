using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Tools
{
    internal interface IDeletableTool
    {
        void RequestDelete(bool shouldConfirm);
    }
}
