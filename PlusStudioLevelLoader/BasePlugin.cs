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
using System.IO;

namespace PlusStudioLevelLoader
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudioloader", "Plus Level Loader", "1.9.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class LevelLoaderPlugin : BaseUnityPlugin
    {
        public static LevelLoaderPlugin Instance;

        public AssetManager assetMan = new AssetManager();

        public Dictionary<string, RoomSettings> roomSettings = new Dictionary<string, RoomSettings>();
        public Dictionary<string, Texture2D> roomTextureAliases = new Dictionary<string, Texture2D>();
        public Dictionary<string, Transform> lightTransforms = new Dictionary<string, Transform>();
        public Dictionary<string, Door> doorPrefabs = new Dictionary<string, Door>();
        public Dictionary<string, WindowObject> windowObjects = new Dictionary<string, WindowObject>();
        public Dictionary<string, TileBasedObject> tileBasedObjectPrefabs = new Dictionary<string, TileBasedObject>();
        public Dictionary<string, LoaderExitData> exitDatas = new Dictionary<string, LoaderExitData>();
        public Dictionary<string, ItemObject> itemObjects = new Dictionary<string, ItemObject>();
        public Dictionary<string, GameObject> basicObjects = new Dictionary<string, GameObject>();
        public Dictionary<string, Activity> activityAliases = new Dictionary<string, Activity>();
        public Dictionary<string, LoaderStructureData> structureAliases = new Dictionary<string, LoaderStructureData>();
        public Dictionary<string, NPC> npcAliases = new Dictionary<string, NPC>();
        public Dictionary<string, PosterObject> posterAliases = new Dictionary<string, PosterObject>();
        public Dictionary<string, RandomEvent> randomEventAliases = new Dictionary<string, RandomEvent>();
        public Dictionary<string, Cubemap> skyboxAliases = new Dictionary<string, Cubemap>();
        public Dictionary<string, Sticker> stickerAliases = new Dictionary<string, Sticker>();
        public Dictionary<string, RoomAsset> roomAssetAliases = new Dictionary<string, RoomAsset>();

        public Pickup stickerPickupPre;

        public static Texture2D RoomTextureFromAlias(string alias)
        {
            if (!Instance.roomTextureAliases.ContainsKey(alias))
            {
                return Instance.roomTextureAliases["PlaceholderWall"];
            }
            return Instance.roomTextureAliases[alias];
        }

        public static PosterObject PosterFromAlias(string alias)
        {
            if (!Instance.posterAliases.ContainsKey(alias))
            {
                return null;
            }
            return Instance.posterAliases[alias];
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
            yield return 6;
            yield return "Fetching misc...";
            stickerPickupPre = Resources.FindObjectsOfTypeAll<Pickup>().First(x => x.GetInstanceID() >= 0 && x.name == "StickerPickup" && x.transform.parent == null);
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
            roomTextureAliases.Add("PlaceholderCeiling", textures.First(x => x.name == "Placeholder_Celing"));
            roomTextureAliases.Add("PlaceholderFloor", textures.First(x => x.name == "Placeholder_Floor"));
            roomTextureAliases.Add("SaloonWall", textures.First(x => x.name == "SaloonWall"));
            roomTextureAliases.Add("MaintenanceFloor", textures.First(x => x.name == "MaintenanceFloor"));
            roomTextureAliases.Add("RedBrickWall", textures.First(x => x.name == "ColoredBrickWall"));
            roomTextureAliases.Add("FactoryCeiling", textures.First(x => x.name == "Factory_Ceiling"));
            roomTextureAliases.Add("LabFloor", textures.First(x => x.name == "LabFloor_Texture"));
            roomTextureAliases.Add("LabWall", textures.First(x => x.name == "LabWall_Texture"));
            roomTextureAliases.Add("LabCeiling", textures.First(x => x.name == "LabCeiling_Texture"));
            roomTextureAliases.Add("Ground2", textures.First(x => x.name == "ground2"));
            roomTextureAliases.Add("DiamondPlateFloor", textures.First(x => x.name == "DiamongPlateFloor"));
            roomTextureAliases.Add("Corn", textures.First(x => x.name == "Corn"));
            roomTextureAliases.Add("BasicFloor", textures.First(x => x.name == "BasicFloor"));
            roomTextureAliases.Add("Black", textures.First(x => x.name == "BlackTexture"));
            roomTextureAliases.Add("Vent", textures.First(x => x.name == "Vent_Base"));
            roomTextureAliases.Add("PlasticTable", textures.First(x => x.name == "PlasticTable"));
            roomTextureAliases.Add(string.Empty, null); // lol

            roomTextureAliases.Add("ElevatorFloor", textures.First(x => x.name == "ElFloor"));
            roomTextureAliases.Add("ElevatorBack", textures.First(x => x.name == "ElBack"));
            yield return "Fetching materials...";
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
            roomSettings.Add("lightbulbtesting", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(1f, 1f, 1f), assetMan.Get<StandardDoorMats>("ClassDoorSet")));
            roomSettings.Add("mystery", new RoomSettings(RoomCategory.Mystery, RoomType.Room, new Color(0f, 1f, 0f), assetMan.Get<StandardDoorMats>("MysteryDoorMats")));
            roomSettings.Add("saferoom", new RoomSettings(RoomCategory.Special, RoomType.Room, new Color(1f, 1f, 1f), assetMan.Get<StandardDoorMats>("SafeRoomDoorSet")));
            roomSettings["faculty"].container = roomFunctions.Find(x => x.name == "FacultyRoomFunction");
            roomSettings["office"].container = roomFunctions.Find(x => x.name == "OfficeRoomFunction");
            roomSettings["class"].container = roomFunctions.Find(x => x.name == "ClassRoomFunction");
            roomSettings["library"].container = roomFunctions.Find(x => x.name == "LibraryRoomFunction");
            roomSettings["cafeteria"].container = roomFunctions.Find(x => x.name == "CafeteriaRoomFunction");
            roomSettings["outside"].container = roomFunctions.Find(x => x.name == "PlaygroundRoomFunction");
            roomSettings["shop"].container = roomFunctions.Find(x => x.name == "JohnnyStoreRoomFunction");
            roomSettings["lightbulbtesting"].container = roomFunctions.Find(x => x.name == "LightbulbTestRoomFunction");
            roomSettings["saferoom"].container = roomFunctions.Find(x => x.name == "SafeRoomRoomFunction");
            CoverInGameRoomFunction saferoom_fix = roomSettings["saferoom"].container.gameObject.AddComponent<CoverInGameRoomFunction>();
            saferoom_fix.hardCover = true;
            roomSettings["saferoom"].container.AddFunction(saferoom_fix); // fix the saferoom from not being covered as intended

            // handle the extra class types for premade rooms
            roomSettings.Add("class_mathmachine", new RoomSettings(RoomCategory.Class, RoomType.Room, Color.green, assetMan.Get<StandardDoorMats>("ClassDoorSet"), assetMan.Get<Material>("MapTile_Classroom")));
            roomSettings["class_mathmachine"].container = roomFunctions.Find(x => x.name == "ClassRoomFunction_MathMachine");
            roomSettings.Add("class_matchactivity", new RoomSettings(RoomCategory.Class, RoomType.Room, Color.green, assetMan.Get<StandardDoorMats>("ClassDoorSet"), assetMan.Get<Material>("MapTile_Classroom")));
            roomSettings["class_matchactivity"].container = roomFunctions.Find(x => x.name == "ClassRoomFunction_MatchActivity");
            roomSettings.Add("class_balloonbuster", new RoomSettings(RoomCategory.Class, RoomType.Room, Color.green, assetMan.Get<StandardDoorMats>("ClassDoorSet"), assetMan.Get<Material>("MapTile_Classroom")));
            roomSettings["class_balloonbuster"].container = roomFunctions.Find(x => x.name == "ClassRoomFunction_BalloonBuster");

            yield return "Fetching prefabs...";
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>().Where(x => x.GetInstanceID() >= 0 && x.transform.parent == null).ToArray();
            lightTransforms.Add("fluorescent", transforms.First(x => x.name == "FluorescentLight"));
            lightTransforms.Add("caged", transforms.First(x => x.name == "CagedLight"));
            lightTransforms.Add("cordedhanging", transforms.First(x => x.name == "CordedHangingLight"));
            lightTransforms.Add("standardhanging", transforms.First(x => x.name == "HangingLight"));
            lightTransforms.Add("null", null);


            Door[] doors = Resources.FindObjectsOfTypeAll<Door>().Where(x => x.GetInstanceID() >= 0 && x.transform.parent == null).ToArray();
            doorPrefabs.Add("standard", doors.First(x => x.name == "ClassDoor_Standard"));
            tileBasedObjectPrefabs.Add("standard", doors.First(x => x.name == "ClassDoor_Standard")); // for premade rooms
            tileBasedObjectPrefabs.Add("swinging", doors.First(x => x.name == "Door_Swinging")); // swinging doors aren't "doors" and don't belong to any room.
            tileBasedObjectPrefabs.Add("oneway", doors.First(x => x.name == "Door_SwingingOneWay")); // swinging doors aren't "doors" and don't belong to any room.
            tileBasedObjectPrefabs.Add("swinging_silent", doors.First(x => x.name == "SilentDoor_Swinging")); // swinging doors aren't "doors" and don't belong to any room.
            tileBasedObjectPrefabs.Add("coinswinging", doors.First(x => x.name == "Door_SwingingCoin")); // swinging doors aren't "doors" and don't belong to any room.
            tileBasedObjectPrefabs.Add("flaps", doors.First(x => x.name == "Door_FlapDoor")); // swinging doors aren't "doors" and don't belong to any room.
            tileBasedObjectPrefabs.Add("autodoor", doors.First(x => x.name == "Door_Auto"));
            doorPrefabs.Add("autodoor", doors.First(x => x.name == "Door_Auto"));
            doorPrefabs.Add("swinging", doors.First(x => x.name == "Door_Swinging")); // swinging doors are smart and may be a regular door somethings
            doorPrefabs.Add("swinging_silent", doors.First(x => x.name == "SilentDoor_Swinging")); // swinging doors are smart and may be a regular door somethings
            doorPrefabs.Add("mysterydoor", doors.First(x => x.name == "SQ_Door_Mystery"));

            WindowObject[] windows = Resources.FindObjectsOfTypeAll<WindowObject>().Where(x => x.GetInstanceID() >= 0).ToArray();
            windowObjects.Add("standard", windows.First(x => x.name == "WoodWindow"));

            exitDatas.Add("elevator", new LoaderExitData() {
                prefab = Resources.FindObjectsOfTypeAll<Elevator>().First(x => x.name == "ElevatorWGate" && x.GetInstanceID() >= 0),
                room = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => ((UnityEngine.Object)x).name == "Room_Elevator" && x.GetInstanceID() >= 0)
            });

            itemObjects.Add("none", ItemMetaStorage.Instance.FindByEnum(Items.None).value);
            itemObjects.Add("quarter", ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value);
            itemObjects.Add("keys", ItemMetaStorage.Instance.FindByEnum(Items.DetentionKey).value);
            itemObjects.Add("zesty", ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value);
            itemObjects.Add("whistle", ItemMetaStorage.Instance.FindByEnum(Items.PrincipalWhistle).value);
            itemObjects.Add("teleporter", ItemMetaStorage.Instance.FindByEnum(Items.Teleporter).value);
            itemObjects.Add("dietbsoda", ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value);
            itemObjects.Add("bsoda", ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value);
            itemObjects.Add("boots", ItemMetaStorage.Instance.FindByEnum(Items.Boots).value);
            itemObjects.Add("clock", ItemMetaStorage.Instance.FindByEnum(Items.AlarmClock).value);
            itemObjects.Add("dirtychalk", ItemMetaStorage.Instance.FindByEnum(Items.ChalkEraser).value);
            itemObjects.Add("grapple", ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value);
            itemObjects.Add("nosquee", ItemMetaStorage.Instance.FindByEnum(Items.Wd40).value);
            itemObjects.Add("nametag", ItemMetaStorage.Instance.FindByEnum(Items.Nametag).value);
            itemObjects.Add("tape", ItemMetaStorage.Instance.FindByEnum(Items.Tape).value);
            itemObjects.Add("scissors", ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value);
            itemObjects.Add("apple", ItemMetaStorage.Instance.FindByEnum(Items.Apple).value);
            itemObjects.Add("swinglock", ItemMetaStorage.Instance.FindByEnum(Items.DoorLock).value);
            itemObjects.Add("portalposter", ItemMetaStorage.Instance.FindByEnum(Items.PortalPoster).value);
            itemObjects.Add("banana", ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value);
            itemObjects.Add("points25", ItemMetaStorage.Instance.GetPointsObject(25, true));
            itemObjects.Add("points50", ItemMetaStorage.Instance.GetPointsObject(50, true));
            itemObjects.Add("points100", ItemMetaStorage.Instance.GetPointsObject(100, true));
            itemObjects.Add("points250", ItemMetaStorage.Instance.GetPointsObject(250, true));
            itemObjects.Add("buspass", ItemMetaStorage.Instance.FindByEnum(Items.BusPass).value);
            itemObjects.Add("inviselixer", ItemMetaStorage.Instance.FindByEnum(Items.InvisibilityElixir).value);
            itemObjects.Add("reachextend", ItemMetaStorage.Instance.FindByEnum(Items.ReachExtender).value);
            itemObjects.Add("shapekey_circle", ItemMetaStorage.Instance.FindByEnum(Items.CircleKey).value);
            itemObjects.Add("shapekey_square", ItemMetaStorage.Instance.FindByEnum(Items.SquareKey).value);
            itemObjects.Add("shapekey_triangle", ItemMetaStorage.Instance.FindByEnum(Items.TriangleKey).value);
            itemObjects.Add("shapekey_weird", ItemMetaStorage.Instance.FindByEnum(Items.WeirdKey).value);
            itemObjects.Add("shapekey_star", ItemMetaStorage.Instance.FindByEnum(Items.PentagonKey).value);
            itemObjects.Add("shapekey_heart", ItemMetaStorage.Instance.FindByEnum(Items.HexagonKey).value);

            //TBA
            ItemMetaData stickerMeta = ItemMetaStorage.Instance.FindByEnum(Items.StickerPack);
            itemObjects.Add("stickerpack", stickerMeta.itemObjects.First(x => x.name == "StickerPack_Normal"));
            itemObjects.Add("stickerpack_large", stickerMeta.itemObjects.First(x => x.name == "StickerPack_Large"));
            itemObjects.Add("stickerpack_twin", stickerMeta.itemObjects.First(x => x.name == "StickerPack_Twin"));
            itemObjects.Add("stickerpack_bonus", stickerMeta.itemObjects.First(x => x.name == "StickerPack_Bonus"));
            itemObjects.Add("stickerpack_fresh", stickerMeta.itemObjects.First(x => x.name == "StickerPack_Fresh"));
            //itemObjects.Add("stickerpack_sticky", ItemMetaStorage.Instance.Find(x => x.id == Items.StickerPack && x.value.name == "StickerPack_Sticky").value);
            itemObjects.Add("gluestick", stickerMeta.itemObjects.First(x => x.name == "GlueStick"));

            // code ported from legacy editor because retyping all of these would be annoying
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => (x.GetInstanceID() >= 0) && (x.transform.parent == null)).ToArray();
            basicObjects.Add("desk", objects.First(x => (x.name == "Table_Test")));
            basicObjects.Add("bigdesk", objects.First(x => (x.name == "BigDesk")));
            basicObjects.Add("cabinet", objects.First(x => (x.name == "FilingCabinet_Tall")));
            basicObjects.Add("chair", objects.First(x => (x.name == "Chair_Test")));
            basicObjects.Add("computer", objects.First(x => (x.name == "MyComputer")));
            basicObjects.Add("computer_off", objects.First(x => (x.name == "MyComputer_Off")));
            basicObjects.Add("roundtable", objects.First(x => (x.name == "RoundTable")));
            basicObjects.Add("locker", objects.First(x => (x.name == "Locker")));
            basicObjects.Add("bluelocker", objects.First(x => (x.name == "BlueLocker")));
            basicObjects.Add("greenlocker", objects.First(x => (x.name == "StorageLocker")));
            basicObjects.Add("decor_pencilnotes", objects.First(x => (x.name == "Decor_PencilNotes")));
            basicObjects.Add("decor_papers", objects.First(x => (x.name == "Decor_Papers")));
            basicObjects.Add("decor_globe", objects.First(x => (x.name == "Decor_Globe")));
            basicObjects.Add("decor_notebooks", objects.First(x => (x.name == "Decor_Notebooks")));
            basicObjects.Add("decor_lunch", objects.First(x => (x.name == "Decor_Lunch")));
            basicObjects.Add("bookshelf", objects.First(x => (x.name == "Bookshelf_Object")));
            basicObjects.Add("bookshelf_hole", objects.First(x => (x.name == "Bookshelf_Hole_Object")));
            basicObjects.Add("rounddesk", objects.First(x => (x.name == "RoundDesk")));
            basicObjects.Add("cafeteriatable", objects.First(x => (x.name == "CafeteriaTable")));
            basicObjects.Add("dietbsodamachine", objects.First(x => (x.name == "DietSodaMachine")));
            basicObjects.Add("bsodamachine", objects.First(x => (x.name == "SodaMachine")));
            basicObjects.Add("zestymachine", objects.First(x => (x.name == "ZestyMachine")));
            basicObjects.Add("crazymachine_bsoda", objects.First(x => (x.name == "CrazyVendingMachineBSODA")));
            basicObjects.Add("crazymachine_zesty", objects.First(x => (x.name == "CrazyVendingMachineZesty")));
            basicObjects.Add("waterfountain", objects.First(x => (x.name == "WaterFountain")));
            basicObjects.Add("counter", objects.First(x => (x.name == "Counter")));
            basicObjects.Add("examinationtable", objects.First(x => (x.name == "ExaminationTable")));
            basicObjects.Add("ceilingfan", objects.First(x => (x.name == "CeilingFan")));
            basicObjects.Add("merrygoround", objects.First(x => (x.name == "MerryGoRound_Object")));
            basicObjects.Add("tree", objects.First(x => (x.name == "TreeCG")));
            basicObjects.Add("pinetree", objects.First(x => (x.name == "PineTree")));
            basicObjects.Add("appletree", objects.First(x => (x.name == "AppleTree")));
            basicObjects.Add("bananatree", objects.First(x => (x.name == "BananaTree")));
            basicObjects.Add("hoop", objects.First(x => (x.name == "HoopBase")));
            basicObjects.Add("payphone", objects.First(x => (x.name == "PayPhone")));
            basicObjects.Add("tapeplayer", objects.First(x => (x.name == "TapePlayer")));
            basicObjects.Add("plant", objects.First(x => (x.name == "Plant")));
            basicObjects.Add("decor_banana", objects.First(x => (x.name == "Decor_Banana")));
            basicObjects.Add("decor_zoneflag", objects.First(x => (x.name == "Decor_ZoningFlag")));
            basicObjects.Add("hopscotch", objects.First(x => (x.name == "PlaygroundPavement")));
            basicObjects.Add("dirtcircle", objects.First(x => (x.name == "DirtCircle")));
            basicObjects.Add("chairsanddesk", objects.First(x => (x.name == "Chairs_Desk_Perfect")));
            basicObjects.Add("picnictable", objects.First(x => (x.name == "PicnicTable")));
            basicObjects.Add("tent", objects.First(x => (x.name == "Tent_Object")));
            basicObjects.Add("rock", objects.First(x => (x.name == "Rock")));
            basicObjects.Add("picnicbasket", objects.First(x => (x.name == "PicnicBasket")));
            basicObjects.Add("pedestal", objects.First(x => (x.name == "Decor_Pedestal")));
            basicObjects.Add("arrow", objects.First(x => x.name == "Arrow"));
            basicObjects.Add("exitsign", objects.First(x => x.name == "Decor_ExitSign"));
            basicObjects.Add("johnnysign", objects.First(x => x.name == "JohnnySign"));
            basicObjects.Add("mysterymarks", objects.First(x => x.name == "MysteryRoomMarks"));
            basicObjects.Add("packetomatic", objects.First(x => x.name == "Packet_O_Matic"));

            // activities
            Activity[] activites = Resources.FindObjectsOfTypeAll<Activity>().Where(x => x.GetInstanceID() >= 0).ToArray();
            activityAliases.Add("notebook", activites.First(x => x.name == "NoActivity"));
            activityAliases.Add("mathmachine", activites.First(x => (x.name == "MathMachine" && (x.transform.parent == null))));
            activityAliases.Add("mathmachine_corner", activites.First(x => (x.name == "MathMachine_Corner" && (x.transform.parent == null))));
            activityAliases.Add("balloonbuster", activites.First(x => (x.name == "Activity_BalloonBuster" && (x.transform.parent == null))));
            activityAliases.Add("matchmachine", activites.First(x => (x.name == "Activity_Match" && (x.transform.parent == null))));

            // structures

            StructureBuilder[] builders = Resources.FindObjectsOfTypeAll<StructureBuilder>().Where(x => x.GetInstanceID() >= 0).ToArray();
            structureAliases.Add("facultyonlydoor", new LoaderStructureData(builders.First(x => x.name == "FacultyOnlyDoorConstructor")));
            LockdownDoor[] lockdownDoors = Resources.FindObjectsOfTypeAll<LockdownDoor>().Where(x => x.GetInstanceID() >= 0).ToArray();
            structureAliases.Add("lockdowndoor", new LoaderStructureData(builders.First(x => x.name == "LockdownDoorConstructor"), new Dictionary<string, GameObject>() { { "lockdowndoor_shut", lockdownDoors.First(x => x.name == "LockdownDoor_Shut").gameObject } }));
            structureAliases.Add("lockdowndoor_button", new LoaderStructureData(builders.First(x => x.name == "LockdownDoorConstructor_Button"), new Dictionary<string, GameObject>() { { "lockdowndoor_shut_stayopen", lockdownDoors.First(x => x.name == "LockdownDoor_Shut_StaysOpen").gameObject } }));
            structureAliases.Add("conveyorbelt", new LoaderStructureData(builders.First(x => x.name == "ConveyorBeltConstructor")));
            structureAliases.Add("vent", new LoaderStructureData(builders.First(x => x.name == "Structure_Vent")));

            structureAliases.Add("lockers", new LoaderStructureData(builders.First(x => x.name == "Structure_Lockers")));
            // the factory box doesn't work outside of randomly generated levels
            //structureAliases.Add("factorybox", new LoaderStructureData(builders.First(x => x.name == "FactoryBoxConstructor")));

            structureAliases.Add("studentspawner", new LoaderStructureData(builders.First(x => x.name == "StudentSpawnerConstructor")));

            // npcs
            npcAliases.Add("baldi", MTM101BaldiDevAPI.npcMetadata.Get(Character.Baldi).value);
            npcAliases.Add("principal", MTM101BaldiDevAPI.npcMetadata.Get(Character.Principal).value);
            npcAliases.Add("sweep", MTM101BaldiDevAPI.npcMetadata.Get(Character.Sweep).value);
            npcAliases.Add("playtime", MTM101BaldiDevAPI.npcMetadata.Get(Character.Playtime).value);
            npcAliases.Add("chalkface", MTM101BaldiDevAPI.npcMetadata.Get(Character.Chalkles).value);
            npcAliases.Add("bully", MTM101BaldiDevAPI.npcMetadata.Get(Character.Bully).value);
            npcAliases.Add("beans", MTM101BaldiDevAPI.npcMetadata.Get(Character.Beans).value);
            npcAliases.Add("prize", MTM101BaldiDevAPI.npcMetadata.Get(Character.Prize).value);
            npcAliases.Add("crafters", MTM101BaldiDevAPI.npcMetadata.Get(Character.Crafters).value);
            npcAliases.Add("pomp", MTM101BaldiDevAPI.npcMetadata.Get(Character.Pomp).value);
            npcAliases.Add("test", MTM101BaldiDevAPI.npcMetadata.Get(Character.LookAt).value);
            npcAliases.Add("cloudy", MTM101BaldiDevAPI.npcMetadata.Get(Character.Cumulo).value);
            npcAliases.Add("reflex", MTM101BaldiDevAPI.npcMetadata.Get(Character.DrReflex).value);

            // auto add all posters i cant be bothered
            List<PosterObject> allBasePosters = Resources.FindObjectsOfTypeAll<PosterObject>().Where(x => x.GetInstanceID() >= 0).ToList();
            List<PosterObject> toRemove = new List<PosterObject>();
            foreach (PosterObject poster in allBasePosters)
            {
                if (poster.multiPosterArray.Length == 0) continue;
                toRemove.AddRange(poster.multiPosterArray);
                toRemove.Remove(poster);
            }
            allBasePosters.RemoveAll(x => toRemove.Contains(x));
            foreach (PosterObject poster in allBasePosters)
            {
                if (poster.name.StartsWith("Chk_Tut_"))
                {
                    continue;
                }
                if (poster.name == ("StoreNeon_4_wRestockBase"))
                {
                    continue;
                }
                
                posterAliases.Add(poster.name, poster);
            }

            // random events
            RandomEvent[] randomEvents = Resources.FindObjectsOfTypeAll<RandomEvent>().Where(x => x.GetInstanceID() >= 0).ToArray();

            randomEventAliases.Add("fog", randomEvents.First(x => x.name == "Event_Fog"));
            randomEventAliases.Add("testprocedure", randomEvents.First(x => x.name == "Event_TestProcedure"));
            randomEventAliases.Add("flood", randomEvents.First(x => x.name == "Event_Flood"));
            randomEventAliases.Add("party", randomEvents.First(x => x.name == "Event_Party"));
            randomEventAliases.Add("brokenruler", randomEvents.First(x => x.name == "Event_BrokenRuler"));
            randomEventAliases.Add("gravitychaos", randomEvents.First(x => x.name == "Event_GravityChaos"));
            randomEventAliases.Add("mysteryroom", randomEvents.First(x => x.name == "Event_MysteryRoom"));
            randomEventAliases.Add("timeout", randomEvents.First(x => x.name == "Event_TimeOut_Base"));
            randomEventAliases.Add("studentshuffle", randomEvents.First(x => x.name == "Event_StudentShuffle"));
            randomEventAliases.Add("balderdash", randomEvents.First(x => x.name == "Event_BalderDash"));

            // skyboxes
            Cubemap[] skyboxes = Resources.FindObjectsOfTypeAll<Cubemap>().Where(x => x.GetInstanceID() >= 0).ToArray();
            skyboxAliases.Add("default", skyboxes.First(x => x.name == "Cubemap_Default"));
            skyboxAliases.Add("daystandard", skyboxes.First(x => x.name == "Cubemap_DayStandard"));
            skyboxAliases.Add("twilight", skyboxes.First(x => x.name == "Cubemap_Twilight"));
            skyboxAliases.Add("void", skyboxes.First(x => x.name == "Cubemap_Void"));

            RoomAsset[] allRoomAssets = Resources.FindObjectsOfTypeAll<RoomAsset>().Where(x => x.GetInstanceID() >= 0).ToArray();
            roomAssetAliases.Add("johnny_store", allRoomAssets.First(x => ((ScriptableObject)x).name == "Room_JohnnysStore"));

            yield return "Defining misc aliases...";
            stickerAliases.Add("nothing", Sticker.Nothing);
            stickerAliases.Add("lingering_hiding", Sticker.LingeringHiding);
            stickerAliases.Add("baldi_praise", Sticker.BaldiPraise);
            stickerAliases.Add("stamina", Sticker.Stamina);
            stickerAliases.Add("elevator", Sticker.Elevator);
            stickerAliases.Add("time_extension", Sticker.TimeExtension);
            stickerAliases.Add("stealth", Sticker.Stealth);
            stickerAliases.Add("inventory_slot", Sticker.InventorySlot);
            stickerAliases.Add("silence", Sticker.Silence);
            stickerAliases.Add("reach", Sticker.Reach);
            stickerAliases.Add("map_range", Sticker.MapRange);
            stickerAliases.Add("door_stop", Sticker.DoorStop);
            stickerAliases.Add("ytp_multiplier", Sticker.YtpMulitplier);
            stickerAliases.Add("baldi_countdown", Sticker.BaldiCountdown);
            stickerAliases.Add("gluestick", Sticker.GlueStick);
            stickerAliases.Add("distance_bonus", Sticker.DistanceBonus);
            stickerAliases.Add("sticker_bonus", Sticker.StickerBonus);
            stickerAliases.Add("exploration_bonus", Sticker.ExplorationBonus);
            stickerAliases.Add("time_bonus", Sticker.TimeBonus);
        }
    }
}
