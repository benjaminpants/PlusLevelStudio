using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.EventSystems;
using static UnityEngine.Random;

namespace PlusLevelStudio
{
    public class BaldiStandaloneInputModule : StandaloneInputModule
    {
        static FieldInfo _clickId = AccessTools.Field(typeof(CursorController), "clickId");
        static FieldInfo _pointerEventData = AccessTools.Field(typeof(CursorController), "pointerEventData");
        static FieldInfo _results = AccessTools.Field(typeof(CursorController), "results");

        bool clickWasPressedLastFrame = false;

        protected override MouseState GetMousePointerEventData(int id)
        {
            if (CursorController.Instance == null) return base.GetMousePointerEventData(id);
            MouseState mouseState = new MouseState();
            string clickId = (string)_clickId.GetValue(CursorController.Instance);
            PointerEventData eventData = (PointerEventData)_pointerEventData.GetValue(CursorController.Instance);
            PointerEventData.FramePressState pressState = PointerEventData.FramePressState.NotChanged;
            if (Singleton<InputManager>.Instance.GetDigitalInput(clickId, true))
            {
                pressState = PointerEventData.FramePressState.PressedAndReleased;
            }
            mouseState.SetButtonState(PointerEventData.InputButton.Left, pressState, eventData);
            mouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, eventData);
            mouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, eventData);

            List<RaycastResult> results = (List<RaycastResult>)_results.GetValue(CursorController.Instance);
            for (int i = 0; i < results.Count && i < 1; i++)
            {
                if (!(results[i].gameObject.tag == "Button"))
                {
                    continue;
                }

                MenuButton button = results[i].gameObject.GetComponent<MenuButton>();
                if (button == null) continue;
                eventData.pointerEnter = button.gameObject;
                break;
            }

            return mouseState;
        }
    }
}
