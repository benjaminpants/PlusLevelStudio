using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlusLevelStudio.Editor
{

    public enum SelectorState
    {
        /// <summary>
        /// Nothing is selected or displayed.
        /// </summary>
        None,
        /// <summary>
        /// A tile is selected, show the tile selection box.
        /// </summary>
        Tile,
        /// <summary>
        /// An area is selected. Show the resize handles.
        /// </summary>
        Area,
        /// <summary>
        /// A direction is waiting to be selected. Show the resize handles in a 1x1 box.
        /// </summary>
        Direction,
        /// <summary>
        /// An object is selected. Show the 3D rotation handles and the settings.
        /// </summary>
        Object,
        /// <summary>
        /// An object that only can have its settings changed. Show the settings. Legacy.
        /// </summary>
        Settings
    }

    public class SelectorArrow : MonoBehaviour, IEditorInteractable
    {
        public Selector selector;
        public Direction direction;

        public bool InteractableByTool(EditorTool tool)
        {
            return true;
        }

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
        public SettingsWorldButton gearButton;
        public MoveHandles moveHandles;
        const float baseUpwardsOffset = 0.02f;
        float upwardsOffset => baseUpwardsOffset + EditorController.Instance.gridManager.Height;

        public IntVector2 selectedTile { get; private set; } = new IntVector2(0, 0);

        protected bool showSettings = false;
        public RectInt selectedArea { get; private set; } = new RectInt(new Vector2Int(0,0), new Vector2Int(0, 0));

        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();
        protected SelectorState state;

        protected Action<IntVector2, IntVector2> resizeAction;
        protected Action<Direction> directionAction;
        public IEditorMovable selectedMovable { get; private set; }


        void Awake()
        {
            UpdateSelectionObjects();
        }

        private void NullActions()
        {
            resizeAction = null;
            directionAction = null;
            if (selectedMovable != null)
            {
                selectedMovable.Unselected();
            }
            selectedMovable = null;
        }

        /// <summary>
        /// The current state of the selector.
        /// </summary>
        public SelectorState currentState => state;

        /// <summary>
        /// Removes the active selection
        /// </summary>
        public void DisableSelection()
        {
            HideSettings();
            state = SelectorState.None;
            NullActions();
            UpdateSelectionObjects();
        }

        /// <summary>
        /// Selects the specified tile.
        /// </summary>
        /// <param name="tile"></param>
        public void SelectTile(IntVector2 tile)
        {
            HideSettings();
            selectedTile = tile;
            state = SelectorState.Tile;
            NullActions();
            UpdateSelectionObjects();
        }

        public void SelectObject(IEditorMovable movable, MoveAxis enabledAxis, RotateAxis enabledRotations)
        {
            HideSettings();
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            NullActions();
            selectedMovable = movable;
            state = SelectorState.Object;
            moveHandles.SetArrows(enabledAxis);
            moveHandles.SetRings(enabledRotations);
            UpdateSelectionObjects();
            selectedMovable.Selected();
        }

        /// <summary>
        /// Places the Selector at the specified tile and shows the arrows.
        /// directionSelectAction is called when one of the arrows is clicked.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="directionSelectAction"></param>
        public void SelectRotation(IntVector2 tile, Action<Direction> directionSelectAction)
        {
            HideSettings();
            selectedTile = tile;
            selectedArea = new RectInt(new Vector2Int(tile.x,tile.z), new Vector2Int(1,1));
            state = SelectorState.Direction;
            NullActions();
            directionAction = directionSelectAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }

        /// <summary>
        /// Select the area specified by Rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="resizeAction">First parameter is the change in size, the second parameter is the change in position.</param>
        public void SelectArea(RectInt rect, Action<IntVector2, IntVector2> resizeAction)
        {
            HideSettings();
            NullActions();
            selectedArea = rect;
            state = SelectorState.Area;
            this.resizeAction = resizeAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }



        public void ShowSettings(Vector3 position, Action onClicked)
        {
            showSettings = true;
            gearButton.clickedAction = onClicked;
            gearButton.transform.position = position;
            gearButton.gameObject.SetActive(true);
        }
        public void HideSettings()
        {
            showSettings = false;
            gearButton.gameObject.SetActive(false);
        }

        public void ShowSettingsSelect(Vector3 position, Action onClicked)
        {
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            state = SelectorState.Settings;
            ShowSettings(position, onClicked);
            UpdateSelectionObjects();
        }


        public bool TileArrowClicked(Direction d)
        {
            if (state == SelectorState.Direction)
            {
                directionAction.Invoke(d);
                return false;
            }
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

        public void UpdateObjectPosition(Vector3 offset)
        {
            selectedMovable.MoveUpdate(offset, null);
        }

        public void UpdateObjectRotation(Quaternion rotation)
        {
            selectedMovable.MoveUpdate(null, rotation);
        }

        void PositionArrow(Direction d, float additionalDistanceFromEdge)
        {
            Vector3 movement = d.ToVector3();
            Vector3 center = (new Vector3(selectedArea.center.x, 0f, selectedArea.center.y) * 10f);

            tileArrows[(int)d].transform.position = center + (movement * 5f * (selectedArea.size.ToMystVector().GetValueForDirection(d) + 1)) + (movement * additionalDistanceFromEdge) + (Vector3.up * upwardsOffset);
        }

        public void TileArrowReleased()
        {
            EditorExtensions.CalculateDifferencesForHandleDrag(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow), out IntVector2 sizeDif, out IntVector2 posDif);
            PositionArrow(currentArrow, 0f);
            if (resizeAction != null)
            {
                resizeAction.Invoke(sizeDif, posDif);
            }
            currentArrow = Direction.Null;
        }

        void Update()
        {
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Direction:
                    transform.position = selectedTile.ToWorld() + (Vector3.up * upwardsOffset); // annoying hack seriously what the fuck is going on
                    for (int i = 0; i < tileArrows.Length; i++)
                    {
                        PositionArrow((Direction)i, 0f);
                    }
                    break;
                case SelectorState.Tile:
                    transform.position = selectedTile.ToWorld() + (Vector3.up * upwardsOffset);
                    break;
                case SelectorState.Object:
                    Transform tf = selectedMovable.GetTransform();
                    moveHandles.GoToTarget(tf);
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
            moveHandles.gameObject.SetActive(false);
            gearButton.gameObject.SetActive(showSettings);
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
                case SelectorState.Direction:
                    tileSelector.SetActive(true);
                    for (int i = 0; i < tileArrows.Length; i++)
                    {
                        tileArrows[i].SetActive(true);
                    }
                    break;
                case SelectorState.Settings:
                    gearButton.gameObject.SetActive(true);
                    break;
                case SelectorState.Object:
                    moveHandles.gameObject.SetActive(true);
                    break;
            }
        }

    }
}
