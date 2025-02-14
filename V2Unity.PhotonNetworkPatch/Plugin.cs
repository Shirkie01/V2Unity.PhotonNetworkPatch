using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace V2Unity.PhotonNetworkPatch
{
    [BepInPlugin("V2Unity.PhotonNetworkPatch", "V2Unity.PhotonNetworkPatch", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static ConfigEntry<string> configUsername;

        public void Awake()
        {
            Log = Logger;            

            configUsername = Config.Bind("General", "Username", "PLAYER", "Username to use for multiplayer.");

            Harmony.CreateAndPatchAll(typeof(Login_LoginUser_Patch));
        }
    }
}
