using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class GridArrow : MonoBehaviour, IEditorInteractable
    {
        public GridManager grid;
        public Direction direction;

        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            return grid.TileArrowClicked(direction);
        }

        public bool OnHeld()
        {
            return grid.TileArrowHeld();
        }

        public void OnReleased()
        {
            grid.TileArrowReleased();
        }
    }


    public class GridManager : MonoBehaviour
    {

        public EditorController editor;
        private GameObject[,] gridObjects = new GameObject[0,0];
        private GameObject[,] gridOverlayObjects = new GameObject[0,0];
        public GameObject gridCellTemplate;
        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();
        protected float offset = -0.01f;
        protected float _height = 0f;
        public float Height
        { 
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                editor.UpdateGridHeight();
                RepositionGrid();
            }
        }

        public GameObject[] arrowObjects = new GameObject[4];

        public Vector3 center => transform.position + new Vector3(editor.levelData.mapSize.x * 5f, _height, editor.levelData.mapSize.z * 5f);


        public bool TileArrowClicked(Direction d)
        {
            currentArrow = d;
            currentStartPosition = Singleton<EditorController>.Instance.mouseGridPosition;
            Singleton<EditorController>.Instance.selector.DisableSelection(); // deselect whatever we had before
            return true;
        }

        public bool TileArrowHeld()
        {
            PositionArrow(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow) * 10f);
            return true;
        }

        public void TileArrowReleased()
        {
            //Debug.Log("Got distance of: " + (currentStartPosition - Singleton<EditorController>.Instance.mouseGridPosition).GetValueForDirection(currentArrow));
            EditorExtensions.CalculateDifferencesForHandleDrag(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow), out IntVector2 sizeDif, out IntVector2 posDif);
            Singleton<EditorController>.Instance.ResizeGrid(posDif, sizeDif);
            PositionArrow(currentArrow);
            currentArrow = Direction.Null;
        }


        void PositionArrow(Direction d, float additionalDistanceFromEdge = 0f)
        {
            Vector3 movement = d.ToVector3();
            arrowObjects[(int)d].transform.position = center + (movement * 5f * (editor.levelData.mapSize.GetValueForDirection(d) + 2f)) + (movement * additionalDistanceFromEdge) + (Vector3.up * 0.01f);
        }

        private bool overlaysUsed = false;

        public void DisableOverlays()
        {
            if (!overlaysUsed) return;
            overlaysUsed = false;
            foreach (var item in gridOverlayObjects)
            {
                item.SetActive(false);
            }
        }

        public void EnableOverlaysWithMask(bool[,] mask, bool invert)
        {
            overlaysUsed = true;
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (EditorController.Instance.levelData.cells[x,y].type == 16)
                    {
                        gridOverlayObjects[x, y].SetActive(false);
                        continue;
                    }
                    gridOverlayObjects[x, y].SetActive(mask[x, y] != invert);
                }
            }
        }

        protected void RepositionGrid()
        {
            for (int x = 0; x < editor.levelData.mapSize.x; x++)
            {
                for (int y = 0; y < editor.levelData.mapSize.z; y++)
                {
                    gridObjects[x,y].transform.position = new IntVector2(x, y).ToWorld() + (Vector3.up * (offset + _height));
                }
            }
        }

        public void RegenerateGrid()
        {
            // todo: make it so it doesn't delete the entire grid everytime it needs to be regenerated
            foreach (var item in gridObjects)
            {
                GameObject.Destroy(item);
            }
            gridObjects = new GameObject[editor.levelData.mapSize.x, editor.levelData.mapSize.z];
            gridOverlayObjects = new GameObject[editor.levelData.mapSize.x, editor.levelData.mapSize.z];
            for (int x = 0; x < editor.levelData.mapSize.x; x++)
            {
                for (int y = 0; y < editor.levelData.mapSize.z; y++)
                {
                    gridObjects[x,y] = GameObject.Instantiate(gridCellTemplate);
                    gridObjects[x, y].transform.position = new IntVector2(x, y).ToWorld() + (Vector3.up * (offset + _height));
                    gridObjects[x, y].transform.SetParent(transform, true);
                    gridOverlayObjects[x, y] = gridObjects[x, y].transform.Find("GridOverlay").gameObject;
                }
            }
            List<Direction> directions = Directions.All(); 
            for (int i = 0; i < directions.Count; i++)
            {
                PositionArrow((Direction)i);
            }
        }
    }
}
