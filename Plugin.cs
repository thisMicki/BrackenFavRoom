using System;
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
        static void StartPatch(EnemyAI __instance) // Start just runs once and its better to find the Backrooms just once so I put it here
        {
            if (__instance is not FlowermanAI) return;
            smallRoom = GameObject.Find("SmallRoom2(Clone)"); // The Backroom is called "SmallRoom2" in game and when spawned it gets the "(Clone)" attached so it needs to be added here as well
        }

        [HarmonyPatch("ChooseFarthestNodeFromPosition")] // Patch that function
        [HarmonyPostfix] // After it excecuted
        static void ChooseFarthestNodeFromPositionPatch(EnemyAI __instance, ref Transform __result, ref Vector3 ___mainEntrancePosition) // __instance is acting the class "EnemyAI" and __result is the return value from the base function
        {
            if (__instance is not FlowermanAI) return; // If this script is not attached to a Bracken, it shouldn't change the output
            if (!__instance.IsOwner) return; // Only the host needs to set the favorite spot
            // Debug.Log("BrackenFavRoom: yeee");
            if (smallRoom == null) // If there is no Backrooms spawned in, there is no need to try and change the Brackens favorite spot to it
            {
                if (!errorSend)
                    Debug.LogWarning("BrackenFavRoom: No Backroom room found!"); // Also warn the user about that
                errorSend = true;
                return;
            }

            Vector3 initialPos = smallRoom.transform.position;
            float offsetZ = -7f;
            Vector3 offset = smallRoom.transform.forward * offsetZ;

            Vector3 favoriteSpotPos = initialPos + offset; // To move the position more to the center of the room

            if (__instance.favoriteSpot != null)
                if (Vector3.Distance(__instance.favoriteSpot.position, favoriteSpotPos) < .5f) return; // To not set the position again with a slight margin of error for rounding and stuff

            if (!CheckForPath(favoriteSpotPos, __instance)) // Check if there is a path to the Bracken room, if there isn't and the favorite position would be set the bracken would be stuck when carrying a dead player
            {
                if (!errorSend)
                {
                    Debug.LogWarning($"BrackenFavRoom: No path to the Backrooms from the Brackens current position");
                    Debug.Log($"BrackenFavPos: Choosing new favorite spot...");
                    __instance.ChooseFarthestNodeFromPosition(___mainEntrancePosition);
                }
                errorSend = true;
                return;
            }
            if (__instance.favoriteSpot != null)
                Debug.Log($"BrackenFavRoom: Brackens favorite was X:{__instance.favoriteSpot.transform.position.x}, Y:{__instance.favoriteSpot.transform.position.y}, Z:{__instance.favoriteSpot.transform.position.z}");
            Debug.Log($"BrackenFavRoom: Changed Brackens favorite spot to X:{favoriteSpotPos.x}, Y:{favoriteSpotPos.y}, Z:{smallRoom.transform.position.z}"); // say in the console that the poition has been changed
            __result.position = favoriteSpotPos;  // Change the return value of the base function
        }

        static bool CheckForPath(Vector3 favoriteSpotPos, EnemyAI __instance)
        {
            Vector3 navMeshPos = RoundManager.Instance.GetNavMeshPosition(favoriteSpotPos, RoundManager.Instance.navHit, 1.75f, -1);
            NavMeshPath path = new NavMeshPath();

            return __instance.agent.CalculatePath(navMeshPos, path); // Check if there is a path to the Bracken room, if there isn't and the favorite position would be set the bracken would be stuck when carrying a dead player
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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