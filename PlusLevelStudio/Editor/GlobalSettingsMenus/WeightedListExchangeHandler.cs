using MTM101BaldAPI;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public struct WeightedListUIEntry
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI weight;
        public StandardMenuButton leftButton;
        public StandardMenuButton rightButton;
        public Image image;
    }
    public abstract class WeightedListExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public List<WeightedID> weightedIDs = new List<WeightedID>();
        public List<WeightedListUIEntry> uiEntries = new List<WeightedListUIEntry>();
        int page = 0;
        int amountPerPage = 0;
        int maxPage = 0;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            int index = 0;
            Transform sprite = transform.Find("Icon" + index);
            while (sprite != null)
            {
                uiEntries.Add(new WeightedListUIEntry()
                {
                    image = sprite.GetComponent<Image>(),
                    title = transform.Find("Text" + index).GetComponent<TextMeshProUGUI>(),
                    weight = transform.Find("TextBox" + index).GetComponent<TextMeshProUGUI>(),
                    leftButton = transform.Find("Left" + index).GetComponent<StandardMenuButton>(),
                    rightButton = transform.Find("Right" + index).GetComponent<StandardMenuButton>()
                });
                index++;
                sprite = transform.Find("Icon" + index);
            }
            amountPerPage = uiEntries.Count;
        }

        public void SwitchToPage(int pageNumber)
        {
            page = Mathf.Clamp(pageNumber, 0, maxPage);
            RefreshListUI();
        }

        public override void Refresh()
        {
            UpdateList();
            maxPage = Mathf.CeilToInt((float)weightedIDs.Count / amountPerPage) - 1;
            SwitchToPage(page);

        }

        public void RefreshListUI()
        {
            int pageStart = page * amountPerPage;
            for (int i = pageStart; i < pageStart + amountPerPage; i++)
            {
                int uiIndex = i - pageStart;
                if (i >= weightedIDs.Count)
                {
                    uiEntries[uiIndex].rightButton.gameObject.SetActive(false);
                    uiEntries[uiIndex].leftButton.gameObject.SetActive(false);
                    uiEntries[uiIndex].image.gameObject.SetActive(false);
                    uiEntries[uiIndex].title.gameObject.SetActive(false);
                    uiEntries[uiIndex].weight.gameObject.SetActive(false);
                    continue;
                }
                uiEntries[uiIndex].title.text = GetNameFor(weightedIDs[i].id);
                uiEntries[uiIndex].image.sprite = GetSpriteFor(weightedIDs[i].id);
                uiEntries[uiIndex].weight.text = weightedIDs[i].weight.ToString();
                uiEntries[uiIndex].rightButton.gameObject.SetActive(true);
                uiEntries[uiIndex].leftButton.gameObject.SetActive(true);
                uiEntries[uiIndex].image.gameObject.SetActive(true);
                uiEntries[uiIndex].title.gameObject.SetActive(true);
                uiEntries[uiIndex].weight.gameObject.SetActive(true);
            }
        }

        public void SortList()
        {
            weightedIDs.Sort((a, b) =>
            {
                int initCompare = -a.weight.CompareTo(b.weight);
                if (initCompare == 0)
                {
                    return a.id.CompareTo(GetNameFor(b.id));
                }
                return initCompare;
            });
        }

        public abstract void UpdateValues();

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "prevPage":
                    SwitchToPage(page - 1);
                    return;
                case "nextPage":
                    SwitchToPage(page + 1);
                    return;
            }
            int pageStart = page * amountPerPage;
            int index = int.Parse(message.Substring(message.Length - 1));
            string baseMessage = message.Remove(message.Length - 1);
            switch(baseMessage)
            {
                case "weightSet":
                    weightedIDs[pageStart + index].weight = Mathf.Clamp(int.Parse((string)data), 0, 9999);
                    UpdateValues();
                    Refresh();
                    break;
                case "left":
                    weightedIDs[pageStart + index].weight = Mathf.Clamp(weightedIDs[pageStart + index].weight - 10, 0, 9999);
                    UpdateValues();
                    Refresh();
                    break;
                case "right":
                    weightedIDs[pageStart + index].weight = Mathf.Clamp(weightedIDs[pageStart + index].weight + 10, 0, 9999);
                    UpdateValues();
                    Refresh();
                    break;
            }
        }

        public abstract void UpdateList();
        public abstract Sprite GetSpriteFor(string key);
        public abstract string GetNameFor(string key);

    }

    public class StoreItemListExchangeHandler : WeightedListExchangeHandler
    {
        TextMeshProUGUI storeCountText;


        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message == "setStoreItemCount")
            {
                if (int.TryParse((string)data, out int count))
                {
                    count = Mathf.Clamp(count, 0, 12);
                    EditorController.Instance.levelData.storeItemCount = count;
                    handler.somethingChanged = true;
                }
                Refresh();
                return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            storeCountText = transform.Find("StoreCountBox").GetComponent<TextMeshProUGUI>();
        }

        public override void Refresh()
        {
            base.Refresh();
            storeCountText.text = EditorController.Instance.levelData.storeItemCount.ToString();
        }

        public override string GetNameFor(string key)
        {
            return LocalizationManager.Instance.GetLocalizedText(LevelLoaderPlugin.Instance.itemObjects[key].nameKey);
        }

        public override Sprite GetSpriteFor(string key)
        {
            return LevelLoaderPlugin.Instance.itemObjects[key].itemSpriteSmall;
        }

        public override void UpdateList()
        {
            weightedIDs.Clear();
            for (int i = 0; i < LevelStudioPlugin.Instance.selectableShopItems.Count; i++)
            {
                WeightedID existingId = EditorController.Instance.levelData.potentialStoreItems.Find(x => x.id == LevelStudioPlugin.Instance.selectableShopItems[i]);
                weightedIDs.Add(new WeightedID()
                {
                    id = LevelStudioPlugin.Instance.selectableShopItems[i],
                    weight = existingId == null ? 0 : existingId.weight,
                });
            }
            SortList();
        }

        public override void UpdateValues()
        {
            for (int i = 0; i < weightedIDs.Count; i++)
            {
                WeightedID existingId = EditorController.Instance.levelData.potentialStoreItems.Find(x => x.id == weightedIDs[i].id);
                if (existingId != null)
                {
                    if (weightedIDs[i].weight == 0)
                    {
                        EditorController.Instance.levelData.potentialStoreItems.Remove(existingId);
                    }
                    else
                    {
                        existingId.weight = weightedIDs[i].weight;
                    }
                }
                else
                {
                    if (weightedIDs[i].weight == 0) continue;
                    EditorController.Instance.levelData.potentialStoreItems.Add(new WeightedID()
                    {
                        id = weightedIDs[i].id,
                        weight = weightedIDs[i].weight
                    });
                }
            }
            handler.somethingChanged = true;
        }
    }

    public class StickerListExchangeHandler : WeightedListExchangeHandler
    {
        public override string GetNameFor(string key)
        {
            return LocalizationManager.Instance.GetLocalizedText("StickerTitle_" + LevelLoaderPlugin.Instance.stickerAliases[key].ToStringExtended());
        }

        public override Sprite GetSpriteFor(string key)
        {
            return LevelStudioPlugin.Instance.stickerSprites[key];
        }

        public override void UpdateList()
        {
            weightedIDs.Clear();
            for (int i = 0; i < LevelStudioPlugin.Instance.selectableStickers.Count; i++)
            {
                WeightedID existingId = EditorController.Instance.levelData.potentialStickers.Find(x => x.id == LevelStudioPlugin.Instance.selectableStickers[i]);
                weightedIDs.Add(new WeightedID()
                {
                    id = LevelStudioPlugin.Instance.selectableStickers[i],
                    weight = existingId == null ? 0 : existingId.weight,
                });
            }
            SortList();
        }

        public override void UpdateValues()
        {
            for (int i = 0; i < weightedIDs.Count; i++)
            {
                WeightedID existingId = EditorController.Instance.levelData.potentialStickers.Find(x => x.id == weightedIDs[i].id);
                if (existingId != null)
                {
                    if (weightedIDs[i].weight == 0)
                    {
                        EditorController.Instance.levelData.potentialStickers.Remove(existingId);
                    }
                    else
                    {
                        existingId.weight = weightedIDs[i].weight;
                    }
                }
                else
                {
                    if (weightedIDs[i].weight == 0) continue;
                    EditorController.Instance.levelData.potentialStickers.Add(new WeightedID()
                    {
                        id = weightedIDs[i].id,
                        weight = weightedIDs[i].weight
                    });
                }
            }
            handler.somethingChanged = true;
        }
    }
}
