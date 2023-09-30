using BepInEx;
using System.Reflection;
using HarmonyLib;

namespace GorillaTierList
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony(PluginInfo.GUID).PatchAll(assembly);
        }
    }
}
