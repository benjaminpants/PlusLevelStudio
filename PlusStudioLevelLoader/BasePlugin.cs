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
using MTM101BaldAPI.AssetTools;

namespace PlusStudioLevelLoader
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudioloader", "Plus Level Loader", "0.0.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class LevelLoaderPlugin : BaseUnityPlugin
    {
        public static LevelLoaderPlugin Instance;

        public AssetManager assetMan = new AssetManager();

        public Dictionary<string, RoomSettings> roomSettings = new Dictionary<string, RoomSettings>();
        public Dictionary<string, Texture2D> roomTextureAliases = new Dictionary<string, Texture2D>();
        public Dictionary<string, Transform> lightTransforms = new Dictionary<string, Transform>();

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
            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadEnumerator(), LoadingEventOrder.Pre);
            Instance = this;
        }

        IEnumerator LoadEnumerator()
        {
            yield return 4;
            yield return "Fetching textures...";
            Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.GetInstanceID() >= 0 && x.isReadable).ToArray();
            roomTextureAliases.Add("HallFloor", textures.First(x => x.name == "TileFloor"));
            roomTextureAliases.Add("Wall", textures.First(x => x.name == "Wall"));
            roomTextureAliases.Add("Ceiling", textures.First(x => x.name == "CeilingNoLight"));
            roomTextureAliases.Add("BlueCarpet", textures.First(x => x.name == "Carpet"));
            roomTextureAliases.Add("WallWithMolding", textures.First(x => x.name == "WallWithMolding"));
            roomTextureAliases.Add("TileFloor", textures.First(x => x.name == "ActualTileFloor"));
            roomTextureAliases.Add("ElevatorCeiling", textures.First(x => x.name == "ElCeiling"));
            roomTextureAliases.Add("Grass", textures.First(x => x.name == "Grass"));
            roomTextureAliases.Add("Fence", textures.First(x => x.name == "fence"));
            roomTextureAliases.Add("JohnnyWall", textures.First(x => x.name == "JohnnyWall"));
            roomTextureAliases.Add("None", textures.First(x => x.name == "Transparent"));
            roomTextureAliases.Add("PlaceholderWall", textures.First(x => x.name == "Placeholder_Wall"));
            roomTextureAliases.Add("SaloonWall", textures.First(x => x.name == "SaloonWall"));
            yield return "Fetching materials...";
            // TODO: this is placeholder
            assetMan.AddFromResourcesNoClones<Material>();
            assetMan.AddFromResourcesNoClones<Cubemap>();
            assetMan.AddFromResourcesNoClones<StandardDoorMats>();
            yield return "Defining room categories...";
            List<RoomFunctionContainer> roomFunctions = Resources.FindObjectsOfTypeAll<RoomFunctionContainer>().ToList();
            roomSettings.Add("hall", new RoomSettings(RoomCategory.Hall, RoomType.Hall, Color.white, assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings.Add("class", new RoomSettings(RoomCategory.Class, RoomType.Room, Color.green, assetMan.Get<StandardDoorMats>("ClassDoorSet"), assetMan.Get<Material>("MapTile_Classroom")));
            roomSettings.Add("faculty", new RoomSettings(RoomCategory.Faculty, RoomType.Room, Color.red, assetMan.Get<StandardDoorMats>("FacultyDoorSet"), assetMan.Get<Material>("MapTile_Faculty")));
            roomSettings.Add("office", new RoomSettings(RoomCategory.Office, RoomType.Room, new Color(1f, 1f, 0f), assetMan.Get<StandardDoorMats>("PrincipalDoorSet"), assetMan.Get<Material>("MapTile_Office")));
            roomSettings.Add("closet", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(1f, 0.6214f, 0f), assetMan.Get<StandardDoorMats>("SuppliesDoorSet")));
            roomSettings.Add("reflex", new RoomSettings(RoomCategory.Null, RoomType.Room, new Color(1f, 1f, 1f), assetMan.Get<StandardDoorMats>("DoctorDoorSet")));
            roomSettings.Add("library", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(0f, 1f, 1f), assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings.Add("cafeteria", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(0f, 1f, 1f), assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings.Add("outside", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(0f, 1f, 1f), assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings.Add("shop", new RoomSettings(RoomCategory.Store, RoomType.Room, new Color(1f, 1f, 1f), assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings["faculty"].container = roomFunctions.Find(x => x.name == "FacultyRoomFunction");
            roomSettings["office"].container = roomFunctions.Find(x => x.name == "OfficeRoomFunction");
            roomSettings["class"].container = roomFunctions.Find(x => x.name == "ClassRoomFunction");
            roomSettings["library"].container = roomFunctions.Find(x => x.name == "LibraryRoomFunction");
            roomSettings["cafeteria"].container = roomFunctions.Find(x => x.name == "CafeteriaRoomFunction");
            roomSettings["outside"].container = roomFunctions.Find(x => x.name == "PlaygroundRoomFunction");
            roomSettings["shop"].container = roomFunctions.Find(x => x.name == "JohnnyStoreRoomFunction");
            yield return "Fetching prefabs...";
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>().Where(x => x.GetInstanceID() >= 0 && x.transform.parent == null).ToArray();
            lightTransforms.Add("fluorescent", transforms.First(x => x.name == "FluorescentLight"));
        }
    }
}
