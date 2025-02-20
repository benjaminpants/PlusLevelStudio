using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class GridArrow : MonoBehaviour, IEditorInteractable
    {
        public GridManager grid;
        public Direction direction;

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
        private GameObject[] gridObjects = new GameObject[0];
        public GameObject gridCellTemplate;
        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();


        public GameObject[] arrowObjects = new GameObject[4];

        public Vector3 center => transform.position + new Vector3(editor.levelData.mapSize.x * 5f, 0f, editor.levelData.mapSize.z * 5f);


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
            arrowObjects[(int)d].transform.position = center + (movement * 5f * (editor.levelData.mapSize.GetValueForDirection(d) + 2f)) + (movement * additionalDistanceFromEdge);
        }

        public void RegenerateGrid()
        {
            // todo: make it so it doesn't delete the entire grid everytime it needs to be regenerated
            for (int i = 0; i < gridObjects.Length; i++)
            {
                GameObject.Destroy(gridObjects[i]);
            }
            gridObjects = new GameObject[editor.levelData.mapSize.x * editor.levelData.mapSize.z];
            int count = 0;
            for (int x = 0; x < editor.levelData.mapSize.x; x++)
            {
                for (int y = 0; y < editor.levelData.mapSize.z; y++)
                {
                    gridObjects[count] = GameObject.Instantiate(gridCellTemplate);
                    gridObjects[count].transform.position = new IntVector2(x, y).ToWorld();
                    gridObjects[count].transform.SetParent(transform, true);
                    count++;
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
