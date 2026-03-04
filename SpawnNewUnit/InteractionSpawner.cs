using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.PC;
using Kingmaker.Code.UI.MVVM.View.Surface.PC;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Interaction;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpawnNewUnit {
    public class GameActionKill : GameAction {
        public override string GetCaption() => "Kill RT";

        public override void RunAction() {
            var user = Game.Instance.Player.MainCharacterEntity;
            user.LifeState.MarkedForDeath = true;
            user.Wake(1f);
            user.Health.LastHandledDamage = null;
        }
    }
    [HarmonyPatch]
    public static class InteractionSpawner {
        [HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.LoadSceneCoroutine)), HarmonyPostfix]
        private static void SceneLoader_LoadSceneCoroutine_Patch(Task __result, SceneReference scene) {
            __result.ContinueWith(t => {
                if (scene.SceneName == AffectedSceneName.Value) {
                    AddButton();
                }
            });
        }
        // Bridge Area
        public const string AffectedAreaGuid = "255859109cec4a042ade1613d80b25a4";
        public static Lazy<string> AffectedSceneName = new(() => (ResourcesLibrary.TryGetBlueprint(AffectedAreaGuid) as BlueprintArea).m_DynamicScene.m_SceneName);
        public static readonly Vector3 SpawnLocation = new(-0.06896957f, 2.004852f, -13.23792f);
        public const string ButtonId = "UniqueSceneObjectButtonName";
        public static void AddButton() {
            try {
                var scene = SceneManager.GetSceneByName(AffectedSceneName.Value);
                if (scene.GetRootGameObjects().Any(obj => obj.name == ButtonId)) {
                    return;
                }
                var obj = new GameObject(ButtonId);
                SceneManager.MoveGameObjectToScene(obj, scene);
                obj.transform.position = SpawnLocation;
                var b = obj.AddComponent<InteractionAction>();
                b.Settings = new() {
                    NotInCombat = true,
                    AlwaysDisabled = false,
                    ShowOvertip = true,
                    UIType = UIInteractionType.Action,
                    ProximityRadius = 100000,
                    Type = InteractionType.Approach,
                    ShowHighlight = true,
                    Condition = new() {
                        Cached = new ConditionsHolder() {
                            Conditions = new()
                        }
                    },
                    Actions = new() {
                        Cached = new ActionsHolder() {
                            Actions = new() {
                                Actions = [new GameActionKill()]
                            }
                        }
                    }

                };
            } catch (Exception ex) {
                Main.Log.Log(ex.ToString());
            }
        }
    }
}