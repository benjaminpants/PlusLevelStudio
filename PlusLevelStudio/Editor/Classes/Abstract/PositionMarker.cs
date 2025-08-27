using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class PositionMarker : MarkerLocation, IEditorMovable
    {
        public Vector3 position;
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        public Transform GetTransform()
        {
            return EditorController.Instance.GetVisual(this).transform;
        }

        public override GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericMarkerDisplays[type];
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            visualObject.GetComponent<MovableObjectInteraction>().target = this;
            UpdateVisual(visualObject);
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                this.position = position.Value;
                EditorController.Instance.UpdateVisual(this);
            }
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            position = reader.ReadUnityVector3().ToUnity();
        }

        public void Selected()
        {
            EditorController.Instance.GetVisual(this).GetComponentInChildren<EditorRendererContainer>().Highlight("yellow");
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            position -= worldOffset;
        }

        public void Unselected()
        {
            EditorController.Instance.GetVisual(this).GetComponentInChildren<EditorRendererContainer>().Highlight("none");
            if (!ValidatePosition(EditorController.Instance.levelData))
            {
                OnDelete(EditorController.Instance.levelData);
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position;
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(position.ToData());
        }
    }
}
