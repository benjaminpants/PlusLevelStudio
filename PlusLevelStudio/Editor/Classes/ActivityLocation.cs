using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class ActivityLocation : IEditorDeletable, IEditorMovable, IEditorVisualizable
    {
        public string type;
        public Vector3 position;
        public Direction direction;
        public EditorRoom myRoom;


        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            EditorRoom room = data.RoomFromPos(position.ToCellVector(), true);
            if (room == null) return false;
            if (room.activity != null) return false;
            return true;
        }

        public void Setup(EditorLevelData data)
        {
            if (myRoom != null)
            {
                myRoom.activity = null;
                myRoom = null;
            }
            EditorRoom room = data.RoomFromPos(position.ToCellVector(), true);
            if (room == null) return;
            Setup(room);
            return;
        }

        public void Setup(EditorRoom room)
        {
            room.activity = this;
            myRoom = room;
        }

        public Transform GetTransform()
        {
            return EditorController.Instance.GetVisual(this).transform;
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.activityDisplays[type];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponentInChildren<MovableObjectInteraction>().target = this;
            visualObject.GetComponentInChildren<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                this.position = position.Value;
            }
            if (rotation.HasValue)
            {
                direction = Directions.DirFromVector3(rotation.Value * Vector3.forward, 45f);
            }
            EditorController.Instance.UpdateVisual(this);
        }

        public bool OnDelete(EditorLevelData data)
        {
            if (myRoom != null)
            {
                myRoom.activity = null;
            }
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void Selected()
        {
            EditorController.Instance.GetVisual(this).GetComponentInChildren<EditorRendererContainer>().Highlight("yellow");
        }

        public void Unselected()
        {
            EditorController.Instance.GetVisual(this).GetComponentInChildren<EditorRendererContainer>().Highlight("none");
            // re-attempt setup so we can see if our room is null now
            SetupDeleteIfInvalid();
        }

        public void SetupDeleteIfInvalid()
        {
            Setup(EditorController.Instance.levelData);
            if (myRoom == null)
            {
                OnDelete(EditorController.Instance.levelData);
            }
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position;
            visualObject.transform.rotation = direction.ToRotation();
        }
    }
}
