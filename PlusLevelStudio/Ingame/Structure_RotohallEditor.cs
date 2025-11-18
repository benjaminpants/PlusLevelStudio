using HarmonyLib;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_RotohallEditor : Structure_Rotohalls, ISerializationCallbackReceiver
    {
        public RotoHall rotoHallPre;

        // this is probably overengineered but. idk if someone wanted to make custom rotohalls i just made their life a lot easier
        // also storing it in a dictionary makes the Load function super clean as i dont need if statements!!!!!
        public Dictionary<string, Sprite> rotohallSprites = new Dictionary<string, Sprite>();
        public Dictionary<string, CylinderShape> cylinderShapes = new Dictionary<string, CylinderShape>();

        public GameButton buttonPre;

        [SerializeField]
        private string[] spriteKeys;
        [SerializeField]
        private Sprite[] sprites;
        [SerializeField]
        private string[] cylinderKeys;
        [SerializeField]
        private CylinderShape[] cylinders;


        static FieldInfo _currentDir = AccessTools.Field(typeof(RotoHall), "currentDir");
        static FieldInfo _cylinder = AccessTools.Field(typeof(RotoHall), "cylinder");

        public override void Load(List<StructureData> dataList)
        {
            Debug.Log("queue");
            Queue<StructureData> dataQueue = new Queue<StructureData>(dataList);
            Debug.Log("queue made");

            // it's... it's BEAUTIFUL.
            while (dataQueue.Count > 0)
            {
                StructureData data = dataQueue.Dequeue();
                Cell cellAt = ec.CellFromPosition(data.position);
                RotoHall rotoHall = GameObject.Instantiate<RotoHall>(rotoHallPre, cellAt.ObjectBase);
                rotoHall.Ec = ec;
                rotoHall.Setup(data.direction, data.prefab.GetComponent<MeshRenderer>(), cylinderShapes[data.prefab.name], cellAt, data.data == 0);
                // roto halls overwrite this for whatever reason so i have to set it back
                _currentDir.SetValue(rotoHall, data.direction);
                ((MeshRenderer)_cylinder.GetValue(rotoHall)).transform.rotation = data.direction.ToRotation();
                rotoHall.AssignMapTile(data.position, rotohallSprites[data.prefab.name]);

                StructureData buttonData = dataQueue.Dequeue();
                GameButton.Build(buttonPre, ec, buttonData.position, buttonData.direction).SetUp(rotoHall);
            }
        }

        public void OnBeforeSerialize()
        {
            spriteKeys = new string[rotohallSprites.Count];
            sprites = new Sprite[rotohallSprites.Count];
            int index = 0;
            foreach (var item in rotohallSprites)
            {
                spriteKeys[index] = item.Key;
                sprites[index] = item.Value;
                index++;
            }
            index = 0;
            cylinderKeys = new string[cylinderShapes.Count];
            cylinders = new CylinderShape[cylinderShapes.Count];
            foreach (var item in cylinderShapes)
            {
                cylinderKeys[index] = item.Key;
                cylinders[index] = item.Value;
                index++;
            }
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < spriteKeys.Length; i++)
            {
                rotohallSprites.Add(spriteKeys[i], sprites[i]);
            }
            for (int i = 0; i < cylinderKeys.Length; i++)
            {
                cylinderShapes.Add(cylinderKeys[i], cylinders[i]);
            }
        }
    }
}
