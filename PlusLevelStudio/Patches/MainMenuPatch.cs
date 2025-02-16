using HarmonyLib;
using MTM101BaldAPI.UI;
using PlusLevelStudio.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(MainMenu))]
    [HarmonyPatch("Start")]
    internal class MainMenuPatch
    {
        static void Postfix(MainMenu __instance)
        {
            Image image = UIHelpers.CreateImage(LevelStudioPlugin.Instance.assetMan.Get<Sprite>("EditorButton"), __instance.transform, Vector3.zero, false, 1f);
            image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchoredPosition = new Vector2(60, -88);
            CursorController.Instance.transform.SetAsLastSibling();
            __instance.transform.Find("Bottom").SetAsLastSibling();
            __instance.transform.Find("BlackCover").SetAsLastSibling();
            if (LevelStudioPlugin.Instance.isFucked)
            {
                image.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("EditorButtonFail");
                return;
            }
            StandardMenuButton button = image.gameObject.ConvertToButton<StandardMenuButton>();
            button.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("EditorButtonGlow");
            button.unhighlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("EditorButton");
            button.swapOnHigh = true;
            button.transitionOnPress = true;
            button.transitionTime = 0.0167f;
            button.transitionType = UiTransition.Dither;
            EditorModeSelectionMenu menu = EditorModeSelectionMenu.Build();
            menu.mainMenu = GameObject.Find("Menu"); ;
            button.OnPress.AddListener(() =>
            {
                menu.mainMenu.SetActive(false);
                menu.gameObject.SetActive(true);
            });
        }
    }
}
