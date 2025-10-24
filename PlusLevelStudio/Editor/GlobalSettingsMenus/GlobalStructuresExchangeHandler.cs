using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public abstract class GlobalStructureUIHandler : UIExchangeHandler
    {
        public StructureLocation structure;
        public abstract void PageLoaded();
        public virtual void StructureEnabled(StructureLocation structure)
        {
            this.structure = structure;
        }
    }
    public class GlobalStructuresExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public TextMeshProUGUI titleText;
        public Transform noConfigSettings;
        public GlobalStructureUIHandler[] pages;
        MenuToggle toggle;
        int currentPage = 0;
        protected List<GlobalStructurePage> globalPages;

        public virtual List<GlobalStructurePage> GetPagesFromMode()
        {
            return EditorController.Instance.currentMode.globalStructures;
        }

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            noConfigSettings = transform.Find("NoConfigurableSettings");
            globalPages = GetPagesFromMode();
            pages = new GlobalStructureUIHandler[globalPages.Count];
            for (int i = 0; i < globalPages.Count; i++)
            {
                GlobalStructurePage globalStructure = globalPages[i];
                if (globalStructure.settingsPageType == null)
                {
                    pages[i] = null;
                    continue;
                }
                GlobalStructureUIHandler pageHand = (GlobalStructureUIHandler)UIBuilder.BuildUIFromFile(globalStructure.settingsPageType, transform.GetComponent<RectTransform>(), globalStructure.structureToSpawn + "_Page", globalStructure.settingsPagePath);
                pages[i] = pageHand;
                pageHand.gameObject.SetActive(false);
            }
            toggle = transform.Find("Checkbox").GetComponent<MenuToggle>();
            if (pages.Length == 0)
            {
                transform.Find("Checkbox").gameObject.SetActive(false);
                transform.Find("Checkbox_Visual").gameObject.SetActive(false);
                transform.Find("NoConfigurableSettings").gameObject.SetActive(false);
                transform.Find("StructureEnabledText").gameObject.SetActive(false);
                transform.Find("Title").gameObject.SetActive(false);
                transform.Find("PageRight").gameObject.SetActive(false);
                transform.Find("PageLeft").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("NoGlobalStructures").gameObject.SetActive(false);
            }
        }

        public void SwitchToPage(int page)
        {
            if (pages.Length == 0) return;
            noConfigSettings.gameObject.SetActive(false);
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null) continue;
                pages[i].gameObject.SetActive(false);
            }
            currentPage = page;
            GlobalStructurePage globalStructure = globalPages[currentPage];
            titleText.text = LocalizationManager.Instance.GetLocalizedText(globalStructure.nameKey);
            // dont show the page if the structure isn't on the map
            if (EditorController.Instance.GetStructureData(globalStructure.structureToSpawn) == null)
            {
                toggle.Set(false);
                return;
            }
            toggle.Set(true);

            if (pages[currentPage] == null)
            {
                noConfigSettings.gameObject.SetActive(true);
            }
            else
            {
                pages[currentPage].gameObject.SetActive(true);
                pages[currentPage].PageLoaded();
            }
        }

        public override void Refresh()
        {
            SwitchToPage(currentPage);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "prevPage":
                    SwitchToPage(Mathf.Max(currentPage - 1, 0));
                    break;
                case "nextPage":
                    SwitchToPage(Mathf.Min(currentPage + 1, globalPages.Count - 1));
                    break;
                case "toggleStructure":
                    if ((bool)data)
                    {
                        StructureLocation structure = EditorController.Instance.AddOrGetStructureToData(globalPages[currentPage].structureToSpawn, true);
                        if (pages[currentPage] != null)
                        {
                            pages[currentPage].StructureEnabled(structure);
                        }
                    }
                    else
                    {
                        StructureLocation structure = EditorController.Instance.GetStructureData(globalPages[currentPage].structureToSpawn);
                        structure.OnDelete(EditorController.Instance.levelData);
                        if (pages[currentPage] != null)
                        {
                            pages[currentPage].structure = null;
                        }
                    }
                    SwitchToPage(currentPage);
                    handler.somethingChanged = true;
                    break;
            }
        }
    }
}
