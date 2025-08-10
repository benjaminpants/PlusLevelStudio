using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class TextBoxBuilder : TextBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            GameObject collision = new ImageBuilder().Build(parent, handler, data);
            collision.name += "_Collision";
            collision.gameObject.tag = "Button";
            collision.GetComponent<Image>().color = Color.clear;
            EditorTextBox box = collision.AddComponent<EditorTextBox>();
            box.text = b.GetComponent<TextMeshProUGUI>();
            box.characterLimit = data["characterLimit"].Value<int>();
            box.upperAll = data["upper"].Value<bool>();
            if (data.ContainsKey("onType"))
            {
                box.typeMessage = data["onType"].Value<string>();
            }
            if (data.ContainsKey("onFinish"))
            {
                box.typeDoneMessage = data["onFinish"].Value<string>();
            }
            else
            {
                box.typeDoneMessage = null;
            }
            box.handler = handler;
            if (data.ContainsKey("allowedCharacters"))
            {
                box.allowedCharacters = data["allowedCharacters"].Value<string>();
            }
            if (data.ContainsKey("tooltip"))
            {
                string key = data["tooltip"].Value<string>();
                box.eventOnHigh = true;
                box.OnHighlight.AddListener(() => EditorController.Instance.tooltipController.UpdateTooltip(key));
                box.OffHighlight.AddListener(() => EditorController.Instance.tooltipController.CloseTooltip());
            }
            return b;
        }
    }

    // TODO: replace with proper text box?
    public class EditorTextBox : MenuButton
    {
        public TextMeshProUGUI text;
        bool typing = false;
        public int characterLimit = 8;
        public bool upperAll = false;
        public string allowedCharacters = null;
        public string typeMessage = null;
        public string typeDoneMessage;
        public UIExchangeHandler handler;
        float timeWithBackDown = 0f;
        public UnityEvent OnHighlight = new UnityEvent();
        public UnityEvent OffHighlight = new UnityEvent();
        public bool eventOnHigh = false;
        public override void Press()
        {
            //base.Press();
            CursorController.Instance.Hide(true);
            typing = true;
        }

        public override void Highlight()
        {
            text.fontStyle = FontStyles.Underline;
            highlighted = true;
            if (!wasHighlighted && eventOnHigh)
            {
                OnHighlight.Invoke();
            }
            wasHighlighted = true;
        }

        void Update()
        {
            if (!highlighted && wasHighlighted)
            {
                text.fontStyle = FontStyles.Normal;
                wasHighlighted = false;
                if (eventOnHigh)
                {
                    OffHighlight.Invoke();
                }
            }
            highlighted = false;
            if (!typing) return;
            if (Input.GetKey(KeyCode.Backspace))
            {
                timeWithBackDown += Time.deltaTime;
            }
            else
            {
                timeWithBackDown = -1f;
            }
            text.fontStyle = FontStyles.Underline;
            bool sendUpdate = false;
            if (Input.anyKeyDown && Input.inputString.Length > 0 && !char.IsControl(Input.inputString, 0))
            {
                if ((allowedCharacters == null) || allowedCharacters.Contains(Input.inputString[0].ToString().ToUpper()))
                {
                    text.text += Input.inputString[0].ToString();
                    sendUpdate = true;
                }
                if (upperAll)
                {
                    text.text = text.text.ToUpper();
                }
            }
            if (Input.GetKeyDown(KeyCode.Backspace) || (timeWithBackDown > 0f))
            {
                timeWithBackDown = Mathf.Min(timeWithBackDown, 0.05f);
                if (text.text.Length > 0)
                {
                    text.text = text.text.Remove(text.text.Length - 1);
                    sendUpdate = true;
                }
            }
            if (text.text.Length > characterLimit)
            {
                text.text = text.text.Remove(characterLimit);
                sendUpdate = true;
            }
            if (sendUpdate && (typeMessage != null))
            {
                handler.SendInteractionMessage(typeMessage, text.text);
            }
            //text.text = baseText;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CursorController.Instance.Hide(false);
                typing = false;
                text.fontStyle = FontStyles.Normal;
                if (typeDoneMessage != null)
                {
                    handler.SendInteractionMessage(typeDoneMessage, text.text);
                }
            }
        }
    }
}
