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

    public class Selector : MonoBehaviour
    {
        public GameObject tileSelector;


        public IntVector2 selectedTile { get; private set; } = new IntVector2(0, 0);

        public bool dragging = false;

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


        void Update()
        {
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Tile:
                    tileSelector.transform.position = selectedTile.ToWorld() + (Vector3.up * 0.01f);
                    break;
            }
        }

        protected void UpdateSelectionObjects()
        {
            tileSelector.SetActive(false);
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
