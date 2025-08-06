using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class RoomEditorController : EditorController
    {
        public override void Export()
        {
            //base.Export();
        }

        public override void PlayLevel()
        {
            // no
        }

        public override void EditorModeAssigned()
        {
            base.EditorModeAssigned();
            levelData.minLightColor = UnityEngine.Color.white;
        }
    }
}
