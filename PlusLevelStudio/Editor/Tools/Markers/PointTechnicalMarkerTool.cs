using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.Tools
{
    /// <summary>
    /// Same as CellMarkerTool, but with the "technical_" id.
    /// </summary>
    public class PointTechnicalMarkerTool : CellMarkerTool
    {
        public override string id => "technical_" + type;

        internal PointTechnicalMarkerTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/technical_" + type))
        {
        }

        public PointTechnicalMarkerTool(string type, Sprite sprite) : base(type, sprite)
        {

        }
    }
}
