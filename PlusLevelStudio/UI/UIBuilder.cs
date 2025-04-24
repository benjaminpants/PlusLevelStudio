using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PlusLevelStudio.UI
{

    public abstract class UIElementBuilder
    {

        protected Vector2 ConvertToVector2(JToken value)
        {
            float[] floatArray = value.ToObject<float[]>(); //lets hope this works?
            return new Vector2(floatArray[0], floatArray[1]);
        }

        public abstract GameObject Build(GameObject parent, Dictionary<string, JToken> data);
    }

    public static class UIBuilder
    {
        public static Dictionary<string, UIElementBuilder> elementBuilders = new Dictionary<string, UIElementBuilder>();

        static Dictionary<string, JToken> mostRecentlyParsedElement;

        static JObject mostRecentlyParsedObject;


        public static GameObject BuildUIFromFile(Canvas canvas, string name, string path)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(canvas.transform, false);
            JObject parsedFile = JObject.Parse(File.ReadAllText(path));


            // handle defines
            Dictionary<string, JToken> defines = new Dictionary<string, JToken>();
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

                elementBuilders[elementType].Build(obj, elementData);
            }


            return obj;
        }
    }
}
