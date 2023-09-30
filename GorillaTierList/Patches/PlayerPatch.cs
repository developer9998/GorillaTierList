using GorillaTierList.Behaviors;
using HarmonyLib;

namespace GorillaTierList.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    public class PlayerPatch
    {
        public static void Postfix(GorillaTagger __instance) => __instance.gameObject.AddComponent<Main>();
    }
}
