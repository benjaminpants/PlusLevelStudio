using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorFileMeta
    {
        public const byte version = 2; // so we can read files from older versions before EditorFileMeta was introduced. 0 is pre-walls with 1 being post-walls.
        public string editorMode;
        public string[] toolbarTools = new string[9];
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(editorMode);
            writer.Write((byte)toolbarTools.Length);
            for (int i = 0; i < toolbarTools.Length; i++)
            {
                writer.Write(toolbarTools[i]);
            }
            writer.Write(cameraPosition.ToData());
            writer.Write(cameraRotation.ToData());
        }

        public static EditorFileMeta Read(BinaryReader reader, byte? passedVersion = null)
        {
            EditorFileMeta meta = new EditorFileMeta();
            byte version;
            if (passedVersion.HasValue)
            {
                version = passedVersion.Value;
            }
            else
            {
                version = reader.ReadByte();
            }
            meta.editorMode = reader.ReadString();
            int toolBoxCount = reader.ReadByte();
            for (int i = 0; i < toolBoxCount; i++)
            {
                meta.toolbarTools[i] = reader.ReadString();
            }
            meta.cameraPosition = reader.ReadUnityVector3().ToUnity();
            meta.cameraRotation = reader.ReadUnityQuaternion().ToUnity();
            return meta;
        }
    }

    public class EditorFileContainer
    {
        public EditorFileMeta meta;
        public EditorLevelData data;

        public void Write(BinaryWriter writer)
        {
            meta.Write(writer);
            data.Write(writer);
        }

        /// <summary>
        /// Read from the specified BinaryReader, without accounting for versions less than 2
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static EditorFileContainer Read(BinaryReader reader)
        {
            EditorFileContainer container = new EditorFileContainer();
            container.meta = EditorFileMeta.Read(reader);
            container.data = EditorLevelData.ReadFrom(reader);
            return container;
        }

        /// <summary>
        /// Read from the specified BinaryReader. The returned EditorFileContainer may have null EditorFileMeta if the file was made before format version 2.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static EditorFileContainer ReadMindful(BinaryReader reader)
        {
            EditorFileContainer container = new EditorFileContainer();
            long start = reader.BaseStream.Position;
            byte formatVersion = reader.ReadByte(); // we gotta read this first to know what to do
            if (formatVersion < 2) // this was pre meta
            {
                reader.BaseStream.Position = start; // go back to the start and read from there
                container.data = EditorLevelData.ReadFrom(reader);
                return container;
            }
            else
            {
                reader.BaseStream.Position = start; // go back to the start and read from there
                container.meta = EditorFileMeta.Read(reader);
            }
            container.data = EditorLevelData.ReadFrom(reader);
            return container;
        }
    }
}
