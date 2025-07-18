using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorBasicObject : MonoBehaviour, IEditorInteractable
    {
        public BasicObjectLocation myLocation;
        public List<Collider> ingameColliders = new List<Collider>();
        public List<Renderer> renderers = new List<Renderer>();
        public int ingameLayer;
        public Collider editorCollider;

        public void AssignLocation(BasicObjectLocation local)
        {
            myLocation = local;
            GetComponent<EditorDeletableObject>().toDelete = local;
        }

        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            EditorController.Instance.selector.SelectObject(myLocation, MoveAxis.All, RotateAxis.Full);
            return false;
        }

        public bool OnHeld()
        {
            throw new NotImplementedException();
        }

        public void OnReleased()
        {
            throw new NotImplementedException();
        }

        public void SetMode(bool editorMode)
        {
            if (editorMode)
            {
                ingameColliders.ForEach(x => x.enabled = false);
                editorCollider.enabled = true;
                gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
                return;
            }
            ingameColliders.ForEach(x => x.enabled = true);
            editorCollider.enabled = false;
            gameObject.layer = ingameLayer;
        }
    }
}
