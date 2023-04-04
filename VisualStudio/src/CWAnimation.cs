namespace CalorieWaltz
{
    internal class CWAnim
    {
        public static bool CR_ManageCandleHoldingAnimation_IsRunning;

        public static void HideVanillaTool(bool hide)
        {
            FirstPersonWeapon leftHandItem = AA.objectToAppendTo.transform.GetParent().GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponLeftHand;
            FirstPersonWeapon rightHandItem = AA.objectToAppendTo.transform.GetParent().GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponRightHand;

            if (leftHandItem) leftHandItem.gameObject.active = !hide;
            if (rightHandItem) rightHandItem.gameObject.active = !hide;
        }

        public static IEnumerator ManageCandleHoldingAnimation(bool start, GameObject bear)
        {
            CR_ManageCandleHoldingAnimation_IsRunning = true;

            GameObject? platePoint = AA.toolRight.transform.Find("platePoint").gameObject;

            if (start)
            {
                while (GameManager.GetVpFPSCamera().WeaponSwitchInProgress()) // wait for vanilla animation end
                {
                    yield return new WaitForEndOfFrame();
                }
                MelonCoroutines.Start(AA.Activate());
                while (AA.CR_Activate_isRunning)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (AA.activationPrevented || !GameManager.GetPlayerManagerComponent().IsInPlacementMode())
                {
                    CR_ManageCandleHoldingAnimation_IsRunning = false;
                    AA.Done();
                    yield break;
                }
                AA.ActivateTool(AA.ToolPoint.RightHand, true);

                AA.SendTrigger(false, "bring");
                AA.SendTrigger(false, "putDown");

                AA.currentAnimator.IsLocked(true);

                // setup plate and bear
                GameObject instancedBear = UnityEngine.Object.Instantiate(bear);
                CollectibleCandleComponent instanceComp = instancedBear.GetComponent<CollectibleCandleComponent>();

                if (bear.GetComponent<CollectibleCandleComponent>().isLit)
                {
                    LayerMask LMincludeHands = (1 << 23); //8388608
                    LayerMask LMexcludeHands = ~LMincludeHands; //-8388609;

                    // prevent DecayOverTODHours calculation
                    instancedBear.GetComponent<GearItem>().GearItemData.m_DailyHPDecay = 0f; // CHANGED
                    instancedBear.GetComponent<GearItem>().CurrentHP = bear.GetComponent<GearItem>().CurrentHP; // CHANGED

                    instanceComp.FXpoint.gameObject.active = true;
                    instanceComp.isUsedForAnimation = true;

                    instanceComp.outdoorsLight.GetComponent<Light>().intensity = 1.25f;
                    instanceComp.outdoorsLight.GetComponent<Light>().range = 5f;
                    //instanceComp.outdoorsLight.GetComponent<Light>().cullingMask = LMexcludeHands;
                    LightTracking lt = instanceComp.outdoorsLight.GetComponent<LightTracking>();
                    lt.m_WasLightingWeaponCamera = false;
                    LightingManager.m_FpsLights.Add(lt);
                    instanceComp.selfLight.GetComponent<LightTracking>().enabled = false;
                    instanceComp.selfLight.GetComponent<Light>().range = 0.12f;
                    instanceComp.selfLight.GetComponent<Light>().cullingMask = LMincludeHands;

                    instanceComp.attenuationBase = 0.3f;

                    AA.currentAnimator.SetLayerWeight(1, 0f); // hold hand in front of candle
                }
                else
                {
                    AA.currentAnimator.SetLayerWeight(1, 1f); // use only right hand
                }

                instancedBear.transform.SetParent(platePoint.transform);
                instancedBear.transform.localPosition = Vector3.zero;
                instancedBear.transform.localEulerAngles = Vector3.zero;
                instancedBear.transform.localScale = Vector3.one * 0.25f;
                Utility.SetLayerRecursively2(instancedBear, 23); // weapon layer
                // setup camera
                if (AA.cameraPositioning)
                {
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_CenterAngle = 20f;
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_MaxAngle = 100f;
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_JointPositionAtStart = new Vector3(0f, -0.1431f, -0.026f);
                }
                else
                {
                    Utility.Log(2);
                }


                // disable any current tool
                HideVanillaTool(true);

                AA.SendTrigger(true, "bring");
                CR_ManageCandleHoldingAnimation_IsRunning = false;
                yield break;
            }
            else
            {
                CR_ManageCandleHoldingAnimation_IsRunning = true;
                AA.SendTrigger(true, "putDown");

                while (AA.currentAnimator.IsInState(new string[] { "put", "idle" } )) // wait for animation to end
                {
                    yield return new WaitForEndOfFrame();
                }

                if (GameManager.GetPlayerManagerComponent().IsInPlacementMode()) // abort if still in placement mode
                {
                    AA.SendTrigger(false, "bring");
                    AA.SendTrigger(false, "putDown");
                }

                platePoint.DestroyAllImmediateChildren();
                platePoint.transform.GetParent().gameObject.active = false;

                if (AA.cameraPositioning) // restore camera positioning
                {
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_CenterAngle = 0f;
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_MaxAngle = 90f;
                    AA.cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_JointPositionAtStart = new Vector3(0f, -0.1431f, 0f);
                }

                // restore previous tools
                HideVanillaTool(false);

                AA.Done();

                AA.currentAnimator.IsLocked(false);

                CR_ManageCandleHoldingAnimation_IsRunning = false;
                yield break;
            }
        }

        public static IEnumerator DelayPlayerAnimatorAfterInteraction(PlayerManager pm)
        {

            while (CR_ManageCandleHoldingAnimation_IsRunning) // wait for animation to end
            {
                yield return new WaitForEndOfFrame();
            }

            CR_ManageCandleHoldingAnimation_IsRunning = true; // for interaction suspention

            yield return new WaitForEndOfFrame(); // prevent infinite loop
            pm.DoDelayedInteractionEnd(InterfaceManager.GetPanel<Panel_HUD>()); // CHANGED
            //pm.ItemInHandsDuringInteractionEndInternal();

            CR_ManageCandleHoldingAnimation_IsRunning = false;
            yield break;
        }


    }
}
