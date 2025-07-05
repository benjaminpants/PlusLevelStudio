using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Tools
{
    public class DoorTool : EditorTool
    {
        public string doorType;
        public override string id => "door_" + doorType;

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            
        }

        public override bool MousePressed()
        {
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            
        }
    }
}
