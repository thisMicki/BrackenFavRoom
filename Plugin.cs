using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BrackenFavRoom
{
    [HarmonyPatch(typeof(FlowermanAI))]
    [HarmonyPatch("DoAIInterval")]
    class FlowermanAIPatch
    {
        //static bool errorSend = false;
        static GameObject SmallRoom;
        static Vector3 favRoomPos = Vector3.zero;

        // [HarmonyPatch("Start")]
        // static void Postfix(){
        //     SmallRoom = GameObject.Find("SmallRoom2(Clone)");
        // }



        static void Postfix(FlowermanAI __instance)
        {
            GameObject SmallRoom = GameObject.Find("SmallRoom2(Clone)"); ;
            Vector3 favRoomPos;

            if (SmallRoom == null)
            {
                //if(!errorSend)
                Debug.LogError("BrackenFavRoom: No Backroom room found!");
                return;
                
            }
            favRoomPos = SmallRoom.transform.position;

            if (__instance.favoriteSpot == null || favRoomPos == __instance.favoriteSpot.transform.position)
            {
                return;
            }
            __instance.favoriteSpot.SetPositionAndRotation(favRoomPos, Quaternion.Euler(Vector3.zero));
            Debug.Log($"BrackenFavRoom: Changed Brackens favorite spot to X:{favRoomPos.x}, Y:{favRoomPos.y}, Z:{favRoomPos.z}");
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInIncompatibility("readthisifbad-SnatchinBracken-1.2.8")]
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
