using MTM101BaldAPI;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PosterPlacement : IEditorVisualizable, IEditorDeletable
    {
        public string type;
        public IntVector2 position;
        public Direction direction;
        public PosterObject myPoster => LevelLoaderPlugin.Instance.posterAliases[type];
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.posterVisual;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponentInChildren<MeshRenderer>().material.SetMainTexture(EditorController.Instance.GetOrGeneratePoster(myPoster));
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            return data.WallFree(position,direction, true);
        }

        // TODO: account for multi-posters
        public bool OccupiesWall(IntVector2 pos, Direction dir)
        {
            return (pos == position) && (direction == dir);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.posters.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }
    }
}
