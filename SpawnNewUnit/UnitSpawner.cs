using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem.Core;
using SpawnNewUnit;
using System;
using UnityEngine;

namespace SpawnNewUnit {
    public class UnitSpawner : IAreaActivationHandler {
        // Bridge Area
        public const string AffectedAreaGuid = "255859109cec4a042ade1613d80b25a4";
        // Pascal Unit
        public const string UnitGuid = "e1cfcddc1dc447278762a0725753c394";
        public static readonly Vector3 SpawnLocation = new(0.18f, 0.02f, -16.43f);
        public void OnAreaActivated() {
            if (Game.Instance.CurrentlyLoadedArea.AssetGuid == AffectedAreaGuid) {
                // Can be done via an Etude also; just make a check that ensures the current save has not already spawned the unit
                if (!SaveSpecificSettings.Instance.WasSpawned) {
                    Game.Instance.EntitySpawner.SpawnUnit(ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(UnitGuid),
                        SpawnLocation, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                    SaveSpecificSettings.Instance.WasSpawned = true;
                    SaveSpecificSettings.Instance.Save();
                }
            }
        }
    }
}