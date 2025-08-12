using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;
using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.UI;
using System.IO;
using PlusLevelStudio.Editor.SettingsUI;

namespace PlusLevelStudio.Editor
{
    public class LightPlacement : IEditorVisualizable, IEditorDeletable, IEditorSettingsable
    {
        public IntVector2 position;
        public ushort lightGroup;
        public string type;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public void SettingsClicked()
        {
            EditorController.Instance.HoldUndo();
            LightSettingsExchangeHandler settings = EditorController.Instance.CreateUI<LightSettingsExchangeHandler>("LightConfig");
            settings.myPlacement = this;
            settings.Refresh(); // make it refresh
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.assetMan.Get<GameObject>("LightDisplay");
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            if (LevelLoaderPlugin.Instance.lightTransforms[type] != null)
            {
                Transform lightTransform = GameObject.Instantiate<Transform>(LevelLoaderPlugin.Instance.lightTransforms[type], visualObject.transform);
                lightTransform.localPosition = Vector3.down * 12f; // TODO: change?
                visualObject.GetComponent<EditorRendererContainer>().AddRendererRange(lightTransform.GetComponentsInChildren<Renderer>(), "none");
            }
            visualObject.GetComponent<SettingsComponent>().activateSettingsOn = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.lights.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            EditorController.Instance.RefreshLights();
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = EditorController.Instance.workerEc.CellFromPosition(position).CenterWorldPosition + (Vector3.up * 7f);
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            // dont allow light stacking
            for (int i = 0; i < data.lights.Count; i++)
            {
                if (data.lights[i] == this) continue;
                if (data.lights[i].position == position) return false;
            }
            return data.RoomIdFromPos(position, true) != 0;
        }
    }

    public class LightGroup
    {
        public Color color = new Color(1f, 1f, 1f);
        public byte strength = 10;

        public LightGroup()
        {

        }

        public LightGroup(LightGroup toCopy)
        {
            color = toCopy.color;
            strength = toCopy.strength;
        }
    }
}
