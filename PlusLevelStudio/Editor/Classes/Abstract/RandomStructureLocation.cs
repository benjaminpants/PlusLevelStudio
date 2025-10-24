using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public abstract class RandomStructureLocation : StructureLocation
    {
        /// <summary>
        /// This throws an exception, do not use.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            throw new NotImplementedException("Compile is not valid for structure of class: " + this.GetType().FullName);
        }

        /// <summary>
        /// Compile this structure into the RandomStructureInfo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public abstract RandomStructureInfo CompileIntoRandom(EditorLevelData data, BaldiLevel level);
    }
}
