using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorInterface
    {

        private static T AddDoorNoArray<T>(string visualName, Material mask, Material[] sideMaterials = null) where T : DoorDisplay
        {
            GameObject standardDoorDisplayObject = new GameObject(visualName);
            standardDoorDisplayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject sideAQuad = LevelStudioPlugin.CreateQuad("SideA", mask, Vector3.zero, Vector3.zero);
            GameObject sideBQuad = LevelStudioPlugin.CreateQuad("SideB", mask, Vector3.zero, new Vector3(0f, 180f, 0f));
            sideAQuad.transform.SetParent(standardDoorDisplayObject.transform);
            sideBQuad.transform.SetParent(standardDoorDisplayObject.transform);
            EditorDeletableObject doorDisplayDeletable = standardDoorDisplayObject.AddComponent<EditorDeletableObject>();
            doorDisplayDeletable.AddRenderer(sideAQuad.GetComponent<MeshRenderer>(), "none");
            doorDisplayDeletable.AddRenderer(sideBQuad.GetComponent<MeshRenderer>(), "none");
            if (sideMaterials != null)
            {
                MaterialModifier.ChangeOverlay(sideAQuad.GetComponent<MeshRenderer>(), sideMaterials[0]);
                MaterialModifier.ChangeOverlay(sideBQuad.GetComponent<MeshRenderer>(), sideMaterials[1]);
            }
            T standardDoorDisplayBehavior = standardDoorDisplayObject.AddComponent<T>();
            standardDoorDisplayBehavior.sideA = sideAQuad.GetComponent<MeshRenderer>();
            standardDoorDisplayBehavior.sideB = sideBQuad.GetComponent<MeshRenderer>();
            standardDoorDisplayObject.AddComponent<BoxCollider>().size = new Vector3(10f, 10f, 0.5f);
            standardDoorDisplayObject.layer = LevelStudioPlugin.editorInteractableLayer;
            return standardDoorDisplayBehavior;
        }

        /// <summary>
        /// Creates the DoorDisplay of the specified type and adds it to the editor's keys.
        /// (Does not add nor create the tool for placing it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="isTileBasedObject"></param>
        /// <param name="mask"></param>
        /// <param name="sideMaterials"></param>
        /// <returns></returns>
        public static T AddDoor<T>(string key, bool isTileBasedObject, Material mask, Material[] sideMaterials = null) where T : DoorDisplay
        {
            T standardDoorDisplayBehavior = AddDoorNoArray<T>(key + "_" + typeof(T).Name,mask, sideMaterials);
            LevelStudioPlugin.Instance.doorDisplays.Add(key, standardDoorDisplayBehavior);
            LevelStudioPlugin.Instance.doorIsTileBased.Add(key, isTileBasedObject);
            return standardDoorDisplayBehavior;
        }

        /// <summary>
        /// Creates the DoorDisplay of the specified type and adds it to the editor's keys.
        /// (Does not add nor create the tool for placing it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="mask"></param>
        /// <param name="sideMaterials"></param>
        /// <returns></returns>
        public static T AddWindow<T>(string key, Material mask, Material[] sideMaterials = null) where T : DoorDisplay
        {
            T standardDoorDisplayBehavior = AddDoorNoArray<T>(key + "_Window" + typeof(T).Name, mask, sideMaterials);
            LevelStudioPlugin.Instance.windowDisplays.Add(key, standardDoorDisplayBehavior);
            return standardDoorDisplayBehavior;
        }

        /// <summary>
        /// Generates the visual for the specified elevator prefab.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prefab"></param>
        public static void AddExit(string key, Elevator prefab)
        {
            GameObject visual = CloneToPrefabStripMonoBehaviors(prefab.gameObject);
            visual.name = "ElevatorVisual_" + key;
            // TODO: add hitbox and delete component
            AddExit(key, visual);
        }

        /// <summary>
        /// Adds the specified exit visual to the editors database. Make sure your visual has a collision box and an EditorDeleteComponent attached!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="visual"></param>
        public static void AddExit(string key, GameObject visual)
        {
            LevelStudioPlugin.Instance.exitDisplays.Add(key, visual);
        }

        /// <summary>
        /// Clones the GameObject, strips the clone's components, and then converts it to a prefab and returns.
        /// </summary>
        /// <param name="toStrip"></param>
        /// <returns></returns>
        public static GameObject CloneToPrefabStripMonoBehaviors(GameObject toStrip, Type[] toPreserve = null)
        {
            GameObject obj = GameObject.Instantiate(toStrip, MTM101BaldiDevAPI.prefabTransform);
            obj.name = obj.name.Replace("(Clone)", "_Stripped");
            MonoBehaviour[] behaviors = obj.GetComponentsInChildren<MonoBehaviour>();
            foreach (var behavior in behaviors)
            {
                if (toPreserve != null)
                {
                    if (toPreserve.Contains(behavior.GetType()))
                    {
                        continue;
                    }
                }
                GameObject.DestroyImmediate(behavior);
            }
            return obj;
        }

        public static GameObject AddActivityVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_Visual");
            MovableObjectInteraction movableObjectInteract = clone.AddComponent<MovableObjectInteraction>();
            movableObjectInteract.allowedRotations = RotateAxis.Flat;
            movableObjectInteract.allowedAxis = MoveAxis.All;
            clone.AddComponent<EditorDeletableObject>().AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
            
            LevelStudioPlugin.Instance.activityDisplays.Add(key, clone);
            return clone;
        }

        public static GameObject AddNPCVisual(string key, NPC npc)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(npc.gameObject, new Type[] { typeof(BillboardUpdater) });
            clone.name = clone.name.Replace("_Stripped", "_Visual");
            Collider[] colliders = clone.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject.DestroyImmediate(colliders[i]);
            }
            Animator[] animators = clone.GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++)
            {
                GameObject.DestroyImmediate(animators[i]);
            }
            clone.layer = LevelStudioPlugin.editorInteractableLayer;
            BoxCollider collider = clone.AddComponent<BoxCollider>();
            collider.center = Vector3.down * 5f;
            collider.size = new Vector3(10f, 0.1f, 10f);
            collider.isTrigger = true;
            clone.gameObject.AddComponent<EditorDeletableObject>().AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            LevelStudioPlugin.Instance.npcDisplays.Add(key, clone);
            return clone;
        }

        public static GameObject AddStructureGenericVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_GenericVisual");
            clone.gameObject.AddComponent<EditorDeletableObject>().AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.layer = LevelStudioPlugin.editorInteractableLayer;
            LevelStudioPlugin.Instance.genericStructureDisplays.Add(key, clone);
            return clone;
        }

        public static EditorBasicObject AddObjectVisual(string key, GameObject obj, bool useRegularColliderAsEditorHitbox)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj, new Type[] { typeof(BillboardUpdater), typeof(AnimatedSpriteRotator) });
            clone.name = clone.name.Replace("_Stripped", "_Visual");
            EditorBasicObject basic = clone.AddComponent<EditorBasicObject>();
            clone.AddComponent<EditorDeletableObject>().AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            basic.ingameLayer = clone.layer;
            basic.ingameColliders.AddRange(clone.GetComponentsInChildren<Collider>());
            basic.ingameColliders.ForEach(x => x.enabled = false);
            basic.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;


            if (useRegularColliderAsEditorHitbox)
            {
                if (basic.ingameColliders.Count == 0)
                {
                    throw new Exception("useRegularColliderAsEditorHitbox is set, but object with key: " + key + " has no hitbox!");
                }
                Collider baseCollider = basic.ingameColliders[0];
                Collider edCollider = null;
                if (baseCollider is BoxCollider)
                {
                    BoxCollider box = basic.gameObject.AddComponent<BoxCollider>();
                    box.size = ((BoxCollider)baseCollider).size;
                    box.center = ((BoxCollider)baseCollider).center;
                    edCollider = box;
                }
                else if (baseCollider is SphereCollider)
                {
                    SphereCollider sphere = basic.gameObject.AddComponent<SphereCollider>();
                    sphere.center = ((SphereCollider)baseCollider).center;
                    sphere.radius = ((SphereCollider)baseCollider).radius;
                    edCollider = sphere;
                }
                else if (baseCollider is MeshCollider)
                {
                    MeshCollider mesh = basic.gameObject.AddComponent<MeshCollider>();
                    mesh.convex = ((MeshCollider)baseCollider).convex;
                    mesh.sharedMesh = ((MeshCollider)baseCollider).sharedMesh; // ????
                    edCollider = mesh;
                }
                else if (baseCollider is CapsuleCollider)
                {
                    CapsuleCollider capsule = basic.gameObject.AddComponent<CapsuleCollider>();
                    capsule.radius = ((CapsuleCollider)baseCollider).radius;
                    capsule.height = ((CapsuleCollider)baseCollider).height;
                    capsule.center = ((CapsuleCollider)baseCollider).center;
                    capsule.direction = ((CapsuleCollider)baseCollider).direction;
                    edCollider = capsule;
                }
                else
                {
                    throw new NotImplementedException("Unknown collider type:" + baseCollider.GetType() + "!");
                }
                basic.editorCollider = edCollider;
            }
            LevelStudioPlugin.Instance.basicObjectDisplays.Add(key, basic);
            return basic;
        }

        public static EditorBasicObject AddObjectVisualWithCustomBoxCollider(string key, GameObject obj, Vector3 size, Vector3 center)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            BoxCollider col = basic.gameObject.AddComponent<BoxCollider>();
            col.size = size;
            col.center = center;
            basic.editorCollider = col;
            return basic;
        }

        public static EditorBasicObject AddObjectVisualWithCustomSphereCollider(string key, GameObject obj, float radius, Vector3 center)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            SphereCollider col = basic.gameObject.AddComponent<SphereCollider>();
            col.radius = radius;
            col.center = center;
            basic.editorCollider = col;
            return basic;
        }

        public static EditorBasicObject AddObjectVisualWithCustomCapsuleCollider(string key, GameObject obj, float radius, float height, int direction, Vector3 center)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            CapsuleCollider col = basic.gameObject.AddComponent<CapsuleCollider>();
            col.radius = radius;
            col.height = height;
            col.direction = direction;
            col.center = center;
            basic.editorCollider = col;
            return basic;
        }

        public static EditorBasicObject AddObjectVisualWithMeshCollider(string key, GameObject obj, bool convex)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            MeshCollider col = basic.gameObject.AddComponent<MeshCollider>();
            col.convex = convex;
            basic.editorCollider = col;
            return basic;
        }

        public static T AddRoomVisualManager<T>(string forType) where T : EditorRoomVisualManager
        {
            if (forType == "hall") throw new Exception("Can't create RoomVisualManager for hallways!");
            GameObject roomVisual = new GameObject(forType + "_VisualManager");
            roomVisual.ConvertToPrefab(true);
            T visual = roomVisual.AddComponent<T>();
            LevelStudioPlugin.Instance.roomVisuals.Add(forType, visual);
            return visual;
        }
    }
}
