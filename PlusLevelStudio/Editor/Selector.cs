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

        public bool dragging = false;

        protected Direction currentArrow = Direction.Null;
        protected SelectorState state;

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
            UpdateSelectionObjects();
        }

        public void SelectTile(IntVector2 tile)
        {
            selectedTile = tile;
            state = SelectorState.Tile;
            UpdateSelectionObjects();
        }


        public bool TileArrowClicked(Direction d)
        {
            currentArrow = d;
            return true;
        }

        public bool TileArrowHeld()
        {
            // todo: implement logic SelectorState.Area
            return true;
        }

        public void TileArrowReleased()
        {
            currentArrow = Direction.Null;
            // todo: implement logic for SelectorState.Area
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
            }
        }

    }
}
