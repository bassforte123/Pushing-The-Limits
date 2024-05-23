using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using Dungeonator;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace PushingTheLimits
{
    [BepInDependency(Alexandria.Alexandria.GUID)] // this mod depends on the Alexandria API: https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency(Gunfiguration.C.MOD_GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class PushingTheLimitsModule : BaseUnityPlugin
    {
        public const string GUID = "bassforte.etg.pushingthelimits";
        public const string NAME = "Pushing The Limits";
        public const string VERSION = "1.0.0";
        public const string TEXT_COLOR = "#00FFFF";

        internal static float currMagnificence = 0;

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
            GunfigConfig.Init();
        }

        internal void Awake()
        {
            var harmony = new Harmony("bassforte.etg.rainbowrunlite");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GameStatsManager), nameof(GameStatsManager.IsRainbowRun), MethodType.Getter)]
        public class RainbowRunPatch
        {
            public static bool newRainbowCheck = false;

            //Set up alternate Rainbow Run flags so that individual Rainbow Run features can be enabled independantly.
            public static void Postfix(ref bool __result)
            {
                GameManager instanceLite = GameManager.Instance;

                if (!(instanceLite.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || instanceLite.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || instanceLite.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
                    && GameStatsManager.Instance.rainbowRunToggled)
                {
                    newRainbowCheck = (GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) != "Vanilla") && (GameManager.Instance.CurrentFloor == 1 || (GameManager.Instance.CurrentFloor >= 1 && GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) == "Rainbow Run Plus"));
                    __result = GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) == "Vanilla";
                }
                else
                {
                    newRainbowCheck = false;
                    __result = false;
                }
            }
        }
        

        [HarmonyPatch(typeof(Chest), nameof(Chest.SpewContentsOntoGround))]

        public class SpewContentsOntoGroundPatch
        {
                        //Only the first item from an initial Rainbow Run chest can be picked up.
                        static bool Prefix(Chest __instance, List<Transform> spawnTransforms)

                        {                           
                            if (__instance.IsRainbowChest && RainbowRunPatch.newRainbowCheck && __instance.transform.position.GetAbsoluteRoom() == GameManager.Instance.Dungeon.data.Entrance)
                            {
                                List<DebrisObject> list = new List<DebrisObject>();

                                for (int i = 0; i < __instance.contents.Count; i++)
                                {
                                    List<GameObject> list2 = new List<GameObject>();
                                    list2.Add(__instance.contents[i].gameObject);
                                    List<DebrisObject> list3 = LootEngine.SpewLoot(list2, spawnTransforms[i].position);
                                    list.AddRange(list3);
                                    for (int j = 0; j < list3.Count; j++)
                                    {
                                        if ((bool)list3[j])
                                        {
                                            list3[j].PreventFallingInPits = true;
                                        }
                                        if (!(list3[j].GetComponent<Gun>() != null) && !(list3[j].GetComponent<CurrencyPickup>() != null) && list3[j].specRigidbody != null)
                                        {
                                            list3[j].specRigidbody.CollideWithOthers = false;
                                            DebrisObject debrisObject = list3[j];
                                            debrisObject.OnTouchedGround = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnTouchedGround, new Action<DebrisObject>(__instance.BecomeViableItem));
                                        }
                                    }
                                }
                                GameManager.Instance.Dungeon.StartCoroutine(__instance.HandleRainbowRunLootProcessing(list));
                                return false;

                            }
                            else { return true; }
                        } 

        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.BecomeRainbowChest))]

        public class BecomeRainbowChestPatch
        {
            //Change Initial Rainbow Chest item drop rarity ratios.
            public static void Postfix(Chest __instance)
            {
                if (RainbowRunPatch.newRainbowCheck && __instance.transform.position.GetAbsoluteRoom() == GameManager.Instance.Dungeon.data.Entrance)
                {
                    __instance.lootTable.S_Chance = 0.2f;
                    __instance.lootTable.A_Chance = 0.7f;
                    __instance.lootTable.B_Chance = 0.4f;
                    __instance.lootTable.C_Chance = 0.2f;
                    __instance.lootTable.D_Chance = 0.2f;
                    if (GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) == "Rainbow Run Lite Lite") {
                        __instance.lootTable.S_Chance = 0.1f;
                        __instance.lootTable.A_Chance = 0.2f;
                        __instance.lootTable.C_Chance = 0.4f;
                        __instance.lootTable.D_Chance = 0.4f;
                    }
                    if (GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) == "Rainbow Run Lite Lite Lite")
                    {
                        __instance.lootTable.S_Chance = 0f;
                        __instance.lootTable.A_Chance = 0f;
                        __instance.lootTable.C_Chance = 0.6f;
                        __instance.lootTable.D_Chance = 0.6f;
                    }
                    __instance.lootTable.overrideItemQualities = new List<PickupObject.ItemQuality>();

                    //Rainbow Run chests force at least one A and S item.  This disables that function for the B and lower option
                    if (GunfigConfig._Gunfig.Value(GunfigConfig.RAINBOWRUN_LABEL) != "Rainbow Run Lite Lite Lite")
                    {
                        float value = UnityEngine.Random.value;
                        if (value < 0.5f)
                        {
                            __instance.lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.S);
                            __instance.lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.A);
                        }
                        else
                        {
                            __instance.lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.A);
                            __instance.lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.S);
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Dungeon), nameof(Dungeon.FloorReached))]
        private class FloorReachedPatch
        {
            //Spawns rainbow chests after the floor has been loaded
            static void Postfix()
            {
                bool rainbowCheck = GameStatsManager.Instance.IsRainbowRun; //run IsRainbowRun to ensure variables are up to date

                if (RainbowRunPatch.newRainbowCheck) SpawnInitialRainbowChest();
                if ((rainbowCheck || RainbowRunPatch.newRainbowCheck) && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GunfigConfig._Gunfig.Enabled(GunfigConfig.RAINBOWCOOP_LABEL)) SpawnInitialRainbowChest();
            }
        }

        //Spawn a Rainbow Chest
        static void SpawnInitialRainbowChest()
        {
                bool success;
                Chest chest = Chest.Spawn(GameManager.Instance.RewardManager.A_Chest, GameManager.Instance.Dungeon.data.Entrance.GetCenteredVisibleClearSpot(2, 2, out success));
                chest.m_isMimic = false;
                chest.IsRainbowChest = true;
                chest.BecomeRainbowChest();
        }


        //Synergy Chest success rate
        [HarmonyPatch(typeof(Chest), nameof(Chest.GenerateContents))]
        private class GenerateContentsPatch
        {
            [HarmonyILManipulator]
            private static void GenerateContentsIL(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);

                if (!cursor.TryGotoNext(MoveType.After,
              instr => instr.MatchCallOrCallvirt<UnityEngine.Mathf>("Clamp")))
                    return;
                cursor.Emit(OpCodes.Call, typeof(PushingTheLimitsModule).GetMethod("AssignSynergyChestValue"));
            }        
        }

        //Sets the Synergy Chest failure rate
        public static float AssignSynergyChestValue(float curr)
        {
            if (GunfigConfig._Gunfig.Value(GunfigConfig.SYNERGYCHEST_LABEL) == "Better Synergy Chests") return 0.25f;
            if (GunfigConfig._Gunfig.Value(GunfigConfig.SYNERGYCHEST_LABEL) == "Best Synergy Chests") return -1.0f;

            return curr;
        }

        //Make Synergy Chests have a normal chance of having a fuse
        [HarmonyPatch(typeof(Chest), nameof(Chest.RoomEntered))]
        private class RoomEnteredPatch
        {
            [HarmonyILManipulator]
            private static void GenerateContentsIL(ILContext il)
            {

                ILCursor cursor = new ILCursor(il);

                if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1)))
                    return;

                cursor.Emit(OpCodes.Ldloc_0); //load the regular fuse chance
                cursor.Emit(OpCodes.Call, typeof(PushingTheLimitsModule).GetMethod("SynergyFuseValue"));
                cursor.Index++; //skip over the storage instruction
                cursor.Emit(OpCodes.Pop); //drop the extra stack entry used here
            }
        }
        
        public static float SynergyFuseValue(float curr)
        {
            if (GunfigConfig._Gunfig.Enabled(GunfigConfig.SYNERGYFUSE_LABEL)) return curr; //return normal fuse chance
            return 1f; //return always have fuse
        }

        [HarmonyPatch(typeof(FloorRewardData), nameof(FloorRewardData.GetTargetQualityFromChances))]
        private class Magcheck
        {
            [HarmonyPostfix]

            //Sets the current Magnificence to a variable
            static void Postfix(FloorRewardData __instance)
            {
                currMagnificence = __instance.DetermineCurrentMagnificence();
            }
        }

        //Modifies how much each point of Magnificence increases your reroll chance
        [HarmonyPatch(typeof(MagnificenceConstants), nameof(MagnificenceConstants.ModifyQualityByMagnificence))]
        private class MagnificenceConstantsPatch
        {
            [HarmonyILManipulator]
            private static void ModifyQualityByMagnificenceIL(ILContext il)
            {

                ILCursor cursor = new ILCursor(il);

                if (!cursor.TryGotoNext(MoveType.After,
              instr => instr.MatchCallOrCallvirt<UnityEngine.Mathf>("Clamp01")))
                    return;

                cursor.Emit(OpCodes.Call, typeof(PushingTheLimitsModule).GetMethod("AssignMagnificenceValue"));
            }
        }

        public static float AssignMagnificenceValue(float curr)
        {
            if (GunfigConfig._Gunfig.Value(GunfigConfig.MAGNIFICENCE_LABEL) == "Vanilla")
            {
                return curr;
            }

            float newValue = -1;
            if (currMagnificence == 0f) newValue = 0f; 
            else
            {
                if (GunfigConfig._Gunfig.Value(GunfigConfig.MAGNIFICENCE_LABEL) == "Prettygoodicence")
                    newValue = Mathf.Clamp((0.2f + .2f * currMagnificence), 0f, 0.995f);
                if (GunfigConfig._Gunfig.Value(GunfigConfig.MAGNIFICENCE_LABEL) == "Okayicence")
                    newValue = Mathf.Clamp((0.15f + .15f * currMagnificence), 0f, 0.6f);
                if (GunfigConfig._Gunfig.Value(GunfigConfig.MAGNIFICENCE_LABEL) == "Mehicence") 
                    newValue = 0f;
            }
            return (1f - newValue);
        }

        //Change how much each synergy affects Synergy Factor
        [HarmonyPatch(typeof(SynergyFactorConstants), nameof(SynergyFactorConstants.GetSynergyFactor))]
        private class SynergyFactorPatch
        {
            [HarmonyILManipulator]
            private static void ModifyGetSynergyFactor(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);

                 if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloc(3)))
                    return;

                cursor.Emit(OpCodes.Call, typeof(PushingTheLimitsModule).GetMethod("MultSynergyFactor"));
            }
        }
        
        public static float MultSynergyFactor(float curr)
        {

            if (GunfigConfig._Gunfig.Value(GunfigConfig.SYNERGYFACTOR_LABEL) == "Vanilla") return curr;

            int synNum = GameStatsManager.Instance.GetNumberOfSynergiesEncounteredThisRun();
            float SFMult = 1;
            curr -= 1;

            if (synNum != 0) SFMult = 4; //Starting Synergy Factor is already high so only boost multiplier after first synergy
            if (GunfigConfig._Gunfig.Value(GunfigConfig.SYNERGYFACTOR_LABEL) == "Synergy Factor Synchrony") SFMult *= 2f;
            if (GunfigConfig._Gunfig.Value(GunfigConfig.SYNERGYFACTOR_LABEL) == "Synergy Factor Hatsune Miku") SFMult *= 5f;

            return (1 + curr * SFMult);
        }

        //Modify Boss DPS Cap
        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.Start))]
        private class HealthHaverStartPatch
        {
            [HarmonyPostfix]
            static void HealthHaverStartPostfix(HealthHaver __instance)
            {
                float capChange = 1;
                if (GunfigConfig._Gunfig.Value(GunfigConfig.CAP_LABEL) == "Vanilla") capChange = 1f;
                if (GunfigConfig._Gunfig.Value(GunfigConfig.CAP_LABEL) == "DPS Cap low") capChange = 1.25f;
                if (GunfigConfig._Gunfig.Value(GunfigConfig.CAP_LABEL) == "DPS Cap lower than that") capChange = 1.5f;
                if (GunfigConfig._Gunfig.Value(GunfigConfig.CAP_LABEL) == "DPS Cap even lower than that") capChange = 2f;
                if (GunfigConfig._Gunfig.Value(GunfigConfig.CAP_LABEL) == "DPS Cap just turn it off") capChange = 50f;

                GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
                if (__instance.IsBoss && !__instance.IsSubboss && lastLoadedLevelDefinition.bossDpsCap > 0f)
                {
                    float num = 1f;
                    if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                    {
                        num = (GameManager.Instance.COOP_ENEMY_HEALTH_MULTIPLIER + 2f) / 2f;
                    }
                    __instance.m_bossDpsCap = lastLoadedLevelDefinition.bossDpsCap * num * capChange;
                }
            }
        }


        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
