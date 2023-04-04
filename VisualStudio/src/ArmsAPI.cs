namespace CalorieWaltz
{
    public static class AA
    {
        public static Animator? currentAnimator;
        public static Animator? animatorPreset;
        public static Animator? vanillaAnimator;

        public static GameObject? animatorHolder;
        public static GameObject? objectToAppendTo;
        public static GameObject? propPointRight;
        public static GameObject? propPointLeft;
        public static GameObject? propPointTorso;

        public static GameObject? toolRight;
        public static GameObject? toolLeft;
        public static GameObject? toolTorso;

        public static readonly int playerHandsLayer = LayerMask.NameToLayer("Weapon");
        public static readonly Shader vanillaSkinShader = Shader.Find("Shader Forge/TLD_StandardSkinned");

        public static bool animatorRegistered = false;

        //public static bool customAnimatorActive;

        public static CameraBasedJointPositioning? cameraPositioning;

        public static bool activationPrevented;


        public static bool CR_Activate_isRunning;


        public enum ToolPoint
        {
            RightHand,
            LeftHand,
            Torso
        }

        public static void Register(AssetBundle assBun, string mainBundleObject, bool force = false) // setup new animator
        {
            // skip if animator is already setup
            if (animatorHolder && !force) return;

            animatorHolder = assBun.LoadAsset<GameObject>(mainBundleObject);
            if (!animatorHolder) return;
            string keepName = animatorHolder.name;
            animatorHolder = UnityEngine.Object.Instantiate(animatorHolder);
            animatorHolder.name = keepName;
            UnityEngine.Object.DontDestroyOnLoad(animatorHolder);
            animatorHolder.active = false;

            objectToAppendTo = GameManager.GetTopLevelCharacterFpsPlayer()?.transform.Find("NEW_FPHand_Rig/GAME_DATA")?.gameObject;
            if (objectToAppendTo == null)
            {
                Utility.Log(CC.Red, "Can't find vanilla rig");
                return;
            }

            propPointRight = GameManager.GetPlayerAnimationComponent().m_RightPropPoint;
            propPointTorso = GameManager.GetPlayerAnimationComponent().m_ShoulderPropPoint;
            propPointLeft = GameManager.GetPlayerAnimationComponent().m_LeftPropPoint;
            animatorPreset = animatorHolder.GetComponent<Animator>();
            vanillaAnimator = objectToAppendTo.transform.GetParent().GetComponent<Animator>();

            cameraPositioning = objectToAppendTo.transform.GetParent().Find("AimingModes/Standard")?.GetComponent<CameraBasedJointPositioning>();

            if (cameraPositioning == null)
            {
                Utility.Log(CC.Red, "Can't find vanilla tilt component");
                return;
            }

            animatorRegistered = true;
        }

        public static GameObject? AppendTool(AssetBundle assBun, string toolName, string? toolRigRoot = null, ToolPoint point = ToolPoint.Torso, bool replaceStandardShader = true, bool force = false)
        {
            if (!animatorRegistered)
            {
                Utility.Log(CC.Red, "Animator is not registered (0)");
                return null;
            }

            // skip if tool is already setup
            if (point == ToolPoint.Torso && toolTorso && !force) return toolTorso;
            if (point == ToolPoint.RightHand && toolRight && !force) return toolRight;
            if (point == ToolPoint.LeftHand && toolLeft && !force) return toolLeft;
            GameObject? toolRig = null;
            GameObject? tool = null;

            // get tool rig
            if (toolRigRoot != null)
            {
                toolRig = animatorHolder.transform.Find(toolRigRoot)?.gameObject;
                if (!toolRig)
                {
                    Utility.Log(CC.Red, "No rig root found for tool: " + toolName);
                    return null;
                }
            }

            //get tool mesh
            tool = animatorHolder.transform.Find(toolName)?.gameObject;

            if (!tool)
            {
                Utility.Log(CC.Red, "Can't find tool: " + toolName);
                return null;
            }

            // set layer for mesh
            foreach (Transform t in tool.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = playerHandsLayer;
                if (replaceStandardShader)
                {
                    Renderer r = t.GetComponent<Renderer>();
                    if (!r) continue;

                    foreach (Material m in r.materials)
                    {
                        if (m.shader.name == "Standard") m.shader = vanillaSkinShader;
                    }

                }

            }

            // append tool rig and mesh to prop point
            if (point == ToolPoint.Torso)
            {
                if (toolRigRoot != null) toolRig.transform.SetParent(propPointTorso.transform);
                tool.transform.SetParent(propPointTorso.transform);
                toolTorso = tool;
            }
            else if (point == ToolPoint.RightHand)
            {
                if (toolRigRoot != null) toolRig.transform.SetParent(propPointRight.transform);
                tool.transform.SetParent(propPointRight.transform);
                toolRight = tool;
            }
            else if (point == ToolPoint.LeftHand)
            {
                if (toolRigRoot != null) toolRig.transform.SetParent(propPointLeft.transform);
                tool.transform.SetParent(propPointLeft.transform);
                toolLeft = tool;
            }
            tool.transform.localPosition = Vector3.zero;
            tool.transform.localEulerAngles = Vector3.zero;
            tool.active = false;

            return tool;
        }

        public static bool? IsFinished(this Animator a, bool? b = null)
        {
            if (b != null)
            {
                a.SetBool("external_finished", (bool)b);
                return null;
            }
            else
            {
                return a.GetBool("external_finished");
            }
        }

        public static bool? IsAccessed(this Animator a, bool? b = null)
        {
            if (b != null)
            {
                a.SetBool("external_access", (bool)b);
                return null;
            }
            else
            {
                return a.GetBool("external_access");
            }
        }

        public static bool? IsLocked(this Animator a, bool? b = null)
        {
            if (b != null)
            {
                a.SetBool("external_lockAcess", (bool)b);
                return null;
            }
            else
            {
                return a.GetBool("external_lockAcess");
            }
        }

        public static bool IsInState(this Animator a, string state) => a.GetCurrentAnimatorStateInfo(0).IsName(state);


        public static bool IsInState(this Animator a, string[] state)
        {
            foreach (string s in state)
            {
                if (a.GetCurrentAnimatorStateInfo(0).IsName(s)) return true;
            }

            return false;
        }


        public static IEnumerator Activate() // kill existing custom animator and enable this one
        {
            CR_Activate_isRunning = true;

            activationPrevented = false;

            if (!animatorRegistered || !objectToAppendTo)
            {
                Utility.Log(CC.Red, "Animator is not registered (1)");
                CR_Activate_isRunning = false;
                yield break;
            }

            Animator temp = objectToAppendTo.GetComponent<Animator>();

            // send access to existing animator and wait for it to finish, then destroy
            if (temp != null && currentAnimator != temp)
            {
                if (temp.IsLocked() == true) // stop if other animation is preventing interruption
                {
                    activationPrevented = true;
                    CR_Activate_isRunning = false;
                    yield break;
                }

                temp.IsAccessed(true);

                while (temp.IsFinished() != true && temp.isActiveAndEnabled)
                {
                    yield return new WaitForEndOfFrame();
                }

                UnityEngine.Object.Destroy(temp);

                while (temp != null)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            if (!currentAnimator)
            {
                currentAnimator = objectToAppendTo.AddComponent<Animator>();
                currentAnimator.runtimeAnimatorController = animatorPreset.runtimeAnimatorController;
                currentAnimator.avatar = animatorPreset.avatar;
            }
            vanillaAnimator.enabled = false;
            currentAnimator.enabled = true;

            // reset animator to rest state
            currentAnimator.Rebind();
            currentAnimator.Update(0f);

            currentAnimator.IsFinished(false);
            CR_Activate_isRunning = false;

        }

        public static void ActivateTool(ToolPoint tool, bool active)
        {
            if (!animatorRegistered)
            {
                Utility.Log(CC.Red, "Animator is not registered (2)");
                return;
            }

            if (tool == ToolPoint.Torso)
            {
                if (toolTorso) toolTorso.active = active;
            }
            else if (tool == ToolPoint.RightHand)
            {
                if (toolRight) toolRight.active = active;
            }
            else if (tool == ToolPoint.LeftHand)
            {
                if (toolLeft) toolLeft.active = active;
            }
        }


        public static void Done()
        {
            if (!animatorRegistered)
            {
                Utility.Log(CC.Red, "Animator is not registered (3)");
                return;
            }

            if (!currentAnimator) return;

            vanillaAnimator.enabled = true;
            currentAnimator.enabled = false;

            currentAnimator.IsFinished(true);
        }

        public static void DestroyOnMainMenu()
        {
            animatorRegistered = false;
            if (animatorHolder) UnityEngine.Object.Destroy(animatorHolder);
        }

        public static void SendTrigger(bool set, string name)
        {
            if (set) currentAnimator.SetTrigger(name);
            else currentAnimator.ResetTrigger(name);
        }

        public static void SendBool(bool value, string name)
        {
            currentAnimator.SetBool(name, value);
        }

        public static void SendFloat(float value, string name)
        {
            currentAnimator.SetFloat(name, value);
        }

        public static void SendInt(int value, string name)
        {
            currentAnimator.SetInteger(name, value);
        }

    }
}
