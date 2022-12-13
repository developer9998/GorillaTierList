using HarmonyLib;
using GorillaLocomotion;
using System.Collections;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTierList.Patches
{
    [HarmonyPatch(typeof(CosmeticsController), "GetUserCosmeticsAllowed")]
    internal class GetCosmeticPatch
    {
        internal static void Postfix(CosmeticsController __instance) => __instance.StartCoroutine(Delay());

        internal static IEnumerator Delay()
        {
            yield return 0;
            yield return new WaitForSeconds(2);

            Plugin.Instance.OnInitialized();
        }
    }

    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerAwakePatch
    {
        internal static void Postfix(Player __instance) => __instance.StartCoroutine(Delay());

        internal static IEnumerator Delay()
        {
            yield return 0;

            Plugin.Instance.OnInitialized();
        }
    }
}
