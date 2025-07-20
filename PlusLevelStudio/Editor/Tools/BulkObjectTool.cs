using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{

    public class BulkObjectData
    {
        public string prefab;
        public Vector3 position;
        public Vector3 rotation;

        public BulkObjectData(string type, Vector3 position, Vector3 euler)
        {
            this.prefab = type;
            this.position = position;
            rotation = euler;
        }

        public BulkObjectData(string type, Vector3 position) : this(type, position, Vector3.zero)
        {
            
        }

        public BulkObjectData(BulkObjectData data)
        {
            prefab = data.prefab;
            position = data.position;
            rotation = data.rotation;
        }
    }

    public class BulkObjectTool : DoorTool
    {
        public override string id => "bulkobject_" + type;
        public BulkObjectData[] data = new BulkObjectData[0];

        internal BulkObjectTool(string type, BulkObjectData[] data) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/bulkobject_" + type), data)
        {

        }

        public BulkObjectTool(string type, Sprite sprite, BulkObjectData[] data) : base(type, sprite)
        {
            this.data = data;
        }

        public override void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            for (int i = 0; i < data.Length; i++)
            {
                Vector3 newPosition = (dir.ToRotation() * data[i].position) + pos.Value.ToWorld();
                Quaternion rotation = Quaternion.Euler(data[i].rotation) * dir.ToRotation();

                BasicObjectLocation bob = new BasicObjectLocation();
                bob.prefab = data[i].prefab;
                bob.position = newPosition;
                bob.rotation = rotation;

                EditorController.Instance.levelData.objects.Add(bob);
                EditorController.Instance.AddVisual(bob);
            }

            EditorController.Instance.SwitchToTool(null);
        }
    }

    public class BulkObjectRandomizedTool : BulkObjectTool
    {
        public List<KeyValuePair<string, string>> replacements = new List<KeyValuePair<string, string>>();
        public BulkObjectData[] originalData = new BulkObjectData[0];

        internal BulkObjectRandomizedTool(string type, BulkObjectData[] data, List<KeyValuePair<string, string>> replacements) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/bulkobject_" + type), data)
        {
            originalData = data;
            this.replacements = replacements;
        }
        public BulkObjectRandomizedTool(string type, Sprite sprite, BulkObjectData[] data, List<KeyValuePair<string,string>> replacements) : base(type, sprite, data)
        {
            originalData = data;
            this.replacements = replacements;
        }

        public override void OnPlaced(Direction dir)
        {
            data = new BulkObjectData[originalData.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new BulkObjectData(originalData[i]);
            }
            List<KeyValuePair<string, string>> remainingReplacements = new List<KeyValuePair<string, string>>(replacements);
            List<BulkObjectData> potentialSubjects = new List<BulkObjectData>(); // define here so we don't fill the garbage collector with lists
            while (remainingReplacements.Count > 0)
            {
                KeyValuePair<string, string> chosenKVP = remainingReplacements[UnityEngine.Random.Range(0, remainingReplacements.Count)];
                remainingReplacements.Remove(chosenKVP);
                potentialSubjects.AddRange(data);
                // there may be multiple of the same subject, or we may have ran out if poorly configured
                while (potentialSubjects.Count > 0)
                {
                    BulkObjectData chosenObject = potentialSubjects[UnityEngine.Random.Range(0, potentialSubjects.Count)];
                    potentialSubjects.Remove(chosenObject);
                    if (chosenObject.prefab != chosenKVP.Key) continue;
                    chosenObject.prefab = chosenKVP.Value;
                    break;
                }
                potentialSubjects.Clear();
            }
            base.OnPlaced(dir);
        }
    }
}
