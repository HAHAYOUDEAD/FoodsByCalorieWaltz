namespace CalorieWaltz
{
    [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
    public class AddCandleComponent
    {
        public static void Postfix(ref GearItem __instance)
        {
            if (__instance.gameObject.IsCandle())
            {
                
                if (__instance.gameObject.GetComponent<CollectibleCandleComponent>() == null)
                {
                    __instance.gameObject.AddComponent<CollectibleCandleComponent>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.InteractiveObjectsProcessInteraction))] // process left click interaction with candle
    public class CatchInteractionWithCandle
    {
        private static GameObject currentBear;

        [HarmonyPriority(Priority.LowerThanNormal)] // avoid skipinteraction mods, default is Priority.Normal 
        public static bool Prefix(PlayerManager __instance)
        {
            currentBear = Utility.GetGameObjectUnderCrosshair();

            if (currentBear != null && currentBear.IsCandle())
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
                    if (currentBear.name == "GEAR_candlePurpleD" && CWMain.bearInDPGO != null) // ignore lighing up DP bear
                    {
                        CWMain.ResetDPBear(false);
                        return true;
                    }

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
                    return true;
                }
            }
            // no item to interact with
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ProcessPickupItemInteraction))] // process pickup of collectible card
    public class AddBearWithCard
    {
        private static bool Prefix(ref GearItem item, ref bool forceEquip, ref bool skipAudio)
        {
            for (int i = 0; i < HarmonyValues.allCandleTiers.Count; i++)
            {
                if (item.name == "GEAR_card" + HarmonyValues.allCandleTiers[i])
                {
                    GameManager.GetInventoryComponent().AddGear(GearItem.InstantiateGearItem("GEAR_candle" + HarmonyValues.allCandleTiers[i])); // add corresponding candle // CHANGED

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

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UpdateInspectGear))] // catch card for later use
    public class GetCardWhenDiscarded
    {
        public static GearItem currentCard;

        private static bool Prefix(PlayerManager __instance)
        {

            if (!__instance.m_InspectModeActive) return false;

            GearItem inspectedGear = GameManager.GetPlayerManagerComponent().GearItemBeingInspected();

            if (inspectedGear == null) return true;

            // when put back pressed, catch card gearitem if not being previously inspected
            if (__instance.m_CanAddToInventory && !InterfaceManager.GetPanel<Panel_PauseMenu>().IsEnabled())
            {
                if (InputManager.GetPutBackPressed(__instance))
                {
                    if (inspectedGear.gameObject.IsCard())
                    {
                        if (!inspectedGear.m_BeenInspected)
                        {
                            currentCard = inspectedGear;
                            return true;
                        }
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ExitInspectGearMode))] // process card discarding
    public class SetDiscardedCardTransform
    {
        private static void MoveItemToPlayerFeetAndResetParent(GameObject go)
        {
            if (GearManager.m_GearCategory) go.transform.parent = GearManager.m_GearCategory.transform;
            Vector3 pos = GameManager.GetPlayerTransform().position;
            go.transform.position = pos;

        }

        private static void Postfix(PlayerManager __instance)
        {
            if (GetCardWhenDiscarded.currentCard != null)
            {
                MoveItemToPlayerFeetAndResetParent(GetCardWhenDiscarded.currentCard.gameObject);
                GetCardWhenDiscarded.currentCard = null;
            }


            // manage DP special spawn
            if (__instance?.m_Inspect)
            {
                if (__instance.m_Inspect.name == "GEAR_candlePurpleD" && CWMain.bearInDPGO != null)
                {
                    MoveItemToPlayerFeetAndResetParent(__instance.m_Inspect.gameObject);
                    CWMain.ResetDPBear(true);
                }
            }
        }
    }


    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.InteractiveObjectsProcessAltFire))] // disable interaction with DP bear except picking up
    public class DisablePlacingOfDPBear
    {
        public static bool Prefix(PlayerManager __instance)
        {
            GameObject currentBear = Utility.GetGameObjectUnderCrosshair();

            if (currentBear.name == "GEAR_candlePurpleD" && CWMain.bearInDPGO != null)
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.StartPlaceMesh), new Type[] { typeof(GameObject), typeof(float), typeof(PlaceMeshFlags), typeof(PlaceMeshRules) })] // play animation when placing candle
    public class PlaceCandleStart
    {
        public static void Postfix(PlayerManager __instance, ref GameObject objectToPlace, ref bool __result)
        {
            if (objectToPlace.IsCandle())
            {
                if (__result)
                {
                    objectToPlace.GetComponent<CollectibleCandleComponent>().isInPlacementMode = true;
                    MelonCoroutines.Start(CWAnim.ManageCandleHoldingAnimation(true, objectToPlace));
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ShouldSuppressCrosshairs))] // prevent interaction while custom animation is running
    public class ShorterName
    {
        public static void Postfix(ref bool __result)
        {

            if (CWAnim.CR_ManageCandleHoldingAnimation_IsRunning)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ExitMeshPlacement))] // stop animation after placing candle
    public class PlaceCandleEnd
    {
        public static void Prefix(PlayerManager __instance)
        {
            //MelonLogger.Msg(__instance.m_ObjectToPlace);
            //MelonLogger.Msg(__instance.m_ObjectToPlace.name);
            if (__instance.m_ObjectToPlace.IsCandle())
            {
                __instance.m_ObjectToPlace.GetComponent<CollectibleCandleComponent>().isInPlacementMode = false;
                MelonCoroutines.Start(CWAnim.ManageCandleHoldingAnimation(false, __instance.m_ObjectToPlace));
                HarmonyValues.animationCanEndNow = true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ItemInHandsDuringInteractionEndInternal))] // delay vanilla hands to allow custom animation to finish
    public class PlaceCandleEndPost
    {
        public static bool Prefix(PlayerManager __instance)
        {
            if (HarmonyValues.animationCanEndNow)
            {
                HarmonyValues.animationCanEndNow = false;
                MelonCoroutines.Start(CWAnim.DelayPlayerAnimatorAfterInteraction(__instance));

                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /* fix for new MC
    [HarmonyPatch(typeof(GearItem), nameof(GearItem.GetHoverText))] // show candle burnt percent
    internal class DisplayBurnPercentageLeft
    {
        private static GameObject currentBear;

        public static void Postfix(GearItem __instance, ref string __result)
        {
            if (__result is null || string.IsNullOrWhiteSpace(GameManager.m_ActiveScene)) return;

            currentBear = Utility.GetGameObjectUnderCrosshair();

            if (currentBear != null && currentBear.IsCandle())
            {
                if (currentBear.GetComponent<GearItem>()?.m_WornOut == true) return;

                float condition = currentBear.GetComponent<GearItem>().GetNormalizedCondition() * 100f;

                string str = CWMain.localizedCandleLeft100.Text();

                
                if (condition < 10f) str = Localization.Get("CWF_candleLeft7");//CWMain.localizedCandleLeft7.Text();
                else if (condition < 33f) str = Localization.Get("CWF_candleLeft25");//CWMain.localizedCandleLeft25.Text();
                else if (condition < 60f) str = Localization.Get("CWF_candleLeft50");//CWMain.localizedCandleLeft50.Text();
                else if (condition < 82f) str = Localization.Get("CWF_candleLeft75");//CWMain.localizedCandleLeft75.Text();
                else if (condition < 95f) str = Localization.Get("CWF_candleLeft90");//CWMain.localizedCandleLeft90.Text();

                __result += "\n" + str;
            }
        }
    }
    */
    [HarmonyPatch(typeof(Weather), nameof(Weather.IsTooDarkForAction))] // adjust distance to lit candle for being able to read
    public class DistanceToCandleForReading
    {
        public static void Postfix(ref bool __result)
        {
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
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(vp_Bullet), nameof(vp_Bullet.SpawnImpactEffects))] // process bullet hit to extinguish candle
    public class GetBulletHit
    {
        public static bool Prefix(ref RaycastHit hit)
        {

            if (hit.collider.gameObject.name == "flamePoint")
            {
                CollectibleCandleComponent comp = hit.collider.gameObject.transform.GetParent().GetParent().GetComponent<CollectibleCandleComponent>();

                if (comp)
                {
                    GearItem candleGI = comp.gameObject.GetComponent<GearItem>();
                    if (!Settings.options.noDamageOnShot)
                    {
                        candleGI.CurrentHP = Mathf.Clamp(candleGI.CurrentHP - candleGI.GearItemData.m_MaxHP * HarmonyValues.damageFromRifle / 100f, 0f, candleGI.GearItemData.m_MaxHP); // CHANGED
                        if (candleGI.CurrentHP < 1f)
                        {
                            candleGI.CurrentHP = 0f;
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

    [HarmonyPatch(typeof(ArrowItem), nameof(ArrowItem.HandleCollisionWithObject))] // process arrow hit to extinguish candle
    public class GetArrowHit
    {
        public static bool Prefix(ref GameObject collider)
        {
            if (collider.name == "flamePoint")
            {
                CollectibleCandleComponent comp = collider.transform.GetParent().GetParent().GetComponent<CollectibleCandleComponent>();
                if (comp)
                {
                    GearItem candleGI = comp.gameObject.GetComponent<GearItem>();
                    if (!Settings.options.noDamageOnShot)
                    {
                        candleGI.CurrentHP = Mathf.Clamp(candleGI.CurrentHP - candleGI.GearItemData.m_MaxHP * HarmonyValues.damageFromArrow / 100f, 0f, candleGI.GearItemData.m_MaxHP); // CHANGED
                        if (candleGI.CurrentHP < 1f)
                        {
                            candleGI.CurrentHP = 0f;
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
}
