using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class GlobalRandomStructuresExchangeHandler : GlobalStructuresExchangeHandler
    {
        public TextMeshProUGUI seedTextbox;
        public override void OnElementsCreated()
        {
            seedTextbox = transform.Find("SeedTextBox").GetComponent<TextMeshProUGUI>();
            base.OnElementsCreated();
        }

        public override void Refresh()
        {
            base.Refresh();
            seedTextbox.text = EditorController.Instance.levelData.seed.ToString();
        }

        public override List<GlobalStructurePage> GetPagesFromMode()
        {
            return EditorController.Instance.currentMode.globalRandomStructures;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "randomizeSeed":
                    seedTextbox.text = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
                    SendInteractionMessage("seedEnter", seedTextbox.text);
                    return;
                case "seedEnter":
                    int.TryParse(seedTextbox.text, out EditorController.Instance.levelData.seed);
                    Refresh();
                    return;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
