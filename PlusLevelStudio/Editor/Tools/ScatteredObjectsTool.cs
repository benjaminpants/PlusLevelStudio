using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ScatteredObjectsTool : EditorTool
    {
        protected string type;
        protected IntVector2? startVector = null;
        protected bool inScaleMode = false;

        protected int minCountPerTile;
        protected int maxCountPerTile;
        protected bool randomizeRotation;
        protected WeightedSelection<string>[] objects;
        protected Bounds area;

        public override string id => "scatter_" + type;

        internal ScatteredObjectsTool(string type, int minCountPerTile, int maxCountPerTile, Bounds area, string[] objects, bool randomizeRotations) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/scatter_" + type), minCountPerTile, maxCountPerTile, area, objects, randomizeRotations)
        {

        }

        internal ScatteredObjectsTool(string type, int minCountPerTile, int maxCountPerTile, Bounds area, WeightedSelection<string>[] objects, bool randomizeRotations) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/scatter_" + type), minCountPerTile, maxCountPerTile, area, objects, randomizeRotations)
        {

        }

        public ScatteredObjectsTool(string type, Sprite sprite, int minCountPerTile, int maxCountPerTile, Bounds area, string[] objects, bool randomizeRotations)
        {
            this.type = type;
            this.sprite = sprite;
            this.minCountPerTile = minCountPerTile;
            this.maxCountPerTile = maxCountPerTile;
            this.area = area;
            this.objects = new WeightedSelection<string>[objects.Length];
            for (int i = 0; i < this.objects.Length; i++)
            {
                this.objects[i] = new WeightedSelection<string>()
                {
                    selection = objects[i],
                    weight = 100
                };
            }
            randomizeRotation = randomizeRotations;
        }

        public ScatteredObjectsTool(string type, Sprite sprite, int minCountPerTile, int maxCountPerTile, Bounds area, WeightedSelection<string>[] objects, bool randomizeRotations)
        {
            this.type = type;
            this.sprite = sprite;
            this.minCountPerTile = minCountPerTile;
            this.maxCountPerTile = maxCountPerTile;
            this.area = area;
            this.objects = new WeightedSelection<string>[objects.Length];
            this.objects = objects;
            randomizeRotation = randomizeRotations;
        }

        public override void Begin()
        {
            startVector = null;
        }

        public override bool Cancelled()
        {
            if (inScaleMode)
            {
                inScaleMode = false;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            inScaleMode = false;
            EditorController.Instance.selector.DisableSelection();
        }

        public override bool MousePressed()
        {
            startVector = EditorController.Instance.mouseGridPosition;
            inScaleMode = true;
            return false;
        }

        public override bool MouseReleased()
        {
            if (inScaleMode)
            {
                EditorController.Instance.HoldUndo();
                RectInt rect = startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector());
                List<BasicObjectLocation> locations = new List<BasicObjectLocation>();
                for (int x = rect.min.x; x < rect.max.x; x++)
                {
                    for (int y = rect.min.y; y < rect.max.y; y++)
                    {
                        IntVector2 cellPos = new IntVector2(x, y);
                        if (EditorController.Instance.levelData.RoomIdFromPos(cellPos, true) == 0) continue;
                        Vector3 origin = cellPos.ToWorld();
                        int countPerTile = Mathf.Max(UnityEngine.Random.Range(minCountPerTile, maxCountPerTile + 1),0);
                        for (int i = 0; i < countPerTile; i++)
                        {
                            string objectType = WeightedSelection<string>.RandomSelection(objects);
                            Vector3 offset = new Vector3(UnityEngine.Random.Range(area.min.x,area.max.x), UnityEngine.Random.Range(area.min.y, area.max.y), UnityEngine.Random.Range(area.min.z, area.max.z));
                            Vector3 objectPosition = origin + offset;
                            BasicObjectLocation location = new BasicObjectLocation();
                            location.prefab = objectType;
                            location.position = objectPosition;
                            if (randomizeRotation)
                            {
                                location.rotation = Quaternion.Euler(0f,UnityEngine.Random.Range(0f,360f),0f);
                            }
                            else
                            {
                                location.rotation = Quaternion.identity;
                            }
                            locations.Add(location);
                        }
                    }
                }
                if (locations.Count == 0)
                {
                    EditorController.Instance.CancelHeldUndo();
                    Cancelled(); // go back
                    return false;
                }
                EditorController.Instance.AddHeldUndo();
                for (int i = 0; i < locations.Count; i++)
                {
                    EditorController.Instance.levelData.objects.Add(locations[i]);
                    EditorController.Instance.AddVisual(locations[i]);
                }
                return true;
            }
            return false;
        }

        public override void Update()
        {
            if (inScaleMode)
            {
                if (startVector == null) throw new InvalidOperationException();
                EditorController.Instance.selector.SelectArea(startVector.Value.ToUnityVector().ToRect(EditorController.Instance.mouseGridPosition.ToUnityVector()), null);
            }
            else
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
