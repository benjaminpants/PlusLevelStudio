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


        public GameObject[] arrowObjects = new GameObject[4];

        public Vector3 center => transform.position + new Vector3(editor.levelData.mapSize.x * 5f, 0f, editor.levelData.mapSize.z * 5f);


        public bool TileArrowClicked(Direction d)
        {
            currentArrow = d;
            return true;
        }

        public bool TileArrowHeld()
        {
            return true;
        }

        public void TileArrowReleased()
        {
            currentArrow = Direction.Null;
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
                Vector3 movement = directions[i].ToVector3();
                arrowObjects[i].transform.position = center + (movement * 5f * (((directions[i] == Direction.North || directions[i] == Direction.South) ? editor.levelData.mapSize.z : editor.levelData.mapSize.x) + 2f));
            }
        }
    }
}
