using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BrackenFavRoom
{
    [HarmonyPatch(typeof(FlowermanAI))]
    
    class FlowermanAIPatch
    {
        static bool errorSend = false;
        static GameObject SmallRoom;
        static Vector3 favRoomPos = Vector3.zero;

        [HarmonyPatch("Start")]
        static void Postfix(){
            SmallRoom = GameObject.Find("SmallRoom2(Clone)");
        }


        [HarmonyPatch("DoAIInterval")]
        static void Postfix(FlowermanAI __instance)
        {
            if (SmallRoom != null)
            {
                favRoomPos = SmallRoom.transform.position;
            }
            else if(!errorSend)
            {
                Debug.LogError("BrackenFavRoom: No Backroom room found!");
                errorSend = true;
                return;
            }
            if (__instance.favoriteSpot == null || favRoomPos == __instance.favoriteSpot.transform.position)
                return;

            Debug.Log("BrackenFavRoom: Changing the Brackens favorite spot");
            __instance.favoriteSpot.transform.position = favRoomPos;
            Debug.Log($"BrackenFavRoom: Changed Brackens favorite spot to X:{favRoomPos.x}, Y:{favRoomPos.y}, Z:{favRoomPos.z}");
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("thismicki.plugin.BrackenFavRoom");
            harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
