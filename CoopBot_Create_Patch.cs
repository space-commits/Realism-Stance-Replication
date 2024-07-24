using SPT.Reflection.Patching;
using Fika.Core.Coop.Players;
using System.Reflection;

namespace StanceReplication
{
    public class CoopBot_Create_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopBot).GetMethod("CreateBot");
        }

        [PatchPostfix]
        public static void Postfix(CoopBot __instance)
        {
            if (Plugin.EnableForBots.Value) 
            {
                __instance.gameObject.AddComponent<RSR_Component>();
            }
        }
    }
}
