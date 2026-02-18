using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class NPCProperties
    {

        public NPCProperties()
        {

        }

        /// <summary>
        /// Generates the appropiate prefabs for the NPC, the first GameObject in the array must always be the NPC itself.
        /// </summary>
        /// <param name="baseNpc"></param>
        /// <returns></returns>
        public abstract GameObject[] GeneratePrefabs(NPC baseNpc);

        /// <summary>
        /// Returns if this NPCProperties is at the default settings.
        /// This is done to avoid saving extra data and more importantly, avoid unnecessarily generating "customized" prefabs.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAtDefaultSettings();

        public abstract void ReadInto(BinaryReader reader);

        public abstract void Write(BinaryWriter writer);
    }
}
