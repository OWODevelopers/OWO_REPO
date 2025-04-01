using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OWO_REPO
{
    [BepInPlugin("org.bepinex.plugins.OWO_REPO", "OWO_REPO", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        #pragma warning disable CS0109
        internal static new ManualLogSource Log;
        #pragma warning restore CS0109

        public static OWOSkin owoSkin;

        private void Awake()
        {
            Log = Logger;
            Logger.LogMessage("OWO_REPO plugin is loaded!");
            owoSkin = new OWOSkin();

            var harmony = new Harmony("owo.patch.repo");
            harmony.PatchAll();
        }
    }
}
