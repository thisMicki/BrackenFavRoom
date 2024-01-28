using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace BrackenFavRoom
{
    [HarmonyPatch(typeof(EnemyAI))] // Patch something in the EnemyAI class
    class EnemyAIPatch
    {
        static GameObject smallRoom;
        static bool errorSend = false;

        [HarmonyPatch("Start")] // Patch that function
        [HarmonyPostfix] // After it excecuted
        static void StartPatch() // Start just runs once and its better to find the Backrooms just once so I put it here
        {
            smallRoom = GameObject.Find("SmallRoom2(Clone)"); // The Backroom is called "SmallRoom2" in game and when spawned it gets the "(Clone)" attached so it needs to be added here as well
        }

        [HarmonyPatch("ChooseFarthestNodeFromPosition")] // Patch that function
        [HarmonyPostfix] // After it excecuted
        static void ChooseFarthestNodeFromPositionPatch(EnemyAI __instance, ref Transform __result) // __instance is acting the class "EnemyAI" and __result is the return value from the base function
        {
            if (!__instance.IsOwner) return; // Only the host needs to set the favorite spot, i think
            if (__instance is not FlowermanAI) return; // If this script is not attached to a Bracken, it shouldn't change the output

            if (smallRoom == null) // If there is no Backrooms spawned in, there is no need to try and change the Brackens favorit spot to it
            {
                if (!errorSend)
                    Debug.LogWarning("BrackenFavRoom: No Backroom room found!"); // Also warn the user about that
                errorSend = true;
                return;
            }

            Vector3 smallRoomPos = smallRoom.transform.position;
            smallRoomPos.x += 5; // To move the position more to the center of the room

            Vector3 navMeshPos = RoundManager.Instance.GetNavMeshPosition(smallRoomPos, RoundManager.Instance.navHit, 1.75f, -1);
            NavMeshPath path = new NavMeshPath();
            if (!__instance.agent.CalculatePath(navMeshPos, path)) // Check if there is a path to the Bracken room, if there isn't and the favorit position would be set the bracken would be stuck when carrying a dead player
            {
                Debug.LogWarning("BrackenFavRoom: There is no path to the Backrooms from the Brackens current position");
                return;
            }
            
            Debug.Log($"BrackenFavRoom: Changed Brackens favorite spot to X:{smallRoom.transform.position.x}, Y:{smallRoom.transform.position.y}, Z:{smallRoom.transform.position.z}"); // say in the console that the poition has been changed
            __result.position = smallRoomPos;  // Change the return value of the base function
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("readthisifbad-SnatchinBracken-1.2.9")] // Something breaks when that mod is also loaded
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("micki.plugin.BrackenFavRoom");
            harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}