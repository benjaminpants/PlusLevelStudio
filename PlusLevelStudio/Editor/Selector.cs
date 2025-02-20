using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{

    public enum SelectorState
    {
        None,
        Tile,
        Area,
        Object
    }

    public class SelectorArrow : MonoBehaviour, IEditorInteractable
    {
        public Selector selector;
        public Direction direction;

        public bool OnClicked()
        {
            return selector.TileArrowClicked(direction);
        }

        public bool OnHeld()
        {
            return selector.TileArrowHeld();
        }

        public void OnReleased()
        {
            selector.TileArrowReleased();
        }
    }

    public class Selector : MonoBehaviour
    {
        public GameObject tileSelector;
        public GameObject[] tileArrows = new GameObject[4];

        public IntVector2 selectedTile { get; private set; } = new IntVector2(0, 0);

        public RectInt selectedArea { get; private set; } = new RectInt(new Vector2Int(0,0), new Vector2Int(0, 0));

        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();
        protected SelectorState state;

        protected Action<IntVector2, IntVector2> resizeAction;

        /// <summary>
        /// The current state of the selector.
        /// </summary>
        public SelectorState currentState => state;

        /// <summary>
        /// Removes the active selection
        /// </summary>
        public void DisableSelection()
        {
            state = SelectorState.None;
            resizeAction = null;
            UpdateSelectionObjects();
        }

        public void SelectTile(IntVector2 tile)
        {
            selectedTile = tile;
            state = SelectorState.Tile;
            UpdateSelectionObjects();
        }

        public void SelectArea(RectInt rect, Action<IntVector2, IntVector2> resizeAction)
        {
            selectedArea = rect;
            state = SelectorState.Area;
            this.resizeAction = resizeAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }


        public bool TileArrowClicked(Direction d)
        {
            currentArrow = d;
            currentStartPosition = Singleton<EditorController>.Instance.mouseGridPosition;
            return true;
        }


        public bool TileArrowHeld()
        {
            switch (state)
            {
                default:
                    break;
                case SelectorState.Area:
                    PositionArrow(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow) * 10f);
                    break;
            }
            return true;
        }

        void PositionArrow(Direction d, float additionalDistanceFromEdge)
        {
            Vector3 movement = d.ToVector3();
            Vector3 center = (new Vector3(selectedArea.center.x, 0f, selectedArea.center.y) * 10f);

            tileArrows[(int)d].transform.position = center + (movement * 5f * (selectedArea.size.ToMystVector().GetValueForDirection(d) + 1)) + (movement * additionalDistanceFromEdge) + (Vector3.up * 0.01f);
        }

        public void TileArrowReleased()
        {
            EditorExtensions.CalculateDifferencesForHandleDrag(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow), out IntVector2 sizeDif, out IntVector2 posDif);
            PositionArrow(currentArrow, 0f);
            resizeAction.Invoke(sizeDif, posDif);
            currentArrow = Direction.Null;
        }

        void Update()
        {
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Tile:
                    transform.position = selectedTile.ToWorld() + (Vector3.up * 0.01f);
                    break;
            }
        }

        protected void UpdateSelectionObjects()
        {
            tileSelector.SetActive(false);
            for (int i = 0; i < tileArrows.Length; i++)
            {
                tileArrows[i].SetActive(false);
            }
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Tile:
                    tileSelector.SetActive(true);
                    break;
                case SelectorState.Area:
                    for (int i = 0; i < tileArrows.Length; i++)
                    {
                        tileArrows[i].SetActive(true);
                    }
                    break;
            }
        }

    }
}
