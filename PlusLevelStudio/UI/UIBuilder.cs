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
        public abstract GameObject Build(Dictionary<string, JToken> data);
    }

    public static class UIBuilder
    {
        public static Dictionary<string, UIElementBuilder> elementBuilders = new Dictionary<string, UIElementBuilder>();


        public static GameObject BuildUIFromFile(string path)
        {
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
                // process the element type so we know what builder to give this to
                //string elementType = currentElement["type"].Value<string>();

                Dictionary<string, JToken> elementData = new Dictionary<string, JToken>();

                foreach (JProperty property in currentElement.Children())
                {
                    if (property.Type == JTokenType.String)
                    {
                        string propertyString = property.Value<string>();
                        // strings beginning with #s are defines and thus should copy values from the defines table
                        if (propertyString.StartsWith("#"))
                        {
                            elementData.Add(property.Name, defines[propertyString.Substring(1)].DeepClone());
                            continue;
                        }
                    }
                    elementData.Add(property.Name, property.Value);
                }

                // handle this last just incase some evil lunatic decides to use macros for defining types
                string elementType = elementData["type"].Value<string>();
            }


            return null;
        }
    }
}
