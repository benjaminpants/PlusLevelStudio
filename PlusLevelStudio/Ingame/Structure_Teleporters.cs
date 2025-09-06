using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_Teleporters : StructureBuilder
    {
    }

    public class EditorTeleporterRoomFunction : TeleporterRoomFunction
    {
        static FieldInfo _teleporterController = AccessTools.Field(typeof(TeleporterRoomFunction), "teleporterController");
        protected override void OnLastEntityExit(Entity entity)
        {
            if (_teleporterController.GetValue(this) == null) return;
            base.OnLastEntityExit(entity);
        }

        public void AssignTeleporterController(TeleporterController tc)
        {
            _teleporterController.SetValue(this, tc);
        }

        // TODO:
        // assign teleporterController the buttons
        // assign us the room labels
        public void AssignButtonPanel(Transform panel)
        {
            throw new NotImplementedException();
        }
    }
}
