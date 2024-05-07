using Aki.Reflection.Patching;
using Fika.Core.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StanceReplication
{
    public class ObservedCoopPlayer_Create_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ObservedCoopPlayer).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void Postfix(ObservedCoopPlayer __instance)
        {
            if (__instance.IsObservedAI && !Plugin.EnableForBots.Value) return;
            __instance.gameObject.AddComponent<RSR_Observed_Component>();
        }
    }
}
