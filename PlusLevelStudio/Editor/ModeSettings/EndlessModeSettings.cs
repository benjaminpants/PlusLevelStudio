using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.Editor.GlobalSettingsMenus;
using PlusLevelStudio.Ingame;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.ModeSettings
{
    public class EndlessModeSettings : EditorGameModeSettings
    {
        public List<WeightedID> items = new List<WeightedID>();
        public override void ApplySettingsToManager(BaseGameManager manager)
        {
            EditorEndlessGameManager man = (EditorEndlessGameManager)manager;
            man.items = items.Select(x => new WeightedItemObject()
            {
                weight = x.weight,
                selection = LevelLoaderPlugin.Instance.itemObjects[x.id]
            }).ToArray();
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            int itemCount = reader.ReadInt32();
            items.Clear();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new WeightedID()
                {
                    id = reader.ReadString(),
                    weight = reader.ReadInt32()
                });
            }
        }

        const byte version = 0;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].id);
                writer.Write(items[i].weight);
            }
        }
    }

    public class EditorEndlessGameMode : EditorGameMode
    {
        public override EditorGameModeSettings CreateSettings()
        {
            return new EndlessModeSettings();
        }
    }

    public class EndlessSettingsPageUIExchangeHandler : ModeSettingsPageUIExchangeHandler
    {
        public EndlessModeSettings properSettings => (EndlessModeSettings)settings;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {

        }

        public override void PageLoaded()
        {
            
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "configureItems":
                    EndlessItemsUIExchangeHandler ui = EditorController.Instance.CreateUI<EndlessItemsUIExchangeHandler>("ItemConfigurator", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "ModeSettings", "EndlessSettings_ItemConfigurator.json"));
                    ui.settingsHandler = this;
                    ui.Refresh();
                    break;
            }
        }
    }

    public class EndlessItemsUIExchangeHandler : WeightedListExchangeHandler
    {
        public EndlessSettingsPageUIExchangeHandler settingsHandler;
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
            for (int i = 0; i < LevelStudioPlugin.Instance.selectableGeneratorItems.Count; i++)
            {
                WeightedID existingId = settingsHandler.properSettings.items.Find(x => x.id == LevelStudioPlugin.Instance.selectableGeneratorItems[i]);
                weightedIDs.Add(new WeightedID()
                {
                    id = LevelStudioPlugin.Instance.selectableGeneratorItems[i],
                    weight = existingId == null ? 0 : existingId.weight,
                });
            }
            SortList();
        }

        public override void UpdateValues()
        {
            for (int i = 0; i < weightedIDs.Count; i++)
            {
                WeightedID existingId = settingsHandler.properSettings.items.Find(x => x.id == weightedIDs[i].id);
                if (existingId != null)
                {
                    if (weightedIDs[i].weight == 0)
                    {
                        settingsHandler.properSettings.items.Remove(existingId);
                    }
                    else
                    {
                        existingId.weight = weightedIDs[i].weight;
                    }
                }
                else
                {
                    if (weightedIDs[i].weight == 0) continue;
                    settingsHandler.properSettings.items.Add(new WeightedID()
                    {
                        id = weightedIDs[i].id,
                        weight = weightedIDs[i].weight
                    });
                }
            }
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message == "exit")
            {
                EditorController.Instance.RemoveUI(gameObject);
                return;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
