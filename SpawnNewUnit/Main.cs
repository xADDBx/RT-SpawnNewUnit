using HarmonyLib;
using Kingmaker.PubSubSystem.Core;
using System.Reflection;
using UnityModManagerNet;

namespace SpawnNewUnit;

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;
    internal static UnityModManager.ModEntry ModEntry;
    internal static IDisposable Subscriber;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        Log = modEntry.Logger;
        ModEntry = modEntry;
        Subscriber = EventBus.Subscribe(new UnitSpawner());
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        try {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        } catch {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        return true;
    }
}
