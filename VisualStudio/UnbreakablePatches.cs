using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using UnityEngine.SceneManagement;
using ModComponent;
using System.IO;
using System.Reflection;

namespace CalorieWaltz
{
    public static class HarmonyValues
    {
        public static readonly int soupCommonVariantsCount = 12; // max 16 each 
        public static readonly int soupRareVariantsCount = 8;
        public static readonly int soupLegendaryVariantsCount = 6;

        public static readonly int soupRareChance = 12;
        public static readonly int soupLegendaryChance = 3;

        public static readonly int gearLayer = LayerMask.NameToLayer("Gear");

        public static LineRenderer line;


        public static GameObject vanillaRig;
        public static GameObject holdCandlePlatePoint;
        //public static Animator newAnimator;
        public static Animator customAnimator;
        public static Animator vanillaAnimator;
        public static AvatarMask leftHandMask;


        public static bool animationCanEndNow;

        public static bool initialAnimationSetup;

        public static readonly float distanceToRead = 1.5f; // about the distance at which you can read actual book
        public static readonly float damageFromRifle = 30f;
        public static readonly float damageFromArrow = 15f;




        public static readonly List<string> allCandleTiers = new List<string>
        {
            "GreenA","GreenB","GreenC","GreenD",
            "BlueA","BlueB","BlueC","BlueD",
            "PurpleA","PurpleB","PurpleC","PurpleD",
            "RedA","RedB","RedC","RedD"
        };
        //public static bool test;
    }



    [HarmonyPatch(typeof(PlayerAnimation), "Start")]
    public class PrepareCustomArms
    {
        private static GameObject vanillaRigChild;
        private static GameObject vanillaRigRightHandPropPoint;
        private static GameObject animationHolder;
        private static Animator newAnimator;

        public static void Postfix(PlayerAnimation __instance) 
        {
            bool isLoaded = HarmonyValues.vanillaRig && HarmonyValues.customAnimator && HarmonyValues.holdCandlePlatePoint;

            if (!isLoaded)
            {
                vanillaRigChild = GameManager.GetTopLevelCharacterFpsPlayer()?.transform?.Find("NEW_FPHand_Rig/GAME_DATA")?.gameObject;
                if (!vanillaRigChild) return;
                animationHolder = CWMain.CWFCustomBundle?.LoadAsset<GameObject>("holdCandleAnimation");
                if (!animationHolder) return;
                HarmonyValues.vanillaRig = vanillaRigChild.transform.GetParent().gameObject;
                vanillaRigRightHandPropPoint = vanillaRigChild.transform.FindChild("Origin/HipJoint/Chest_Joint/Camera_Weapon_Offset/Shoulder_Joint/Shoulder_Joint_Offset/Right_Shoulder_Joint_Offset/RightClavJoint/RightShoulderJoint/RightElbowJoint/RightWristJoint/RightPalm/right_prop_point")?.gameObject;
                if (!vanillaRigRightHandPropPoint) return;
                HarmonyValues.vanillaAnimator = HarmonyValues.vanillaRig.GetComponent<Animator>();
                newAnimator = animationHolder.GetComponent<Animator>();

                // clone data from embedded animator into newly created animator
                HarmonyValues.customAnimator = vanillaRigChild.AddComponent<Animator>();
                HarmonyValues.customAnimator.enabled = false;
                HarmonyValues.customAnimator.runtimeAnimatorController = newAnimator.runtimeAnimatorController;
                HarmonyValues.customAnimator.avatar = newAnimator.avatar;
                HarmonyValues.customAnimator.SetLayerWeight(1, 1f); // prevent animation popping

                // clone plate to right hand prop point and disable
                if (!HarmonyValues.holdCandlePlatePoint) HarmonyValues.holdCandlePlatePoint = UnityEngine.Object.Instantiate(animationHolder.transform.Find("plate")?.gameObject);
                if (HarmonyValues.holdCandlePlatePoint)
                {
                    HarmonyValues.holdCandlePlatePoint.layer = 23; // weapon
                    HarmonyValues.holdCandlePlatePoint.GetComponent<MeshRenderer>().material.shader = CWMain.vanillaShader;

                    HarmonyValues.holdCandlePlatePoint.transform.SetParent(vanillaRigRightHandPropPoint.transform);
                    HarmonyValues.holdCandlePlatePoint.transform.localPosition = Vector3.zero;
                    HarmonyValues.holdCandlePlatePoint.transform.localEulerAngles = Vector3.zero;
                    HarmonyValues.holdCandlePlatePoint.active = false;
                }
                HarmonyValues.holdCandlePlatePoint = HarmonyValues.holdCandlePlatePoint.transform.Find("platePoint")?.gameObject;
            }
        }
    }


    [HarmonyPatch] // ModComponent patch of gear spawns
    class ManageGearSpawnFiles
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("ModComponent.Mapper.ZipFileLoader");
            return AccessTools.FirstMethod(type, method => method.Name.Contains("TryHandleTxt"));
        }
        public static bool Prefix(string zipFilePath, string internalPath, ref string text, ref bool __result)
        {
            if (zipFilePath.EndsWith("FoodsByCalorieWaltz.modcomponent"))
            {
                string fileName = internalPath.Replace("gear-spawns/", "").Replace(".txt", "");

                if (Settings.options.disableCollectibleSpawns && fileName == "collectibles")
                {
                    MelonLogger.Msg(ConsoleColor.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableSpecialSpawns && fileName == "specials")
                {
                    MelonLogger.Msg(ConsoleColor.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableEasterEggSpawns && fileName == "funiBanana")
                {
                    MelonLogger.Msg(ConsoleColor.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableLore && fileName == "notes")
                {
                    MelonLogger.Msg(ConsoleColor.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }
            }

            return true;
        } 
    }

    [HarmonyPatch(typeof(MeshSwapItem), "Update")] // random texture for ABC soup
    public class ChangeSoupTexture
    {
        public static bool Prefix(MeshSwapItem __instance)
        {
            
            if (__instance.IsOpened() && __instance.gameObject.name == "GEAR_ABCSoup")
            {
                Material currentSoupMaterial = __instance.m_MeshObjOpened.GetComponent<MeshRenderer>().GetMaterialArray()[1];

                if (currentSoupMaterial.mainTexture.name == "bigMap")
                {
                    int rInt;

                    string set;

                    System.Random r = new System.Random(__instance.m_GearItem.m_InstanceID);

                    int setInt = r.Next(1, 100);

                    if (setInt <= HarmonyValues.soupLegendaryChance) 
                    {
                        set = "legendary";
                        rInt = r.Next(1, HarmonyValues.soupLegendaryVariantsCount);
                    }

                    else if (setInt > HarmonyValues.soupLegendaryChance && setInt <= HarmonyValues.soupRareChance + HarmonyValues.soupLegendaryChance)
                    {
                        set = "rare";
                        rInt = r.Next(1, HarmonyValues.soupRareVariantsCount);
                    }
                    else
                    {
                        set = "common";
                        rInt = r.Next(1, HarmonyValues.soupCommonVariantsCount);
                    }

                    currentSoupMaterial.shader = CWMain.vanillaLiquidMaterial.shader;
                    currentSoupMaterial.CopyPropertiesFromMaterial(CWMain.vanillaLiquidMaterial);
                    currentSoupMaterial.mainTexture = CWMain.GetRandomSoupTexture(set, rInt);
                    currentSoupMaterial.SetFloat("_Flow_Power", 0.2f);
                    currentSoupMaterial.name = set + rInt;

                    Material[] temp = __instance.m_MeshObjOpened.GetComponent<MeshRenderer>().GetMaterialArray();
                    temp[1] = currentSoupMaterial;

                    __instance.m_MeshObjOpened.GetComponent<MeshRenderer>().SetMaterialArray(temp);
                }
                
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GearItem), "Awake")]
    public class AddCandleComponent
    {
        public static void Postfix(ref GearItem __instance)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (__instance.m_GearName == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                {
                    if (__instance.gameObject.GetComponent<CollectibleCandleComponent>() == null)
                    {
                        __instance.gameObject.AddComponent<CollectibleCandleComponent>();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "ProcessPickupItemInteraction")] // process pickup of collectible card
    public class AddBearWithCard
    {
        private static bool Prefix(ref GearItem item, ref bool forceEquip, ref bool skipAudio)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (item.name == "GEAR_card" + HarmonyValues.allCandleTiers[i])
                {
                    GameManager.GetInventoryComponent().AddGear(Utils.InstantiateGearFromPrefabName("GEAR_candle" + HarmonyValues.allCandleTiers[i]).gameObject); // add corresponding candle

                    if (GameManager.GetInventoryComponent().GetNumGearWithName("GEAR_card" + HarmonyValues.allCandleTiers[i]) > 0) // discard duplicate cards
                    {
                        UnityEngine.Object.Destroy(item.gameObject);
                        return false;
                    }
                    return true;
                }
            }
            return true;

        }
    }

    [HarmonyPatch(typeof(PlayerManager), "UpdateInspectGear")] // catch card for later use
    public class GetCardWhenDiscarded
    {
        public static GearItem currentCard;

        private static bool Prefix(PlayerManager __instance)
        {
           
            if (!__instance.m_InspectModeActive) return false;

            GearItem inspectedGear = GameManager.GetPlayerManagerComponent().GearItemBeingInspected();

            if (inspectedGear == null) return true;

            // when put back pressed, catch card gearitem if not being previously inspected
            if (__instance.m_CanAddToInventory && !InterfaceManager.m_Panel_PauseMenu.IsEnabled())
            {
                if (InputManager.GetPutBackPressed(__instance))
                {
                    for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
                    {
                        if (inspectedGear.name == "GEAR_card" + HarmonyValues.allCandleTiers[i])
                        {
                            if (!inspectedGear.m_BeenInspected)
                            {
                                currentCard = inspectedGear;
                                return true;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }



    [HarmonyPatch(typeof(PlayerManager), "ExitInspectGearMode")] // process card discarding
    public class SetDiscardedCardTransform
    {
        private static void MoveItemToPlayerFeet(GearItem gi)
        {
            Vector3 pos = GameManager.GetPlayerTransform().position;
            gi.gameObject.transform.position = pos;
        }

        private static void Postfix(PlayerManager __instance)
        {
            if (GetCardWhenDiscarded.currentCard != null)
            {
                MoveItemToPlayerFeet(GetCardWhenDiscarded.currentCard);
                GetCardWhenDiscarded.currentCard = null;
            }

            
            if (__instance?.m_Gear)
            {
                if (__instance.m_Gear.name == "GEAR_candlePurpleD")
                {
                    CWMain.bearInDPCurrentlyInspecting = false;
                }
            }
        }
    }



    [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")] // process left click interaction with candle
    public class CatchInteractionWithCandle
    {
        private static GameObject currentBear;
        private static GameObject candleFlame;

        [HarmonyPriority(Priority.LowerThanNormal)] // avoid skipinteraction mods, default is Priority.Normal 
        public static bool Prefix(PlayerManager __instance)
        {
            currentBear = __instance.m_InteractiveObjectUnderCrosshair;

            if (currentBear != null)
            {
                for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
                {
                    if (currentBear.name == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                    {
                        CollectibleCandleComponent comp = currentBear.GetComponent<CollectibleCandleComponent>();

                        if (!comp) return true;

                        // skip if burned out
                        if (currentBear.GetComponent<GearItem>().m_WornOut)
                        {
                            return true;
                        }
                        // chill until animation ends
                        if (comp.ienumeratorWorking)
                        {
                            return false;
                        }
                        // extinguish
                        if (comp.isLit)
                        {
                            if (__instance.m_ItemInHands != null)
                            {
                                if (__instance.m_ItemInHands.m_TorchItem && !__instance.m_ItemInHands.IsLitTorch())
                                {
                                    GameManager.GetPlayerManagerComponent().m_ItemInHands.m_TorchItem.IgniteDelayed(3f, "", true);
                                    return false;
                                }
                            }

                            comp.isLit = false;
                            comp.lastExtinguishReason = ExtinguishType.BlowOut;
                            return false;
                        }
                        else
                        {
                            // light up
                            if (__instance.m_ItemInHands != null)
                            {
                                if (__instance.m_ItemInHands.IsLitMatch() || __instance.m_ItemInHands.IsLitFlare() || __instance.m_ItemInHands.IsLitTorch())
                                {
                                    comp.isLit = true;
                                    currentBear.GetComponent<GearItem>().m_BeenInPlayerInventory = true;
                                    return false;
                                }
                            }
                            // pick up
                            if (currentBear.name == "GEAR_candlePurpleD")
                            {
                                CWMain.bearInDPCurrentlyInspecting = true;
                            }
                            return true;
                        }
                    }
                }
            }
            // no item to interact with
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectDisplayText")] // show candle burnt percent
    internal class DisplayBurnPercentageLeft
    {
        private static GameObject currentBear;

        public static void Postfix(PlayerManager __instance, ref string __result)
        {
            if (__result is null || string.IsNullOrWhiteSpace(GameManager.m_ActiveScene)) return;

            currentBear = __instance?.m_InteractiveObjectUnderCrosshair;

            if (currentBear != null)
            {
                if (currentBear.GetComponent<GearItem>()?.m_WornOut == true) return;

                for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
                {
                    if (currentBear.name == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                    {
                        float condition = currentBear.GetComponent<GearItem>().GetNormalizedCondition() * 100f;

                        /*
                        float percentLeft = 100f;
                        string mark = " ~";

                        if (condition < 5f)
                        {
                            mark = " <";
                            percentLeft = 5f;
                        }
                        else if (condition < 15f) percentLeft = 15f;
                        else if (condition < 30f) percentLeft = 30f;
                        else if (condition < 50f) percentLeft = 50f;
                        else if (condition < 75f) percentLeft = 75f;
                        else if (condition < 90f) percentLeft = 90f;

                        __result += "\n" + CWMain.localizedBurntimeLeft.Text() + mark + percentLeft + CWMain.localizedBurntimeUnit.Text();
                        */
                        string str = CWMain.localizedCandleLeft100.Text();

                        if (condition < 10f) str = CWMain.localizedCandleLeft7.Text();
                        else if (condition < 33f) str = CWMain.localizedCandleLeft25.Text();
                        else if (condition < 60f) str = CWMain.localizedCandleLeft50.Text();
                        else if (condition < 82f) str = CWMain.localizedCandleLeft75.Text();
                        else if (condition < 95f) str = CWMain.localizedCandleLeft90.Text();


                        __result += "\n" + str;

                    }
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(GearItem), "IsLitMatch")] // allow reading under candle light
    public class CandleIsMatch
    {
        public static void Postfix(ref GearItem __instance, ref bool __result)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (__instance.name == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                {
                    CollectibleCandleComponent thisCandle = __instance.gameObject.GetComponent<CollectibleCandleComponent>();

                    if (thisCandle)
                    {
                        __result = thisCandle.isLit;
                    }
                }
            }
        }
    }
    */

    /*

    [HarmonyPatch(typeof(Weather), "IsTooDarkForAction")] // adjust distance to lit candle for being able to read
    public class DistanceToCandleForReading
    {
        public static void Postfix(ref ActionsToBlock actionBeingChecked, ref bool __result)
        {
            if (!GameManager.GetPlayerManagerComponent().m_ActionsToBlockInDarkness.Contains(actionBeingChecked))
            {
                __result = false;
            }
    
            for (int i = 0; i < GearManager.m_Gear.Count; i++)
            {
                CollectibleCandleComponent comp = GearManager.m_Gear[i].GetComponent<CollectibleCandleComponent>();
                if (comp && comp.isLit)
                {
                    //MelonLogger.Msg("Found candle, distance: " + Vector3.Distance(GearManager.m_Gear[i].transform.position, GameManager.GetVpFPSCamera().transform.position));
                    if (Vector3.Distance(GearManager.m_Gear[i].transform.position, GameManager.GetVpFPSCamera().transform.position) <= HarmonyValues.distanceToRead)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
    */

    [HarmonyPatch(typeof(Weather), "IsTooDarkForAction")] // adjust distance to lit candle for being able to read
    public class DistanceToCandleForReading
    {
        public static void Postfix(ref ActionsToBlock actionBeingChecked, ref bool __result)
        {
            if (!GameManager.GetPlayerManagerComponent().m_ActionsToBlockInDarkness.Contains(actionBeingChecked))
            {
                __result = false;
            }

            if (__result == true)
            {
                foreach (GameObject go in CWMain.currentCandleList)
                {
                    if (!go) continue;
                    CollectibleCandleComponent comp = go.GetComponent<CollectibleCandleComponent>();
                    GearItem gi = go.GetComponent<GearItem>();
                    if (gi && comp && comp.isLit && !gi.m_InPlayerInventory && !gi.m_WornOut)
                    {
                        if (Vector3.Distance(go.transform.position, GameManager.GetVpFPSCamera().transform.position) <= HarmonyValues.distanceToRead)
                        {
                            //MelonLogger.Msg("distance to closest " + Vector3.Distance(go.transform.position, GameManager.GetVpFPSCamera().transform.position));
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(vp_Bullet), "SpawnImpactEffects")] // process bullet hit to extinguish candle
    public class GetBulletHit
    {
        public static bool Prefix(ref RaycastHit hit)
        {
            /*
            // DEBUG
            HUDMessage.AddMessage(hit.collider.gameObject.name, true, true);

            HarmonyValues.line.SetPosition(0, Utils.GetPlayerEyePosition());
            HarmonyValues.line.SetPosition(1, hit.point);
            // DEBUG
            */


            //MelonLogger.Msg(hit.collider.gameObject.name);

            if (hit.collider.gameObject.name == "flamePoint")
            {
                CollectibleCandleComponent comp = hit.collider.gameObject.transform.GetParent().GetParent().GetComponent<CollectibleCandleComponent>();
                
                if (comp)
                {
                    GearItem candleGI = comp.gameObject.GetComponent<GearItem>();
                    if (!Settings.options.noDamageOnShot)
                    {
                        candleGI.m_CurrentHP = Mathf.Clamp(candleGI.m_CurrentHP - candleGI.m_MaxHP * HarmonyValues.damageFromRifle / 100f, 0f, candleGI.m_MaxHP);
                        if (candleGI.m_CurrentHP < 1f)
                        {
                            candleGI.m_CurrentHP = 0f;
                            candleGI.m_WornOut = true;
                        }
                    }
                    if (comp.isLit && !candleGI.m_WornOut)
                    {
                        comp.isLit = false;
                        comp.lastExtinguishReason = ExtinguishType.Instant;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ArrowItem), "HandleCollisionWithObject")] // process arrow hit to extinguish candle
    public class GetArrowHit
    {
        public static bool Prefix(ref GameObject collider)
        {
            /*
            // DEBUG
            HUDMessage.AddMessage(collider.name, true, true);
            
            HarmonyValues.line.SetPosition(0, Utils.GetPlayerEyePosition());
            HarmonyValues.line.SetPosition(1, collider.transform.position);
            // DEBUG
            */

            if (collider.name == "flamePoint")
            {
                CollectibleCandleComponent comp = collider.transform.GetParent().GetParent().GetComponent<CollectibleCandleComponent>();
                if (comp)
                {
                    GearItem candleGI = comp.gameObject.GetComponent<GearItem>();
                    if (!Settings.options.noDamageOnShot)
                    {
                        candleGI.m_CurrentHP = Mathf.Clamp(candleGI.m_CurrentHP - candleGI.m_MaxHP * HarmonyValues.damageFromArrow / 100f, 0f, candleGI.m_MaxHP);
                        if (candleGI.m_CurrentHP < 1f)
                        {
                            candleGI.m_CurrentHP = 0f;
                            candleGI.m_WornOut = true;
                        }
                    }
                    
                    if (comp.isLit && !candleGI.m_WornOut)
                    {
                        comp.isLit = false;
                        comp.lastExtinguishReason = ExtinguishType.Instant;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "StartPlaceMesh", new Type[] { typeof(GameObject), typeof(float), typeof(PlaceMeshFlags) })] // play animation when placing candle
    public class PlaceCandleStart
    {
        public static void Postfix(PlayerManager __instance, ref GameObject objectToPlace, ref bool __result)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (objectToPlace.name == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                {
                    if (__result)
                    {
                        objectToPlace.GetComponent<CollectibleCandleComponent>().isInPlacementMode = true;
                        MelonCoroutines.Start(CWMain.ManageCandleHoldingAnimation(true, objectToPlace));
                        
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")] // stop animation after placing candle
    public class PlaceCandleEnd
    {
        public static void Prefix(PlayerManager __instance)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (__instance.m_ObjectToPlace.name == "GEAR_candle" + HarmonyValues.allCandleTiers[i])
                {
                    __instance.m_ObjectToPlace.GetComponent<CollectibleCandleComponent>().isInPlacementMode = false;
                    MelonCoroutines.Start(CWMain.ManageCandleHoldingAnimation(false, __instance.m_ObjectToPlace));
                    HarmonyValues.animationCanEndNow = true;
                }
            }
        }
    }


    [HarmonyPatch(typeof(PlayerManager), "ItemInHandsDuringInteractionEndInternal")] // delay vanilla hands to allow custom animation to finish
    public class PlaceCandleEndPost
    {
        public static bool Prefix(PlayerManager __instance)
        {
            if (HarmonyValues.animationCanEndNow)
            {
                HarmonyValues.animationCanEndNow = false;
                MelonCoroutines.Start(CWMain.DelayPlayerAnimatorAfterInteraction(__instance));
                
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "ShouldSuppressCrosshairs")] // prevent interaction while custom animation is running
    public class ShorterName
    {
        public static void Postfix(ref bool __result)
        {
            
            if (CWMain.holdingAnimationCoroutineIsRunning)
            {
                __result = true;
            }
        }
    }
}
