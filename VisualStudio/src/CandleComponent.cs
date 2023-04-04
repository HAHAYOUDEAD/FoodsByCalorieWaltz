namespace CalorieWaltz
{
    [RegisterTypeInIl2Cpp]
    public class CollectibleCandleComponent : MonoBehaviour
    {
        public CollectibleCandleComponent(IntPtr intPtr) : base(intPtr) { }

        public GearItem thisGearItem;
        public Vector3 blowDirection;

        // stored values
        //public GameObject flameFX;

        public Transform FXpoint;
        public GameObject lights;
        public GameObject flame;
        public GameObject smoke;

        public GameObject indoorsLight;
        public GameObject outdoorsLight;
        public GameObject selfLight;

        public GameObject pickupHelper;

        //public readonly float indoorsLightIntensity = 1.2f;
        public readonly float outdoorsLightIntensity = 1.5f;

        public bool thisLoaded;

        public bool isLit;

        public bool isInPlacementMode;
        public bool isUsedForAnimation;
        public bool isWindy;
        public bool destroyed = false;
        public bool ienumeratorWorking;

        public float attenuation; // "SSS" intensity
        public float attenuationBase = 0.55f; // 0.5-0.8 is alright

        public readonly float burnTimeHours = 8f;

        public float curveMult; // flame particles force curve multiplier

        public ExtinguishType lastExtinguishReason = ExtinguishType.Instant;


        public float debugTimer = 0f;

        AnimationCurve xCurve = new AnimationCurve();
        AnimationCurve zCurve = new AnimationCurve();

        //ParticleSystem.MinMaxCurve xFinalCurve = new ParticleSystem.MinMaxCurve();
        //ParticleSystem.MinMaxCurve zFinalCurve = new ParticleSystem.MinMaxCurve();
        //ParticleSystem.MinMaxCurve zeroCurve = new ParticleSystem.MinMaxCurve();

        public void Awake()
        {
            thisGearItem = this.gameObject.GetComponent<GearItem>();

            if (!thisGearItem.gameObject.transform.FindChild("wick")) // for when instanced to be held
            {
                destroyed = true;
                return;
            }

            //MelonLogger.Msg(Utility.GetGameObjectPath(this.gameObject));
            CWMain.currentCandleList.Add(this.gameObject);

            FXpoint = thisGearItem.gameObject.transform.FindChild("wick").GetChild(0);
            
            flame = FXpoint.Find("Particles/Fire").gameObject;
            smoke = FXpoint.Find("Particles/Smoke").gameObject;

            lights = FXpoint.Find("Lights").gameObject;

            indoorsLight = lights.transform.Find("Indoors").gameObject;
            outdoorsLight = lights.transform.Find("Outdoors").gameObject;
            selfLight = lights.transform.Find("Self").gameObject;

            outdoorsLight.transform.rotation = Quaternion.identity;

            //zeroCurve.mode = ParticleSystemCurveMode.TwoCurves;

            pickupHelper = thisGearItem.transform.FindChild("pickupHelper")?.gameObject;

        }

        private void SetSSSAttenuation(float value)
        {
            if (value == 0f)
            {
                thisGearItem.GetComponent<MeshRenderer>().material.SetColor("_EmissiveTint", Color.black); // main
                thisGearItem.GetComponent<MeshRenderer>().material.SetFloat("_EmissiveStrength", 0f);
                thisGearItem.transform.FindChild("wick").gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissiveTint", Color.black); // wick
                return;
            }

            Color color = new Color(0.9f, 0.5f, 0.1f);
            thisGearItem.GetComponent<MeshRenderer>().material.SetColor("_EmissiveTint", color * value);
            thisGearItem.GetComponent<MeshRenderer>().material.SetFloat("_EmissiveStrength", 1.2f);
            thisGearItem.transform.FindChild("wick").gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissiveTint", color * value); // wick
        }

        public void ToggleLight(bool on = false, bool suppressSmoke = false)
        {
            lights.SetActive(on);

            if (on)
            {
                flame.GetComponent<ParticleSystem>().Play();

                thisGearItem.GearItemData.m_DailyHPDecay = (24f / burnTimeHours) * thisGearItem.GearItemData.m_MaxHP * (1f / GameManager.GetExperienceModeManagerComponent().GetDecayScale()); // * m_DailyHPDecay scales based on difficulty
                thisGearItem.m_BeenInspected = false; // CHANGED

            }

            else
            {
                flame.GetComponent<ParticleSystem>().Stop();
                if (!suppressSmoke)
                {
                    smoke.GetComponent<ParticleSystem>().Play();
                    GameAudioManager.PlaySound("Play_MatchBurnOut", thisGearItem.gameObject);
                }

                curveMult = 0f;
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.x = zeroCurve;
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.y = zeroCurve;
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.z = zeroCurve;
                outdoorsLight.GetComponent<Light>().intensity = outdoorsLightIntensity;

                attenuation = 0f;
                SetSSSAttenuation(attenuation);

                thisGearItem.GearItemData.m_DailyHPDecay = 0f;
                thisGearItem.m_BeenInspected = true;
            }
        }

        [HideFromIl2Cpp]
        IEnumerator AnimateFlameInWind()
        {
            bool resetVelocity = false;
            float randomStepMin = 1f;
            float randomStepMax = 1f;
            int stepsPerRandom = 3;
            int currentStep = 0;
            float delay = 0.5f;
            float randomStepLerp = 0;

            while (isLit)
            {
                if (ienumeratorWorking) yield return null;

                if (isWindy)
                {
                    if (currentStep >= stepsPerRandom)
                    {
                        randomStepMin = UnityEngine.Random.Range(1f, 3f);
                        stepsPerRandom = UnityEngine.Random.Range(2, 7);
                        currentStep = 0;
                        randomStepLerp = 0f;
                    }

                    if (resetVelocity) resetVelocity = false;

                    blowDirection = Quaternion.Inverse(thisGearItem.transform.rotation) * GameManager.GetWindComponent().m_CurrentDirection.normalized;
                    curveMult = GameManager.GetWindComponent().m_CurrentMPH * Mathf.SmoothStep(randomStepMin, randomStepMax, randomStepLerp += 1f / stepsPerRandom);

                    // setup force curve
                    xCurve.MoveKey(1, new Keyframe(0.5f, blowDirection.x / 8f));
                    zCurve.MoveKey(1, new Keyframe(0.5f, blowDirection.z / 8f));

                    xCurve.MoveKey(2, new Keyframe(1.0f, blowDirection.x));
                    zCurve.MoveKey(2, new Keyframe(1.0f, blowDirection.z));

                    //xFinalCurve.m_CurveMultiplier = curveMult;
                    //xFinalCurve.m_CurveMin = xFinalCurve.m_CurveMax = xCurve;

                    //zFinalCurve.m_CurveMultiplier = curveMult;
                    //zFinalCurve.m_CurveMin = zFinalCurve.m_CurveMax = zCurve;

                    // apply force to particles
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.x = xFinalCurve;
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.y = zeroCurve;
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.z = zFinalCurve;

                    currentStep += 1;
                }

                else if(!resetVelocity)
                {
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.x = zeroCurve;
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.y = zeroCurve;
                    //flame.GetComponent<ParticleSystem>().velocityOverLifetime.z = zeroCurve;
                    resetVelocity = true;

                    currentStep = 0;
                }
                yield return new WaitForSeconds(delay);
            }
            yield break;
        }

        [HideFromIl2Cpp]
        IEnumerator ExtinguishAnimated(ExtinguishType type = ExtinguishType.Instant)
        {
            if (type == ExtinguishType.Instant)
            {
                ToggleLight(false);
                yield break;
            }

            if (type == ExtinguishType.NoAnimation)
            {
                ToggleLight(false, true);
                yield break;
            }

            if (type == ExtinguishType.DestroyWick)
            {
                ToggleLight(false);
                yield return new WaitForSeconds(0.75f);
                Destroy(thisGearItem.gameObject.transform.FindChild("wick").gameObject); // destroy wick
                yield break;
            }

            float t = 0f;
            float tMult;
            //float windMult = 1f;
            float animationTime = 0.5f; // in seconds
            //float lightInitialIntensity = outdoorsLight.GetComponent<Light>().intensity;

            ienumeratorWorking = true;

            bool stopped = false;

            float fadeoutMult = 1f;

            xCurve.MoveKey(2, new Keyframe(1.0f, 0.25f));
            zCurve.MoveKey(2, new Keyframe(1.0f, 0.25f));

            while (t < animationTime)
            {
                t += Time.deltaTime;

                if (type == ExtinguishType.BlowOut)
                {
                    blowDirection = Quaternion.Inverse(thisGearItem.transform.rotation) * GameManager.GetPlayerTransform().forward.normalized;

                    tMult = t * 10f / animationTime;
                    curveMult = 2f * (1.5f * tMult + tMult * Mathf.Sin(tMult));

                }
                else if (type == ExtinguishType.Wind)
                {
                    blowDirection = Quaternion.Inverse(thisGearItem.transform.rotation) * GameManager.GetWindComponent().m_CurrentDirection.normalized;
                    curveMult = Mathf.Lerp(0f, Mathf.Clamp(GameManager.GetWindComponent().m_CurrentMPH * 2f, 1f, 40f), t / animationTime);

                    xCurve.MoveKey(2, new Keyframe(1.0f, blowDirection.x));
                    zCurve.MoveKey(2, new Keyframe(1.0f, blowDirection.z));
                }

                // setup force curve
                xCurve.MoveKey(1, new Keyframe(0.25f, blowDirection.x));
                zCurve.MoveKey(1, new Keyframe(0.25f, blowDirection.z));

                //xFinalCurve.m_CurveMultiplier = curveMult;
                //xFinalCurve.m_CurveMin = xFinalCurve.m_CurveMax = xCurve;

                //zFinalCurve.m_CurveMultiplier = curveMult;
                //zFinalCurve.m_CurveMin = zFinalCurve.m_CurveMax = zCurve;

                // apply force to particles
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.x = xFinalCurve;
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.y = zeroCurve;
                //flame.GetComponent<ParticleSystem>().velocityOverLifetime.z = zFinalCurve;

                if (stopped) // fadeout
                {
                    curveMult = curveMult / (fadeoutMult += t);
                }

                // dim lights
                outdoorsLight.GetComponent<Light>().intensity = outdoorsLightIntensity - outdoorsLightIntensity / (Mathf.Clamp(curveMult, 2f, 20f) / 2f);

                // dim SSS 
                attenuation = attenuationBase - attenuationBase / (Mathf.Clamp(curveMult, 2f, 20f) / 2f);
                SetSSSAttenuation(attenuation);

                yield return new WaitForEndOfFrame();

                if (!stopped && t > animationTime / 2f)
                {
                    ToggleLight(false);
                    stopped = true;
                }
            }

            ToggleLight(false); // again to zero all values

            ienumeratorWorking = false;
            yield break;
        }

        [HideFromIl2Cpp]
        IEnumerator DestroyWhenSafe()
        {
            while (isInPlacementMode)
            {
                yield return new WaitForEndOfFrame();
            }

            Destroy(thisGearItem.gameObject);

            yield break;
        }

        // disabnle gflame on update while moveinh

        private void DoSetup()
        {
            if (thisGearItem.m_BeenInPlayerInventory)
            {
                isLit = !thisGearItem.m_BeenInspected;
            }

            Color color0 = new Color(1f, 0.7f, 0.375f);
            Color color1 = new Color(1f, 0.66f, 0.375f);


            if (!outdoorsLight.GetComponent<LightTracking>())
            {
                outdoorsLight.AddComponent<LightTracking>();
                outdoorsLight.GetComponent<LightTracking>().EnableWeaponCameraLighting(true);
            }

            if (!outdoorsLight.GetComponent<LightFlickering>())
            {
                outdoorsLight.AddComponent<LightFlickering>();
                outdoorsLight.GetComponent<LightFlickering>().duration = 0.1f;
                outdoorsLight.GetComponent<LightFlickering>().color0 = color0;
                outdoorsLight.GetComponent<LightFlickering>().color1 = color1;
            }


            if (!selfLight.GetComponent<LightTracking>())
            {
                selfLight.AddComponent<LightTracking>();
            }

            if (!selfLight.GetComponent<LightRandomIntensity>())
            {
                selfLight.AddComponent<LightRandomIntensity>();
                selfLight.GetComponent<LightRandomIntensity>().m_Min = 2f;
                selfLight.GetComponent<LightRandomIntensity>().m_Max = 6f;
                selfLight.GetComponent<LightRandomIntensity>().m_IntervalSeconds = 0.2f;
            }

            // setup shader
            Texture emissiveTex = thisGearItem.gameObject.GetComponent<MeshRenderer>().material.GetTexture("_EmissionMap"); // get texture from embedded

            thisGearItem.gameObject.GetComponent<MeshRenderer>().material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            thisGearItem.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_Emissive", emissiveTex); // apply texture to HL's shader

            // setup force curves
            xCurve.AddKey(0.0f, 0.0f);
            xCurve.AddKey(0.25f, 0.0f);
            xCurve.AddKey(1.0f, 0.25f);
            //xFinalCurve.m_Mode = ParticleSystemCurveMode.TwoCurves;

            zCurve.AddKey(0.0f, 0.0f);
            zCurve.AddKey(0.25f, 0.0f);
            zCurve.AddKey(1.0f, 0.25f);
            //zFinalCurve.m_Mode = ParticleSystemCurveMode.TwoCurves;

        }

        public void Update()
        {
            if (!thisGearItem) return;
            if (!CWMain.isLoaded) return;

            if (destroyed) return;
            else if (thisGearItem.m_WornOut)
            {
                destroyed = true;

                if (isUsedForAnimation) return;

                if (Settings.options.keepFigurines)
                {
                    if (thisGearItem.gameObject.transform.FindChild("wick"))
                    {
                        isLit = false;
                        MelonCoroutines.Start(ExtinguishAnimated(ExtinguishType.DestroyWick));

                    }
                }
                else MelonCoroutines.Start(DestroyWhenSafe()); // destroy self

                return;
            }

            FXpoint.gameObject.active = !isInPlacementMode;


            if (!thisLoaded)
            {
                DoSetup();
                thisLoaded = true;
            }

            if (!thisGearItem.m_InPlayerInventory)
            {
                if (ienumeratorWorking) return;

                // something is changing it to Gear when dropped, so this is on Update instead of Awake
                if (pickupHelper.layer != 15)
                {
                    pickupHelper.layer = 15;
                }

                // lights on
                if (isLit) 
                {
                    if (!lights.active)
                    {
                        ToggleLight(true);
                        MelonCoroutines.Start(AnimateFlameInWind());
                    }

                    attenuation = attenuationBase + selfLight.GetComponent<Light>().intensity / 20f;

                    SetSSSAttenuation(attenuation);

                    if (!flame.GetComponent<ParticleSystem>().isPlaying) flame.GetComponent<ParticleSystem>().Play();

                    if (!GameManager.GetWindComponent().IsPositionOccludedFromWind(thisGearItem.transform.position) && !isUsedForAnimation)
                    {
                        isWindy = true;
                        if (GameManager.GetWindComponent().m_CurrentMPH > 10f)
                        {
                            isLit = false;
                            lastExtinguishReason = ExtinguishType.Wind;
                        }
                    }
                    else isWindy = false;
                    
                }

                // lights off
                if (!isLit)
                {
                    if (lights.active)
                    {
                        MelonCoroutines.Start(ExtinguishAnimated(lastExtinguishReason)); 
                    }
                }

            }
            // put in inventory
            else
            {
                if (isLit)
                {
                    isLit = false;
                    lastExtinguishReason = ExtinguishType.NoAnimation;

                }
            }
        }
    }
}
