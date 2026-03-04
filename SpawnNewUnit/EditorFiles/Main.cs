using HarmonyLib;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Logging;
using System;
using System.Reflection;

namespace SpawnNewUnit {
    public static class Main {
        internal static Harmony HarmonyInstance;
        internal static string Path;
        internal static IDisposable UnitSubscriber;
        internal static LogChannel Logger;  
        [OwlcatModificationEnterPoint]
        public static void EnterPoint(OwlcatModification modification) {
            Logger = modification.Logger;
            HarmonyInstance = new Harmony(modification.Manifest.UniqueName);
            SaveSpecificSettings.Initialize();
            Path = modification.Path;
            try {
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            } catch {
                HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
                throw;
            }
            UnitSubscriber = EventBus.Subscribe(new UnitSpawner());
        }
    }
}