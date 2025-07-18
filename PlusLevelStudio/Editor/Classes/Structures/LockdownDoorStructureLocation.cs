using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class LockdownDoorStructureLocation : HallDoorStructureLocationWithLevers
    {
        public override bool ShouldLeverBeDown(SimpleLeverLocation lever)
        {
            return myChildren[buttons.IndexOf(lever)].prefab == "lockdowndoor_shut";
        }

        public override SimpleLocation CreateNewChild()
        {
            LockdownDoorLocation simple = new LockdownDoorLocation();
            simple.prefab = type;
            simple.deleteAction = OnSubDelete;
            return simple;
        }
    }

    public class LockdownDoorLocation : SimpleLocation, IEditorSettingsable
    {
        public override void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<SettingsComponent>().activateSettingsOn = this;
            base.InitializeVisual(visualObject);
        }
        public void SettingsClicked()
        {
            LockdownDoorSettingsExchangeHandler exchange = EditorController.Instance.CreateUI<LockdownDoorSettingsExchangeHandler>("LockdownDoorConfig");
            exchange.myDoor = this;
            exchange.myLocal = (LockdownDoorStructureLocation)EditorController.Instance.levelData.structures.Where(x => x is LockdownDoorStructureLocation).First(x => ((LockdownDoorStructureLocation)x).myChildren.Contains(this));
            exchange.Assigned();
        }
    }
}
