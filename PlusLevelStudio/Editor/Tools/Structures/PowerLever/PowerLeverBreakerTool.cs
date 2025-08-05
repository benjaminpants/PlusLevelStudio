using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PowerLeverBreakerTool : EditorTool
    {
        IntVector2? pos;

        public override string id => "structure_powerlever_breaker";

        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            pos = null;
        }

        public virtual void OnPlaced(Direction dir)
        {
            PowerLeverStructureLocation structure = (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
            BreakerLocation breaker = structure.CreateBreaker();
            breaker.position = pos.Value;
            breaker.direction = dir;
            structure.breakers.Add(breaker);
            EditorController.Instance.AddVisual(breaker);
            EditorController.Instance.UpdateVisual(structure);
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (pos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                pos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(pos.Value, OnPlaced);
                return false;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if (pos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
