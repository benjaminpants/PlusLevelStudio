using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MTM101BaldAPI;
using HarmonyLib;
using System.Collections;
using MTM101BaldAPI.Registers;
using System.Linq;

namespace PlusStudioLevelLoader
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudioloader", "Plus Level Loader", "0.0.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class LevelLoaderPlugin : BaseUnityPlugin
    {
        public static LevelLoaderPlugin Instance;

        public Dictionary<string, Texture2D> roomTextureAliases = new Dictionary<string, Texture2D>();

        public static Texture2D RoomTextureFromAlias(string alias)
        {
            if (!Instance.roomTextureAliases.ContainsKey(alias))
            {
                return Instance.roomTextureAliases["PlaceholderWall"];
            }
            return Instance.roomTextureAliases[alias];
        }

        void Awake()
        {
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.levelstudioloader");
            harmony.PatchAllConditionals();
            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadEnumerator(), false);
            Instance = this;
        }

        IEnumerator LoadEnumerator()
        {
            yield return 1;
            yield return "Fetching textures...";
            Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.GetInstanceID() >= 0 && x.isReadable).ToArray();
            roomTextureAliases.Add("HallFloor", textures.First(x => x.name == "TileFloor"));
            roomTextureAliases.Add("Wall", textures.First(x => x.name == "Wall"));
            roomTextureAliases.Add("Ceiling", textures.First(x => x.name == "CeilingNoLight"));
            roomTextureAliases.Add("BlueCarpet", textures.First(x => x.name == "Carpet"));
            roomTextureAliases.Add("FacultyWall", textures.First(x => x.name == "WallWithMolding"));
            roomTextureAliases.Add("TileFloor", textures.First(x => x.name == "ActualTileFloor"));
            roomTextureAliases.Add("ElevatorCeiling", textures.First(x => x.name == "ElCeiling"));
            roomTextureAliases.Add("Grass", textures.First(x => x.name == "Grass"));
            roomTextureAliases.Add("Fence", textures.First(x => x.name == "fence"));
            roomTextureAliases.Add("JohnnyWall", textures.First(x => x.name == "JohnnyWall"));
            roomTextureAliases.Add("None", textures.First(x => x.name == "Transparent"));
            roomTextureAliases.Add("PlaceholderWall", textures.First(x => x.name == "Placeholder_Wall"));
        }
    }
}
