using GorillaNetworking;
using GorillaTierList.Behaviors;
using HarmonyLib;

namespace GorillaTierList.Patches
{
    [HarmonyPatch(typeof(CosmeticsController), "GetUserCosmeticsAllowed")]
    public class CosmeticsPatch
    {
        public static void Postfix() => Main.Instance.OccasionalUpdate();
    }
}
