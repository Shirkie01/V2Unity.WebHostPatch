using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace V2Unity.WebHostPatch
{
    [BepInPlugin("V2Unity.WebHostPatch", "WebHostPatch", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<string>? URLConfig;
        public static ManualLogSource Log { get; private set; } = new ManualLogSource("Default");

        public static string URL => URLConfig?.Value ?? "localhost";

        public void Awake()
        {
            // Set the logger so other classes can use it
            Log = Logger;

            string defaultURL = "https://v2unityapi20250224101209.azurewebsites.net/";

#if DEBUG
            defaultURL = "localhost";
#endif

            // Allow users to change the URL
            URLConfig = Config.Bind("Web Options", "URL", defaultURL);

            // Patch everything
            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly);
        }
    }
}
