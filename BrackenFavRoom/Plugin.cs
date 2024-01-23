using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace LethalCompanyTemplate
{
    [HarmonyPatch(typeof(FlowermanAI))]
    [HarmonyPatch("DoAIInterval")]
    class FlowermanAIPatch
    {
        static void Postfix(FlowermanAI __instance)
        {
            Vector3 favRoomPos = GameObject.Find("SmallRoom2(Clone)").transform.position;
            if (__instance.favoriteSpot == null || favRoomPos == __instance.favoriteSpot.transform.position)
                return;
            if (favRoomPos == null)
            {
                Debug.LogError("No Backroom room found!");
                return;
            }
            Debug.Log("Changing the Brackens favorite spot");
            __instance.favoriteSpot.transform.position = favRoomPos;
            Debug.Log($"Changed Brackens favorite spot to X:{favRoomPos.x}, Y:{favRoomPos.y}, Z:{favRoomPos.z}");
        }
    }

    [BepInPlugin("thismicki.plugin.BrackenFavRoom", "BrackenFavRoom", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("thismicki.plugin.BrackenFavRoom");
            harmony.PatchAll();
            Logger.LogInfo($"Patched Methods: {harmony.GetPatchedMethods()}");

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}