using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus.Structures
{
    public class RandomLockersUIHandler : GlobalStructureUIHandler
    {
        public TextMeshProUGUI hideLockerText;

        public TextMeshProUGUI minLockerText;
        public TextMeshProUGUI maxLockerText;

        public TextMeshProUGUI minHallText;
        public TextMeshProUGUI maxHallText;
        public RandomLockerLocation lockerStructure
        {
            get
            {
                return (structure == null) ? null : (RandomLockerLocation)structure;
            }
        }
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            hideLockerText = transform.Find("BlueLockerChanceBox").GetComponent<TextMeshProUGUI>();
            minLockerText = transform.Find("MinLockerBox").GetComponent<TextMeshProUGUI>();
            maxLockerText = transform.Find("MaxLockerBox").GetComponent<TextMeshProUGUI>();

            minHallText = transform.Find("MinHallBox").GetComponent<TextMeshProUGUI>();
            maxHallText = transform.Find("MaxHallBox").GetComponent<TextMeshProUGUI>();
        }

        public override void PageLoaded(StructureLocation structure)
        {
            base.PageLoaded(structure);
            hideLockerText.text = (lockerStructure.cyanLockerChance * 100f).ToString();
            minLockerText.text = lockerStructure.minMaxLockerCount.x.ToString();
            maxLockerText.text = lockerStructure.minMaxLockerCount.z.ToString();

            minHallText.text = lockerStructure.minMaxHallCount.x.ToString();
            maxHallText.text = lockerStructure.minMaxHallCount.z.ToString();
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "chanceEnter":
                    if (float.TryParse((string)data, out lockerStructure.cyanLockerChance))
                    {
                        lockerStructure.cyanLockerChance = Mathf.Clamp(lockerStructure.cyanLockerChance / 100f, 0f, 1f);
                    }
                    PageLoaded(structure);
                    break;
                case "minLockerEnter":
                    if (int.TryParse((string)data, out int minL_chance))
                    {
                        lockerStructure.minMaxLockerCount = new PlusStudioLevelFormat.MystIntVector2(Mathf.Clamp(minL_chance, 0, 999), lockerStructure.minMaxLockerCount.z);
                    }
                    PageLoaded(structure);
                    break;
                case "maxLockerEnter":
                    if (int.TryParse((string)data, out int maxL_chance))
                    {
                        lockerStructure.minMaxLockerCount = new PlusStudioLevelFormat.MystIntVector2(lockerStructure.minMaxLockerCount.x, Mathf.Clamp(maxL_chance, 0, 999));
                    }
                    PageLoaded(structure);
                    break;
                case "minHallEnter":
                    if (int.TryParse((string)data, out int minH_chance))
                    {
                        lockerStructure.minMaxHallCount = new PlusStudioLevelFormat.MystIntVector2(Mathf.Clamp(minH_chance, 0, 999), lockerStructure.minMaxHallCount.z);
                    }
                    PageLoaded(structure);
                    break;
                case "maxHallEnter":
                    if (int.TryParse((string)data, out int maxH_chance))
                    {
                        lockerStructure.minMaxHallCount = new PlusStudioLevelFormat.MystIntVector2(lockerStructure.minMaxHallCount.x, Mathf.Clamp(maxH_chance, 0, 999));
                    }
                    PageLoaded(structure);
                    break;
            }
        }
    }
}
