using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class HotSlotToolboxBuilder : HotSlotBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            HotSlotScript hsc = b.GetComponent<HotSlotScript>();
            hsc.button.OnPress = new UnityEngine.Events.UnityEvent(); // clear
            hsc.button.OnRelease = new UnityEngine.Events.UnityEvent(); // clear?
            DraggableHotslotScript drag = b.AddComponent<DraggableHotslotScript>();
            drag.hotSlot = hsc;
            drag.handler = handler;

            GameObject deleteBGameObject = new GameObject("Delete");
            deleteBGameObject.transform.SetParent(b.transform);
            Image deleteImage = deleteBGameObject.AddComponent<Image>();
            deleteImage.rectTransform.anchoredPosition3D = Vector3.zero;
            deleteImage.transform.localPosition = Vector3.zero;
            deleteImage.transform.localRotation = Quaternion.identity;
            deleteImage.rectTransform.anchorMin = Vector2.zero;
            deleteImage.rectTransform.anchorMax = Vector2.zero;
            deleteImage.rectTransform.pivot = Vector2.zero;
            deleteImage.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            deleteImage.sprite = GetSprite("SlotDelete");
            deleteImage.rectTransform.sizeDelta = new Vector2(8f, 8f);
            deleteImage.transform.localScale = Vector3.one; // unity what the fuck

            StandardMenuButton smb = deleteBGameObject.ConvertToButton<StandardMenuButton>();
            smb.highlightedSprite = GetSprite("SlotDeleteHover");
            smb.unhighlightedSprite = GetSprite("SlotDelete");
            smb.swapOnHigh = true;
            smb.gameObject.SetActive(false);
            hsc.deleteButton = smb;

            /*deleteImage.rectTransform.localScale = Vector3.one;
            deleteImage.rectTransform.sizeDelta = new Vector2(8,8);
            deleteImage.rectTransform.anchorMin = new Vector2(0,1);
            deleteImage.rectTransform.anchorMax = new Vector2(0,1);
            deleteImage.rectTransform.pivot = new Vector2(0, 1);
            deleteImage.rectTransform.anchoredPosition = Vector2.zero;
            deleteImage.sprite = GetSprite("SlotDelete");*/

            hsc.usesToolOverride = true;

            return b;
        }
    }

    public class DraggableHotslotScript : MonoBehaviour
    {
        public HotSlotScript hotSlot;
        public UIExchangeHandler handler;
        public RectTransform rectTransform;
        public Vector3 startingLocalPos;
        bool beingHeld = false;
        bool beingDraggedOverNewSlot = false;
        float timeBeingHeld = 0f;
        const float threshold = 0.5f;
        void Update()
        {
            if (beingDraggedOverNewSlot)
            {
                transform.localPosition = CursorController.Instance.transform.localPosition + CursorController.Instance.cursorTransform.localPosition;
                transform.localPosition += Vector3.left * 20f;
                transform.localPosition += Vector3.up * 20f;
                return;
            }
            if (beingHeld)
            {
                timeBeingHeld += Time.deltaTime;
                if (timeBeingHeld >= threshold)
                {
                    handler.SendInteractionMessage("hide", this.gameObject);
                    beingDraggedOverNewSlot = true;
                    hotSlot.button.eventOnHigh = false;
                }
            }
        }

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            startingLocalPos = transform.localPosition;
        }

        void Start()
        {
            hotSlot.button.OnPress = new UnityEngine.Events.UnityEvent(); // clear
            hotSlot.button.OnRelease = new UnityEngine.Events.UnityEvent(); // clear?
            hotSlot.button.OnPress.AddListener(() =>
            {
                if (hotSlot.currentTool == null) return;
                beingHeld = true;
            });
            hotSlot.button.OnRelease.AddListener(() =>
            {
                if (hotSlot.currentTool == null) return;
                hotSlot.button.eventOnHigh = true;
                if (!beingDraggedOverNewSlot)
                {
                    handler.SendInteractionMessage("exit");
                    EditorController.Instance.SwitchToToolToolbox(hotSlot.currentTool);
                }
                else
                {
                    beingDraggedOverNewSlot = false;
                    EditorController.Instance.SwitchCurrentHoveringSlot(hotSlot.currentTool);
                    handler.SendInteractionMessage("show");
                    transform.localPosition = startingLocalPos;
                }
                beingHeld = false;
                timeBeingHeld = 0f;
            });
            hotSlot.button.eventOnHigh = true;
            hotSlot.button.OnHighlight.AddListener(() =>
            {
                handler.SendInteractionMessage("tip", hotSlot.currentTool);
            });
        }
    }
}
