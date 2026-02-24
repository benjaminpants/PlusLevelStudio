using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class NPCPropertyPage
    {
        public Type npcPropertiesType;
        public Type pageType;
        public string pagePath;
    }

    public abstract class NPCPropertyExchangeHandler : EditorOverlayUIExchangeHandler
    {
        public NPCProperties properties;
        public bool propertiesChanged = false;

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            EditorController.Instance.HoldUndo();
        }

        public abstract void OnPropertiesAssigned();

        public override bool OnExit()
        {
            if (propertiesChanged)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
            return base.OnExit();
        }
    }

    public class DummyNPCPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public override void OnPropertiesAssigned()
        {
            // do nothing
        }
    }
}
