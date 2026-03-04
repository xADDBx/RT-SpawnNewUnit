using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpawnNewUnit {
    public class GameActionHurtRT : GameAction {
        public override string GetCaption() => "Kill RT";

        protected override void RunAction() {
            var user = Game.Instance.Player.MainCharacterEntity;
            user.LifeState.MarkedForDeath = true;
            user.Wake(1f);
            user.Health.LastHandledDamage = null;
        }
    }
    // https://discord.com/channels/645948717400064030/791053285657542666/1478767896850333756
    [HarmonyPatch]
    public static class InteractionSpawner {
        #region Ignore
        private static FieldInfo? m_DynamicSceneInfo;
        private static FieldInfo? m_SceneNameInfo;
        private static MethodInfo? m_ElementsReferenceBaseSetCachedInfo;
        private static Dictionary<string, string> m_AreaToSceneName = new();
        private static string GetSceneNameForArea(string areaGuid) {
            if (!m_AreaToSceneName.TryGetValue(areaGuid, out var areaName)) {
                var bp = ResourcesLibrary.TryGetBlueprint(AffectedAreaGuid) as BlueprintArea;
                m_DynamicSceneInfo ??= typeof(BlueprintAreaPart).GetField("m_DynamicScene", BindingFlags.Instance | BindingFlags.NonPublic);
                m_SceneNameInfo ??= typeof(SceneReference).GetField("m_SceneName", BindingFlags.Instance | BindingFlags.NonPublic);
                areaName = m_SceneNameInfo.GetValue(m_DynamicSceneInfo.GetValue(bp)) as string;
                m_AreaToSceneName[areaGuid] = areaName;
            }
            return areaName;
        }
        [HarmonyPatch(typeof(SceneLoader), "LoadSceneCoroutine"), HarmonyPostfix]
        private static void SceneLoader_LoadSceneCoroutine_Patch(ref Task __result, SceneReference scene) {
            __result = __result.ContinueWith(t => {
                try {
                    if (scene.SceneName == GetSceneNameForArea(GetSceneNameForArea(AffectedAreaGuid))) {
                        AddButton();
                    }
                } catch (Exception ex) {
                    Main.Logger.Log(ex.ToString());
                }
            });
        }
        #endregion
        // Bridge Area
        public const string AffectedAreaGuid = "255859109cec4a042ade1613d80b25a4";
        public static readonly Vector3 SpawnLocation = new(-0.06896957f, 2.004852f, -13.23792f);
        public const string UniqueIdForButton = "UniqueSceneObjectButtonName";
        public static void AddButton() {
            #region Ignore
            var scene = SceneManager.GetSceneByName(GetSceneNameForArea(AffectedAreaGuid));
            if (scene.GetRootGameObjects().Any(obj => obj.name == UniqueIdForButton)) {
                return;
            }

            m_ElementsReferenceBaseSetCachedInfo ??= typeof(ElementsReferenceBase).GetMethod("set_Cached", BindingFlags.Instance | BindingFlags.NonPublic);

            var gameObj = new GameObject(UniqueIdForButton);
            SceneManager.MoveGameObjectToScene(gameObj, scene);
            gameObj.transform.position = SpawnLocation;
            var interaction = gameObj.AddComponent<InteractionAction>();
            // Important!
            gameObj.GetComponent<MapObjectView>().UniqueViewId = UniqueIdForButton;
            #endregion

            // Configure as you want.
            interaction.Settings = new() {
                NotInCombat = true,
                ShowOvertip = true,
                /* ProximityRadius = 1000000, */
                UIType = UIInteractionType.Action,
                Type = InteractionType.Approach,
                ShowHighlight = true,
                Condition = new(),
                Actions = new()
            };

            // You can also re-use existing ConditionsHolder or ActionsHolder via their guid like this:

            // interaction.Settings.Condition.ReadGuidFromJson("AssetGuidOfConditionsHolderHere");
            // interaction.Settings.Actions.ReadGuidFromJson("AssetGuidOfActionsHolderHere");
            
            // If you want to use new ConditionsHolder or ActionsHolder you can do this:
            
            // Empty ConditionsHolder evaluates to true
            var conditionToUse = new ConditionsHolder() {
                Conditions = new()
            };

            // ActionsHolder that only executes the GameActionHurtRT action; which is a custom action defined in line 18
            var actionToUse = new ActionsHolder() {
                Actions = new() {
                    Actions = new[] { new GameActionHurtRT() }
                }
            };



            m_ElementsReferenceBaseSetCachedInfo.Invoke(interaction.Settings.Condition, new[] { conditionToUse });
            m_ElementsReferenceBaseSetCachedInfo.Invoke(interaction.Settings.Actions, new[] { actionToUse });
        }
    }
}