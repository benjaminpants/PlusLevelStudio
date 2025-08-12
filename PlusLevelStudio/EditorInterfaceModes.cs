using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorInterfaceModes
    {

        internal static List<Action<EditorMode, bool>> toCallAfterEditorMode = new List<Action<EditorMode, bool>>();

        public static void AddModeCallback(Action<EditorMode, bool> callback)
        {
            if (LevelStudioPlugin.editorModesDefined)
            {
                foreach (EditorMode mode in LevelStudioPlugin.Instance.modes.Values)
                {
                    callback(mode, mode.vanillaComplaint);
                }
            }
            toCallAfterEditorMode.Add(callback);
        }

        /// <summary>
        /// Adds the vanilla tools (the ones in the tools category) to the specified EditorMode
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaToolTools(EditorMode modeToModify)
        {
            if (modeToModify.caresAboutSpawn)
            {
                AddToolsToCategory(modeToModify, "tools", new EditorTool[]
                {
                    new ElevatorTool("elevator", true),
                    new ElevatorTool("elevator", false),
                }, true);
            }
            AddToolsToCategory(modeToModify, "tools", new EditorTool[]
            {
                new WallTool(true),
                new WallTool(false),
            }, true);
            if (modeToModify.caresAboutSpawn)
            {
                AddToolToCategory(modeToModify, "tools", new SpawnpointTool());
            }
            AddToolsToCategory(modeToModify, "tools", new EditorTool[]
            {
                new MergeTool(),
                new DeleteTool(),
            });
        }

        /// <summary>
        /// Add the vanilla doors to the specified EditorMode
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaDoors(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "doors", new EditorTool[]
            {
                new DoorTool("standard"),
                new DoorTool("swinging"),
                new DoorTool("oneway"),
                new DoorTool("coinswinging"),
                new DoorTool("swinging_silent"),
                new DoorTool("autodoor"),
                new DoorTool("flaps"),
                new WindowTool("standard"),
            }, true);
        }

        /// <summary>
        /// Add the vanilla lights to the specified EditorMode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaLights(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "lights", new EditorTool[]
            {
                new LightTool("fluorescent"),
                new LightTool("caged"),
                new LightTool("cordedhanging"),
                new LightTool("standardhanging"),
                new LightTool("null")
            }, true);
        }

        /// <summary>
        /// Add the vanilla activities to the specified EditorMode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaActivities(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "activities", new EditorTool[]
            {
                new ActivityTool("notebook", 5f),
                new ActivityTool("mathmachine", 0f),
                new ActivityTool("mathmachine_corner", 0f),
            }, true);
        }

        /// <summary>
        /// Add the vanilla posters to the specified EditorMode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaPosters(EditorMode modeToModify)
        {
            List<PosterObject> allPosters = LevelLoaderPlugin.Instance.posterAliases.Values.Where(x => x.GetInstanceID() >= 0).ToList();
            allPosters.Sort((a, b) =>
            {
                int texNameCompare = a.baseTexture.name.CompareTo(b.baseTexture.name);
                if (texNameCompare != 0)
                {
                    return texNameCompare;
                }
                return a.name.CompareTo(b.name);
            });
            AddToolsToCategory(modeToModify, "posters", allPosters.Select(z => new PosterTool(LevelLoaderPlugin.Instance.posterAliases.First(x => x.Value == z).Key)), true);
        }

        /// <summary>
        /// Add the vanilla NPCs to the specified EditorMode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaNPCs(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "npcs", new EditorTool[]
            {
                new NPCTool("baldi"),
                new NPCTool("principal"),
                new NPCTool("sweep"),
                new NPCTool("playtime"),
                new NPCTool("bully"),
                new NPCTool("crafters"),
                new NPCTool("prize"),
                new NPCTool("cloudy"),
                new NPCTool("chalkface"),
                new NPCTool("beans"),
                new NPCTool("pomp"),
                new NPCTool("test"),
                new NPCTool("reflex"),
            }, true);
        }

        /// <summary>
        /// Add the vanilla random events to the specified EditorMode.
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="includeNonVanillaComplaint"></param>
        public static void AddVanillaEvents(EditorMode modeToModify, bool includeNonVanillaComplaint)
        {
            modeToModify.availableRandomEvents.AddRange(new string[]
            {
                "fog",
                "flood",
                "brokenruler",
                "party",
                "mysteryroom",
                "testprocedure",
                "gravitychaos"
            });
        }

        /// <summary>
        /// Add the vanilla structures to the specified editor mode.
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="includeNonVanillaComplaintTools">If true, includes tools that require editor specific versions of the structures to work</param>
        public static void AddVanillaStructures(EditorMode modeToModify, bool includeNonVanillaComplaintTools)
        {
            AddToolsToCategory(modeToModify, "structures", new EditorTool[]
            {
                new HallDoorStructureTool("facultyonlydoor"),
                new HallDoorWithButtonsTool("lockdowndoor"),
                new ConveyorBeltTool("conveyorbelt", true),
                new ConveyorBeltTool("conveyorbelt", false),
                new VentTool("vent")
            }, true);
            if (!includeNonVanillaComplaintTools) return;
            AddToolsToCategory(modeToModify, "structures", new EditorTool[]
            {
                new ShapeLockTool("shapelock_circle"),
                new ShapeLockTool("shapelock_triangle"),
                new ShapeLockTool("shapelock_square"),
                new ShapeLockTool("shapelock_star"),
                new ShapeLockTool("shapelock_heart"),
                new ShapeLockTool("shapelock_weird"),
                new PowerLeverAlarmTool(),
                new PowerLeverLeverTool(CableColor.red),
                new PowerLeverLeverTool(CableColor.yellow),
                new PowerLeverLeverTool(CableColor.green),
                new PowerLeverLeverTool(CableColor.cyan),
                new PowerLeverLeverTool(CableColor.blue),
                new PowerLeverLeverTool(CableColor.magenta),
                new PowerLeverLeverTool(CableColor.white),
                new PowerLeverLeverTool(CableColor.gray),
                new PowerLeverLeverTool(CableColor.black),
                new PowerLeverBreakerTool(),
            }, true);
        }

        /// <summary>
        /// Add the vanilla items to the specified editor mode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaItems(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "items", new EditorTool[]
            {
                new ItemTool("quarter"),
                new ItemTool("dietbsoda"),
                new ItemTool("bsoda"),
                new ItemTool("zesty"),
                new ItemTool("banana"),
                new ItemTool("scissors"),
                new ItemTool("boots"),
                new ItemTool("nosquee"),
                new ItemTool("keys"),
                new ItemTool("tape"),
                new ItemTool("clock"),
                new ItemTool("swinglock"),
                new ItemTool("whistle"),
                new ItemTool("dirtychalk"),
                new ItemTool("nametag"),
                new ItemTool("inviselixer"),
                new ItemTool("reachextend"),
                new ItemTool("teleporter"),
                new ItemTool("portalposter"),
                new ItemTool("grapple"),
                new ItemTool("apple"),
                new ItemTool("buspass"),
                new ItemTool("shapekey_circle"),
                new ItemTool("shapekey_triangle"),
                new ItemTool("shapekey_square"),
                new ItemTool("shapekey_star"),
                new ItemTool("shapekey_heart"),
                new ItemTool("shapekey_weird"),
                new ItemTool("points25", LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/items_points25")),
                new ItemTool("points50", LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/items_points50")),
                new ItemTool("points100", LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/items_points100")),
            }, true);
        }

        /// <summary>
        /// Adds the vanilla rooms to the specified editor mode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaRooms(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "rooms", new EditorTool[]
            {
                    new RoomTool("hall"),
                    new RoomTool("class"),
                    new RoomTool("faculty"),
                    new RoomTool("office"),
                    new RoomTool("closet"),
                    new RoomTool("reflex"),
                    new OnlyOneRoomTool("mystery"),
                    new RoomTool("cafeteria"),
                    new RoomTool("outside"),
                    new RoomTool("library"),
                    new RoomTool("lightbulbtesting")
            }, true);
        }

        /// <summary>
        /// Adds the vanilla objects to the specified editor mode.
        /// </summary>
        /// <param name="modeToModify"></param>
        public static void AddVanillaObjects(EditorMode modeToModify)
        {
            AddToolsToCategory(modeToModify, "objects", new EditorTool[]
            {
                new ObjectTool("bigdesk"),
                new ObjectTool("desk"),
                new ObjectTool("chair"),
                new BulkObjectTool("chairdesk", new BulkObjectData[]
                {
                    new BulkObjectData("chair", new Vector3(0f,0f,-2f)),
                    new BulkObjectData("desk", new Vector3(0f,0f,0f)),
                }),
                new ObjectTool("roundtable"),
                new BulkObjectTool("roundtable1", new BulkObjectData[]
                {
                    new BulkObjectData("roundtable", new Vector3(0f,0f,0f)),
                    new BulkObjectData("chair", new Vector3(0f,0f,-5f)),
                    new BulkObjectData("chair", new Vector3(-5f,0f,0f), new Vector3(0f, 90f, 0f)),
                    new BulkObjectData("chair", new Vector3(0f,0f,5f), new Vector3(0f, 180f, 0f)),
                    new BulkObjectData("chair", new Vector3(5f,0f,0f), new Vector3(0f, 270f, 0f))
                }),
                new BulkObjectTool("roundtable2", new BulkObjectData[]
                {
                    new BulkObjectData("roundtable", new Vector3(0f,0f,0f)),
                    new BulkObjectData("chair", new Vector3(3.5355f,0f,-3.535f), new Vector3(0f, 315f, 0f)),
                    new BulkObjectData("chair", new Vector3(-3.5355f,0f,-3.535f), new Vector3(0f, 45f, 0f)),
                    new BulkObjectData("chair", new Vector3(-3.5355f,0f,3.535f), new Vector3(0f, 135f, 0f)),
                    new BulkObjectData("chair", new Vector3(3.5355f,0f,3.535f), new Vector3(0f, 225f, 0f))
                }),
                new ObjectTool("cabinet"),
                new ObjectTool("cafeteriatable"),
                new ObjectTool("waterfountain"),
                new ObjectTool("dietbsodamachine"),
                new ObjectTool("bsodamachine"),
                new ObjectTool("zestymachine"),
                new ObjectTool("crazymachine_bsoda"),
                new ObjectTool("crazymachine_zesty"),
                new ObjectToolNoRotation("payphone"),
                new ObjectToolNoRotation("tapeplayer", 5f),
                new ObjectTool("locker"),
                new ObjectTool("bluelocker"),
                new ObjectTool("greenlocker"),
                new BulkObjectTool("multilockers", new BulkObjectData[]
                {
                    new BulkObjectData("locker", new Vector3(-4f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(-2f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(0f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(2f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(4f,0f,4f))
                }),
                new BulkObjectRandomizedTool("lockerrandomblue", new BulkObjectData[]
                {
                    new BulkObjectData("locker", new Vector3(-4f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(-2f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(0f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(2f,0f,4f)),
                    new BulkObjectData("locker", new Vector3(4f,0f,4f))
                }, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("locker", "bluelocker") }),
                new ObjectToolNoRotation("plant"),
                new ObjectToolNoRotation("decor_banana", 3.75f),
                new ObjectToolNoRotation("decor_globe", 3.75f),
                new ObjectToolNoRotation("decor_lunch", 3.75f),
                new ObjectToolNoRotation("decor_notebooks", 3.75f),
                new ObjectToolNoRotation("decor_papers", 3.75f),
                new ObjectToolNoRotation("decor_pencilnotes", 3.75f),
                new ObjectToolNoRotation("ceilingfan"),
                new ObjectTool("computer", 3.75f),
                new ObjectTool("computer_off", 3.75f),
                new ObjectTool("rounddesk"),
                new ObjectTool("bookshelf"),
                new ObjectTool("bookshelf_hole"),
                new ObjectTool("counter"),
                new ObjectTool("examinationtable"),
                new ObjectToolNoRotation("pedestal"),
                new ObjectToolNoRotation("pinetree"),
                new ObjectToolNoRotation("tree"),
                new ObjectToolNoRotation("appletree"),
                new ObjectToolNoRotation("bananatree"),
                new ObjectTool("merrygoround"),
                new ObjectTool("hopscotch"),
                new ObjectTool("hoop"),
                new ObjectTool("picnictable"),
                new ObjectToolNoRotation("picnicbasket"),
                new ObjectToolNoRotation("rock"),
                new ObjectTool("tent"),
                new ObjectToolNoRotation("decor_zoneflag"),
                new ObjectTool("arrow", 5f),
                new ObjectToolNoRotation("exitsign", 10f),
            }, true);
        }

        /// <summary>
        /// Insert tools after the specified tool in the category.
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="category"></param>
        /// <param name="idToInsertAt"></param>
        /// <param name="tools"></param>
        /// <returns></returns>
        public static bool InsertToolsInCategory(EditorMode modeToModify, string category, string idToInsertAt, IEnumerable<EditorTool> tools)
        {
            if (!modeToModify.availableTools.ContainsKey(category)) return false;
            int index = modeToModify.availableTools[category].FindIndex(0, x => x.id == idToInsertAt);
            if (index == -1)
            {
                return AddToolsToCategory(modeToModify, category, tools, false);
            }
            modeToModify.availableTools[category].InsertRange(index + 1, tools);
            return true;
        }

        /// <summary>
        /// Insert a tool after the specified tool in the category.
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="category"></param>
        /// <param name="idToInsertAt"></param>
        /// <param name="tool"></param>
        /// <returns></returns>
        public static bool InsertToolInCategory(EditorMode modeToModify, string category, string idToInsertAt, EditorTool tool)
        {
            return InsertToolsInCategory(modeToModify, category, idToInsertAt, new EditorTool[1] { tool });
        }

        /// <summary>
        /// Add the tools into the specified category of the EditorMode if it exists.
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="category"></param>
        /// <param name="tools"></param>
        /// <param name="addCategoryIfDoesntExist"></param>
        /// <returns></returns>
        public static bool AddToolsToCategory(EditorMode modeToModify, string category, IEnumerable<EditorTool> tools, bool addCategoryIfDoesntExist = false)
        {
            if (!modeToModify.availableTools.ContainsKey(category))
            {
                if (!addCategoryIfDoesntExist) return false;
                modeToModify.availableTools.Add(category, new List<EditorTool>());
                if (!modeToModify.defaultTools.Contains(category))
                {
                    modeToModify.defaultTools = modeToModify.defaultTools.AddToArray(category);
                }
            }
            modeToModify.availableTools[category].AddRange(tools);
            return true;
        }

        /// <summary>
        /// Add a tool into the specified category of the EditorMode if it exists
        /// </summary>
        /// <param name="modeToModify"></param>
        /// <param name="category"></param>
        /// <param name="tool"></param>
        /// <param name="addCategoryIfDoesntExist"></param>
        /// <returns></returns>
        public static bool AddToolToCategory(EditorMode modeToModify, string category, EditorTool tool, bool addCategoryIfDoesntExist = false)
        {
            return AddToolsToCategory(modeToModify, category, new EditorTool[1] { tool }, addCategoryIfDoesntExist);
        }
    }
}
