using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BrackenFavRoom
{
    [HarmonyPatch(typeof(EnemyAI))] // Patch something in the EnemyAI class
    class EnemyAIPatch
    {
        static GameObject smallRoom;

        [HarmonyPatch("Start")] // Patch that function
        [HarmonyPostfix] // After it ran
        static void StartPatch() // Start just runs once and its better to find the Backrooms just once so I put it here
        {
            smallRoom = GameObject.Find("SmallRoom2(Clone)"); // The Backroom is called "SmallRoom2" in game and when spawned it gets the "(Clone)" attached so it needs to be added here as well
        }

        [HarmonyPatch("ChooseFarthestNodeFromPosition")] // Patch that function
        [HarmonyPostfix] // After it ran
        static void ChooseFarthestNodeFromPositionPatch(EnemyAI __instance, ref Transform __result) // __instance is acting the class "EnemyAI" and __result is the return value from the base function
        {
            if (__instance is not FlowermanAI) return; // If this script is not attached to a Bracken, it shouldn't change the output

            if (smallRoom == null) // If there is no Backrooms spawned in, there is no need to try and change the Brackens favorit spot to it
            {
                Debug.LogWarning("BrackenFavRoom: No Backroom room found!"); // Also warn the console about that
                return;
            }

            // GameObject[] nodesTempArray = (
            //     from x in __instance.allAINodes
            //     orderby Vector3.Distance(pos, x.transform.position) descending // For some reason this doesn't work so I'll just do it more inefficient :)
            //     select x).ToArray<GameObject>();

            GameObject closestNode = null; // If I don't assign null, I can't use it after the foreach loop
            float closestDistance = 43210; // it's high just to make sure a node gets picked

            foreach (GameObject node in __instance.allAINodes) // Check all nodes for the closeset one to the Backrooms
            {
                float distance = Vector3.Distance(smallRoom.transform.position, node.transform.position); // The distance between the Backrooms and the currently selected node
                if (distance < closestDistance)
                {
                    closestNode = node;
                    closestDistance = distance;
                }
            }
            Debug.Log($"BrackenFavRoom: Changed Brackens favorite spot to X:{closestNode.transform.position.x}, Y:{closestNode.transform.position.y}, Z:{closestNode.transform.position.z}"); // say in the console that the poition has been changed
            __result = closestNode.transform; // Change the return value of the base function
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("read__instanceifbad-SnatchinBracken-1.2.8")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("__instancemicki.plugin.BrackenFavRoom");
            harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
