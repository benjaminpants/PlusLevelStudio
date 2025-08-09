﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PowerLeverBreakerTool : PlaceAndRotateTool
    {
        public override string id => "structure_powerlever_breaker";

        public PowerLeverBreakerTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            PowerLeverStructureLocation structure = (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
            BreakerLocation breaker = structure.CreateBreaker();
            breaker.position = position;
            breaker.direction = dir;
            structure.breakers.Add(breaker);
            EditorController.Instance.AddVisual(breaker);
            EditorController.Instance.UpdateVisual(structure);
            EditorController.Instance.SwitchToTool(null);
            return true;
        }
    }
}
