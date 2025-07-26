using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using PlusLevelStudio.Editor;

namespace PlusLevelStudio.UI
{

    public abstract class UIElementBuilder
    {

        protected Sprite GetSprite(string spriteName)
        {
            return LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>(spriteName);
        }

        protected Sprite GetSprite(JToken token)
        {
            return GetSprite(token.Value<string>());
        }

        protected Vector2 ConvertToVector2(JToken value)
        {
            float[] floatArray = value.ToObject<float[]>();
            return new Vector2(floatArray[0], floatArray[1]);
        }

        protected Color ConvertToColor(JToken value)
        {
            float[] floatArray = value.ToObject<float[]>();
            if (floatArray.Length == 3)
            {
                return new Color(floatArray[0] / 255f, floatArray[1] / 255f, floatArray[2] / 255f);
            }
            else
            {
                return new Color(floatArray[0] / 255f, floatArray[1] / 255f, floatArray[2] / 255f, floatArray[3] / 255f);
            }
        }

        public abstract GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data);
    }

    public abstract class UIExchangeHandler : MonoBehaviour
    {
        /// <summary>
        /// Sends an interaction message to this exchange handler.
        /// </summary>
        /// <param name="message"></param>
        public abstract void SendInteractionMessage(string message, object data = null);

        /// <summary>
        /// Gets the respective state boolean.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool GetStateBoolean(string key);

        /// <summary>
        /// Called when the UI is finished being built.
        /// </summary>
        public abstract void OnElementsCreated();
    }

    /// <summary>
    /// A dummy UIExchangeHandler that just logs whenever its methods are called. Does nothing and shouldn't be used in anything.
    /// </summary>
    public class DummyUIExchangeHandler : UIExchangeHandler
    {
        public override bool GetStateBoolean(string key)
        {
            Debug.Log("Get State Boolean:" + key);
            return false;
        }

        public override void OnElementsCreated()
        {
            Debug.Log("Elements Created");
        }

        public override void SendInteractionMessage(string message, object data)
        {
            Debug.Log("Interaction message sent: " + message);
        }
    }

    public class EditorOverlayUIExchangeHandler : UIExchangeHandler
    {
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            
        }

        public virtual bool OnExit()
        {
            return true;
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message == "exit")
            {
                if (OnExit())
                {
                    EditorController.Instance.RemoveUI(gameObject);
                }
            }
        }
    }

    public static class UIBuilder
    {
        public static Dictionary<string, UIElementBuilder> elementBuilders = new Dictionary<string, UIElementBuilder>();

        static Dictionary<string, JToken> mostRecentlyParsedElement;

        static JObject mostRecentlyParsedObject;

        public static Dictionary<string, JToken> globalDefines = new Dictionary<string, JToken>();

        public static void LoadGlobalDefinesFromFile(string path)
        {
            globalDefines.Clear();
            JObject parsedFile = JObject.Parse(File.ReadAllText(path));
            foreach (JProperty child in parsedFile.Children())
            {
                globalDefines.Add(child.Name, child.Value);
            }
        }

        public static T BuildUIFromFile<T>(RectTransform parent, string name, string path) where T : UIExchangeHandler
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = parent.sizeDelta;
            JObject parsedFile = JObject.Parse(File.ReadAllText(path));

            T handler = obj.AddComponent<T>();


            // handle defines
            Dictionary<string, JToken> defines = new Dictionary<string, JToken>(globalDefines);
            JToken definesToken = parsedFile["defines"];

            foreach (JProperty child in definesToken.Children())
            {
                defines.Add(child.Name, child.Value);
            }

            // now handle elements themselves
            JArray elementsArray = parsedFile["elements"] as JArray;
            for (int i = 0; i < elementsArray.Count; i++)
            {
                JObject currentElement = elementsArray[i] as JObject;
                mostRecentlyParsedObject = currentElement;
                // process the element type so we know what builder to give this to
                //string elementType = currentElement["type"].Value<string>();

                Dictionary<string, JToken> elementData = new Dictionary<string, JToken>();

                foreach (JProperty property in currentElement.Children())
                {
                    if (property.Value.Type == JTokenType.String)
                    {
                        string propertyString = property.Value.Value<string>();
                        // strings beginning with #s are defines and thus should copy values from the defines table
                        if (propertyString.StartsWith("#"))
                        {
                            elementData.Add(property.Name, defines[propertyString.Substring(1)].DeepClone());
                            continue;
                        }
                    }
                    elementData.Add(property.Name, property.Value);
                }

                mostRecentlyParsedElement = elementData;

                // handle this last just incase some evil lunatic decides to use macros for defining types
                string elementType = elementData["type"].ToString();

                elementBuilders[elementType].Build(obj, handler, elementData);
            }

            handler.OnElementsCreated();


            return handler;
        }
    }
}
