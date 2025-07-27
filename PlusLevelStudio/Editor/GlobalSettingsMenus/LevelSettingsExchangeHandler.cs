using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class LevelSettingsExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        TextMeshProUGUI elevatorText;
        TextMeshProUGUI initEventText;
        TextMeshProUGUI minEventText;
        TextMeshProUGUI maxEventText;
        StandardMenuButton[] randomEventButtons;
        DigitalNumberDisplay[] displays;
        int randomEventViewOffset = 0;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            elevatorText = transform.Find("ElevatorTitle").GetComponent<TextMeshProUGUI>();
            randomEventButtons = new StandardMenuButton[]
            {
                transform.Find("Event0").GetComponent<StandardMenuButton>(),
                transform.Find("Event1").GetComponent<StandardMenuButton>(),
                transform.Find("Event2").GetComponent<StandardMenuButton>(),
                transform.Find("Event3").GetComponent<StandardMenuButton>(),
                transform.Find("Event4").GetComponent<StandardMenuButton>(),
                transform.Find("Event5").GetComponent<StandardMenuButton>(),
                transform.Find("Event6").GetComponent<StandardMenuButton>()
            };
            initEventText = transform.Find("InitialEventTime").GetComponent<TextMeshProUGUI>();
            minEventText = transform.Find("MinEventTime").GetComponent<TextMeshProUGUI>();
            maxEventText = transform.Find("MaxEventTime").GetComponent<TextMeshProUGUI>();
            displays = new DigitalNumberDisplay[]
            {
                transform.Find("LevelTimeSeg0").GetComponent<DigitalNumberDisplay>(),
                transform.Find("LevelTimeSeg1").GetComponent<DigitalNumberDisplay>(),
                transform.Find("LevelTimeSeg2").GetComponent<DigitalNumberDisplay>(),
                transform.Find("LevelTimeSeg3").GetComponent<DigitalNumberDisplay>(),
            };

            for (int i = 0; i < randomEventButtons.Length; i++)
            {
                int index = i; // why must i do this
                randomEventButtons[i].eventOnHigh = true;
                randomEventButtons[i].OnHighlight.AddListener(() =>
                {
                    EventHighlight(index);
                });
                randomEventButtons[i].OffHighlight.AddListener(() =>
                {
                    EditorController.Instance.tooltipController.CloseTooltip();
                });
            }
        }

        public override void Refresh()
        {
            elevatorText.text = EditorController.Instance.levelData.elevatorTitle;
            initEventText.text = EditorController.Instance.levelData.initialRandomEventGap.ToString();
            minEventText.text = EditorController.Instance.levelData.minRandomEventGap.ToString();
            maxEventText.text = EditorController.Instance.levelData.maxRandomEventGap.ToString();
            int time = Mathf.RoundToInt(EditorController.Instance.levelData.timeLimit);
            string displayTime = string.Format("{0}{1}", Mathf.Floor((float)(time / 60)).ToString("00"), (time % 60).ToString("00"));
            for (int i = 0; i < displays.Length; i++)
            {
                displays[i].currentValue = (int)char.GetNumericValue(displayTime, i);
            }
            RefreshEventView();
        }

        public void RefreshEventView()
        {
            for (int i = 0; i < randomEventButtons.Length; i++)
            {
                if ((i + randomEventViewOffset) >= EditorController.Instance.currentMode.availableRandomEvents.Count)
                {
                    randomEventButtons[i].image.color = Color.clear;
                    continue;
                }
                randomEventButtons[i].image.sprite = LevelStudioPlugin.Instance.eventSprites[EditorController.Instance.currentMode.availableRandomEvents[i + randomEventViewOffset]];
                if (EditorController.Instance.levelData.randomEvents.Contains(EditorController.Instance.currentMode.availableRandomEvents[i + randomEventViewOffset]))
                {
                    randomEventButtons[i].image.color = Color.white;
                }
                else
                {
                    randomEventButtons[i].image.color = Color.black;
                }
            }
        }

        public void EventHighlight(int index)
        {
            if ((index + randomEventViewOffset) >= EditorController.Instance.currentMode.availableRandomEvents.Count) return;
            EditorController.Instance.tooltipController.UpdateTooltip("Ed_RandomEvent_" + EditorController.Instance.currentMode.availableRandomEvents[index + randomEventViewOffset]);
        }

        public void ToggleEvent(string evnt)
        {
            if (EditorController.Instance.levelData.randomEvents.Contains(evnt))
            {
                EditorController.Instance.levelData.randomEvents.Remove(evnt);
            }
            else
            {
                EditorController.Instance.levelData.randomEvents.Add(evnt);
            }
            handler.somethingChanged = true;
            RefreshEventView();
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("levelTime:"))
            {
                float byAmount = float.Parse(message.Replace("levelTime:", ""));
                EditorController.Instance.levelData.timeLimit = Mathf.Clamp((EditorController.Instance.levelData.timeLimit + byAmount), 0f, 5999f); // 356459 is 99:59 in seconds
                handler.somethingChanged = true;
                Refresh();
            }
            if (message.StartsWith("event"))
            {
                int index = int.Parse(message.Replace("event", ""));
                int selectableEventsIndex = index + randomEventViewOffset;
                if (selectableEventsIndex >= EditorController.Instance.currentMode.availableRandomEvents.Count)
                {
                    return;
                }
                ToggleEvent(EditorController.Instance.currentMode.availableRandomEvents[index + randomEventViewOffset]);
            }
            switch (message)
            {
                case "elevatorTitleChanged":
                    handler.somethingChanged = true;
                    EditorController.Instance.levelData.elevatorTitle = elevatorText.text;
                    break;
                case "nextEvent":
                    randomEventViewOffset = Mathf.Clamp(randomEventViewOffset + 1, 0, EditorController.Instance.currentMode.availableRandomEvents.Count - randomEventButtons.Length);
                    RefreshEventView();
                    break;
                case "prevEvent":
                    randomEventViewOffset = Mathf.Clamp(randomEventViewOffset - 1, 0, EditorController.Instance.currentMode.availableRandomEvents.Count - randomEventButtons.Length);
                    RefreshEventView();
                    break;
                case "initialEventTimeChanged":
                    if (float.TryParse((string)data, out float initResult))
                    {
                        handler.somethingChanged = true;
                        EditorController.Instance.levelData.initialRandomEventGap = Mathf.Abs(initResult);
                    }
                    Refresh();
                    break;
                case "minEventTimeChanged":
                    if (float.TryParse((string)data, out float minResult))
                    {
                        handler.somethingChanged = true;
                        EditorController.Instance.levelData.minRandomEventGap = Mathf.Abs(minResult);
                    }
                    Refresh();
                    break;
                case "maxEventTimeChanged":
                    if (float.TryParse((string)data, out float maxResult))
                    {
                        handler.somethingChanged = true;
                        EditorController.Instance.levelData.maxRandomEventGap = Mathf.Abs(maxResult);
                    }
                    Refresh();
                    break;
            }
        }
    }
}
