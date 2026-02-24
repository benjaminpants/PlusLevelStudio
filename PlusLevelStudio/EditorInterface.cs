using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using PlusLevelStudio.Editor;
using UnityEngine;

namespace PlusLevelStudio
{
    /// <summary>
    /// The status of a door when compiled in game.
    /// </summary>
    public enum DoorIngameStatus
    {
        /// <summary>
        /// When the level is compiled, this door will always be a door assigned to the respective room.
        /// </summary>
        AlwaysDoor,
        /// <summary>
        /// When the level is compiled, this door will always be a tile object.
        /// </summary>
        AlwaysObject,
        /// <summary>
        /// When the level is compiled, if this door is connected to a non-hall room, it'll be a door assigned to that room, otherwise it'll be a tile object.
        /// </summary>
        Smart
    }

    public static class EditorInterface
    {
        // **** PLACE TO INSERT FIELD INFOS BECAUSE IDK WHERE ELSE TO ADD ****
        static FieldInfo _AnimatedRotator_renderer = AccessTools.Field(typeof(AnimatedSpriteRotator), "renderer"),
        _AnimatedRotator_spriteMap = AccessTools.Field(typeof(AnimatedSpriteRotator), "spriteMap"),
        _SpriteRotationMap_overrideSpriteSheet = AccessTools.Field(typeof(SpriteRotationMap), "overrideSpriteSheet"),
        _SpriteRotationMap_spriteSheet = AccessTools.Field(typeof(SpriteRotationMap), "spriteSheet"),
        _SpriteRotator_spriteRenderer = AccessTools.Field(typeof(SpriteRotator), "spriteRenderer"),
        _SpriteRotator_sprites = AccessTools.Field(typeof(SpriteRotator), "sprites");
        private static T AddDoorNoArray<T>(string visualName, Material mask, Material[] sideMaterials = null) where T : DoorDisplay
        {
            GameObject standardDoorDisplayObject = new GameObject(visualName);
            standardDoorDisplayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject sideAQuad = LevelStudioPlugin.CreateQuad("SideA", mask, Vector3.zero, Vector3.zero);
            GameObject sideBQuad = LevelStudioPlugin.CreateQuad("SideB", mask, Vector3.zero, new Vector3(0f, 180f, 0f));
            sideAQuad.transform.SetParent(standardDoorDisplayObject.transform);
            sideBQuad.transform.SetParent(standardDoorDisplayObject.transform);
            EditorRendererContainer container = standardDoorDisplayObject.gameObject.AddComponent<EditorRendererContainer>();
            EditorDeletableObject doorDisplayDeletable = standardDoorDisplayObject.AddComponent<EditorDeletableObject>();
            doorDisplayDeletable.renderContainer = container;
            container.AddRenderer(sideAQuad.GetComponent<MeshRenderer>(), "none");
            container.AddRenderer(sideBQuad.GetComponent<MeshRenderer>(), "none");
            if (sideMaterials != null)
            {
                MaterialModifier.ChangeOverlay(sideAQuad.GetComponent<MeshRenderer>(), sideMaterials[0]);
                MaterialModifier.ChangeOverlay(sideBQuad.GetComponent<MeshRenderer>(), sideMaterials[1]);
            }
            T standardDoorDisplayBehavior = standardDoorDisplayObject.AddComponent<T>();
            standardDoorDisplayBehavior.sideA = sideAQuad.GetComponent<MeshRenderer>();
            standardDoorDisplayBehavior.sideB = sideBQuad.GetComponent<MeshRenderer>();
            BoxCollider boxCol = standardDoorDisplayObject.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(10f, 10f, 0.5f);
            boxCol.isTrigger = true; // to avoid getting detected by the collision system
            standardDoorDisplayObject.layer = LevelStudioPlugin.editorInteractableLayer;
            return standardDoorDisplayBehavior;
        }

        /// <summary>
        /// Creates the DoorDisplay of the specified type and adds it to the editor's keys.
        /// (Does not add nor create the tool for placing it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ingameState"></param>
        /// <param name="mask"></param>
        /// <param name="sideMaterials"></param>
        /// <returns></returns>
        public static T AddDoor<T>(string key, DoorIngameStatus ingameState, Material mask, Material[] sideMaterials = null) where T : DoorDisplay
        {
            T standardDoorDisplayBehavior = AddDoorNoArray<T>(key + "_" + typeof(T).Name, mask, sideMaterials);
            LevelStudioPlugin.Instance.doorDisplays.Add(key, standardDoorDisplayBehavior);
            LevelStudioPlugin.Instance.doorIngameStatus.Add(key, ingameState);
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
        /// Creates the DoorDisplay of the specified type and adds it to the editor's keys.
        /// Uses the specified WindowObject for visuals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="mask"></param>
        /// <param name="sideMaterials"></param>
        /// <returns></returns>
        public static T AddWindow<T>(string key, WindowObject windObject) where T : DoorDisplay
        {
            return AddWindow<T>(key, windObject.mask, windObject.overlay);
        }

        /// <summary>
        /// Creates the DoorDisplay of the DoorDisplay type and adds it to the editor's keys.
        /// Uses the specified WindowObject for visuals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="mask"></param>
        /// <param name="sideMaterials"></param>
        /// <returns></returns>
        public static DoorDisplay AddWindow(string key, WindowObject windObject)
        {
            return AddWindow<DoorDisplay>(key, windObject.mask, windObject.overlay);
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
            AddExit(key, visual);
        }

        /// <summary>
        /// Adds the specified exit visual to the editors database.
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

        /// <summary>
        /// Automatically sets up the visual and adds it to the dictionary for the specified Activity. You may need to further configure the generated object.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject AddActivityVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_Visual");
            MovableObjectInteraction movableObjectInteract = clone.AddComponent<MovableObjectInteraction>();
            movableObjectInteract.allowedRotations = RotateAxis.Flat;
            movableObjectInteract.allowedAxis = MoveAxis.All;
            EditorRendererContainer container = clone.gameObject.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.AddComponent<EditorDeletableObject>().renderContainer = container;
            clone.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;

            LevelStudioPlugin.Instance.activityDisplays.Add(key, clone);
            return clone;
        }

        /// <summary>
        /// Automatically generates the visual and adds it to the dictionary for the specified NPC based off of the prefab.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="npc"></param>
        /// <returns></returns>
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
            collider.size = new Vector3(6f, 0.1f, 6f);
            collider.isTrigger = true;
            EditorRendererContainer container = clone.gameObject.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.gameObject.AddComponent<EditorDeletableObject>().renderContainer = container;
            clone.gameObject.AddComponent<SettingsComponent>();
            LevelStudioPlugin.Instance.npcDisplays.Add(key, clone);
            return clone;
        }

        public static void AddNPCExtraVisual(string extraKey, string originalKey)
        {
            LevelStudioPlugin.Instance.npcDisplays.Add(extraKey, LevelStudioPlugin.Instance.npcDisplays[originalKey]);
        }

        /// <summary>
        /// Generates a visual for the specified tile based object, and adds it to the tileBasedObjectDisplays dictionary.
        /// </summary>
        /// <param name="key">The key to use for the dictionary.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject AddTileBasedObjectVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_TileBasedObject");
            EditorRendererContainer container = clone.gameObject.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.gameObject.AddComponent<EditorDeletableObject>().renderContainer = container;
            clone.layer = LevelStudioPlugin.editorInteractableLayer;
            LevelStudioPlugin.Instance.tileBasedObjectDisplays.Add(key, clone);
            return clone;
        }

        /// <summary>
        /// Generates a visual for the specified structure, and adds it to the genericStructureDisplays dictionary.
        /// </summary>
        /// <param name="key">The key to use for the dictionary.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject AddStructureGenericVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_GenericStructureVisual");
            EditorRendererContainer container = clone.gameObject.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.gameObject.AddComponent<EditorDeletableObject>().renderContainer = container;
            clone.layer = LevelStudioPlugin.editorInteractableLayer;
            LevelStudioPlugin.Instance.genericStructureDisplays.Add(key, clone);
            return clone;
        }

        /// <summary>
        /// Generates a visual for the specified marker, and adds it to the genericMarkerDisplays dictionary.
        /// </summary>
        /// <param name="key">The key to use for the dictionary.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject AddMarkerGenericVisual(string key, GameObject obj)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj);
            clone.name = clone.name.Replace("_Stripped", "_GenericMarkerVisual");
            EditorRendererContainer container = clone.gameObject.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.gameObject.AddComponent<EditorDeletableObject>().renderContainer = container;
            clone.layer = LevelStudioPlugin.editorInteractableLayer;
            LevelStudioPlugin.Instance.genericMarkerDisplays.Add(key, clone);
            return clone;
        }
        /// <summary>
        /// Replaces all <see cref="AnimatedSpriteRotator"/> components in the hierarchy of the <paramref name="rootObject"/>
        /// with <see cref="SpriteRotator"/> components, while preserving the sprite mapping.
        /// <para> If the target sprite cannot be found in the sprite map, or if the sprite sheet is too short, a warning is logged and the conversion for that rotator is skipped. </para>
        /// </summary>
        /// <param name="rootObject">The root <see cref="GameObject"/> whose children will be searched for the <see cref="AnimatedSpriteRotator"/> components to replace.</param>
        public static void ReplaceAnimatedRotators(this GameObject rootObject)
        {
            foreach (var animatedRotator in rootObject.GetComponentsInChildren<AnimatedSpriteRotator>())
            {
                GameObject targetGameObject = animatedRotator.gameObject;
                var renderer = (SpriteRenderer)_AnimatedRotator_renderer.GetValue(animatedRotator);
                Sprite targetSprite = animatedRotator.targetSprite ?? renderer.sprite; // A failsafe for this case

                // Replicate what AnimatedSpriteRotator.LateUpdate does to find the correct animation frame and sprite map based on the targetSprite
                int foundMapId = -1;
                int foundSpriteId = -1;
                bool wasFound = false;
                var spriteMap = (SpriteRotationMap[])_AnimatedRotator_spriteMap.GetValue(animatedRotator);

                for (int i = 0; i < spriteMap.Length; i++)
                {
                    var map = spriteMap[i];
                    // The original logic iterates through the start of each animation sequence.
                    for (int j = 0; j < map.SpriteCount; j += map.angleCount)
                    {
                        // Check for normal sprite and override, bruh!
                        if (map.Sprite(j) == targetSprite || (map.HasOverride && map.OverriddenSprite(i) == targetSprite))
                        {
                            foundMapId = i;
                            foundSpriteId = j;
                            wasFound = true;
                            break;
                        }
                    }
                    if (wasFound)
                        break;
                }

                if (!wasFound)
                {
                    Debug.LogWarning($"Could not find targetSprite {targetSprite?.name} in the sprite maps for the rotator on {targetGameObject.name}.", targetGameObject);
                    return;
                }

                //New spriterotator data here
                SpriteRotationMap activeMap = spriteMap[foundMapId];
                int angleCount = activeMap.angleCount;

                // Create flat array of sprites for the new rotator
                Sprite[] newSprites = new Sprite[angleCount];
                Sprite[] sourceSheet = activeMap.HasOverride ?
                    (Sprite[])_SpriteRotationMap_overrideSpriteSheet.GetValue(activeMap) :
                    (Sprite[])_SpriteRotationMap_spriteSheet.GetValue(activeMap);

                if (sourceSheet.Length < foundSpriteId + angleCount) // Shouldn't happen much
                {
                    Debug.LogWarning($"Failed to convert the sprite array because sourceSheet length ({sourceSheet.Length}) is less than the angleCount from id ({foundSpriteId + angleCount})", targetGameObject);
                    continue;
                }

                int shift = Mathf.RoundToInt(angleCount * 0.25f); // quarter rotation step to shift -90°
                for (int i = 0; i < angleCount; i++)
                {
                    newSprites[(i + shift) % angleCount] = sourceSheet[foundSpriteId + i];
                }

                // Add the SpriteRotator at the end
                SpriteRotator newRotator = targetGameObject.AddComponent<SpriteRotator>();

                _SpriteRotator_spriteRenderer.SetValue(newRotator, renderer);
                _SpriteRotator_sprites.SetValue(newRotator, newSprites);

                // Destroy old comp
                UnityEngine.Object.DestroyImmediate(animatedRotator);
            }
        }

        /// <summary>
        /// Automatically generates a visual for the specified BasicObject and sets up its collision.
        /// If you aren't using useRegularColliderAsEditorHitbox, be sure to assign EditorBasicObject's .editorCollider variable.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj">The prefab to copy.</param>
        /// <param name="useRegularColliderAsEditorHitbox">If true, a copy of the regular collider for the object will be created based off the first found collider.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static EditorBasicObject AddObjectVisual(string key, GameObject obj, bool useRegularColliderAsEditorHitbox)
        {
            GameObject clone = CloneToPrefabStripMonoBehaviors(obj, new Type[] { typeof(BillboardUpdater), typeof(AnimatedSpriteRotator) });
            clone.name = clone.name.Replace("_Stripped", "_Visual");
            EditorBasicObject basic = clone.AddComponent<EditorBasicObject>();
            EditorRendererContainer container = clone.AddComponent<EditorRendererContainer>();
            container.AddRendererRange(clone.GetComponentsInChildren<Renderer>(), "none");
            clone.AddComponent<EditorDeletableObject>().renderContainer = container;
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

        /// <summary>
        /// Adds the specified object visual with a custom box collider for use in editor.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj">The prefab to copy.</param>
        /// <param name="size"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public static EditorBasicObject AddObjectVisualWithCustomBoxCollider(string key, GameObject obj, Vector3 size, Vector3 center)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            BoxCollider col = basic.gameObject.AddComponent<BoxCollider>();
            col.size = size;
            col.center = center;
            basic.editorCollider = col;
            return basic;
        }

        /// <summary>
        /// Adds the specified object visual with a custom sphere collider for use in editor.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj">The prefab to copy.</param>
        /// <param name="radius"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public static EditorBasicObject AddObjectVisualWithCustomSphereCollider(string key, GameObject obj, float radius, Vector3 center)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            SphereCollider col = basic.gameObject.AddComponent<SphereCollider>();
            col.radius = radius;
            col.center = center;
            basic.editorCollider = col;
            return basic;
        }

        /// <summary>
        /// Adds the specified object visual with a custom capsule collider for use in editor.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj">The prefab to copy.</param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="direction"></param>
        /// <param name="center"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the specified object visual with a custom capsule collider for use in editor.
        /// </summary>
        /// <param name="key">The key to use in the dictionary.</param>
        /// <param name="obj">The prefab to copy.</param>
        /// <param name="convex"></param>
        /// <returns></returns>
        public static EditorBasicObject AddObjectVisualWithMeshCollider(string key, GameObject obj, bool convex)
        {
            EditorBasicObject basic = AddObjectVisual(key, obj, false);
            MeshCollider col = basic.gameObject.AddComponent<MeshCollider>();
            col.convex = convex;
            basic.editorCollider = col;
            return basic;
        }

        /// <summary>
        /// Creates a RoomVisualManager of the specified class type for the specified room type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
