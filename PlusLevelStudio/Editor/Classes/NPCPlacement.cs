﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class NPCPlacement : IEditorVisualizable, IEditorDeletable, IEditorPositionVerifyable
    {
        public string npc;
        public IntVector2 position;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.npcDisplays[npc];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.npcs.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld() + (Vector3.up * 5f);
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }
    }
}
