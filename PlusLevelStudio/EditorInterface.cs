using MTM101BaldAPI;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorInterface
    {
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
            GameObject standardDoorDisplayObject = new GameObject("StandardDoorVisual");
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
            LevelStudioPlugin.Instance.doorDisplays.Add(key, standardDoorDisplayBehavior);
            LevelStudioPlugin.Instance.doorIsTileBased.Add(key, isTileBasedObject);
            return standardDoorDisplayBehavior;
        }
    }
}
