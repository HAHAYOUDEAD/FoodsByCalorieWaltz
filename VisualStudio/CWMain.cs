using System;
using System.IO;
using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;


namespace CalorieWaltz
{

    public class CWMain : MelonMod
    {
        // honey parameters
        private static GearItem honeyNuts;

        private static bool eatingHoney;
        private static float honeyConsumed;

        private static bool drankHotBeverage;
        private static float poisonChanceReduction;
        
        private static List<float> honeyEatEndTime = new List<float>();
        private static List<float> honeyEatAmount = new List<float>();
        
        private static readonly float honeySafeAmount = 300f; //calories
        private static readonly float honeyCureTime = 8f; //hours
        
        private static string localizedName; // for poison display

        // icecream parameters
        private static GearItem iceCream;

        private static float iceCreamLeft;
        private static float iceCreamTotalCalories;

        private static bool eatingIcecream;

        // lucky parameters
        private static GearItem prianik;

        private static float prianikLastCalories;

        private static bool lucky;

        private static readonly int redChance = 6;
        private static readonly int purpleChance = 12;
        private static readonly int blueChance = 30;
        //private static int greenChance = 34;

        private static int humanChance = 0;

        private static int godChance = 0;

        private static bool godHelped;

        // pot texture
        public static bool loadedCookingTex;

        private static List<string> cookableGear = new List<string>();

        public static Material vanillaLiquidMaterial;

        // soup texture
        public static Texture bigSoup;
        public static Texture2D bigSoup2D;


        // opened cans
        private static bool injectedMeshSwapComponent;

        private static List<string> cannedGear = new List<string>();


        // calorie tweak
        public static object[][] allFoodItems;

        public static readonly int DCALapplesaucePryanik = 300;
        public static readonly int DCALbananaChips = 500;
        public static readonly int DCALcrabSnack = 320;
        public static readonly int DCALfigConfiture = 900;
        public static readonly int DCALglintweinCup = 200;
        public static readonly int DCALhoneyNuts = 3000;
        public static readonly int DCALhotSpringSoda = 120;
        public static readonly int DCALicecreamCup = 300;
        public static readonly int DCALjubileeCookies = 800;
        public static readonly int DCALkvass = 150;
        public static readonly int DCALlecso = 300;
        public static readonly int DCALmarinaraMackerel = 250;
        public static readonly int DCALmysteryCake = 600;
        public static readonly int DCALnordShoreChocolate = 500;
        public static readonly int DCALoatsBowl = 250;
        public static readonly int DCALpinenutBrittle = 1200;
        public static readonly int DCALroastedAlmonds = 240;
        public static readonly int DCALsolitudeCider = 180;
        public static readonly int DCALstrawberryPlombir = 200;
        public static readonly int DCALtunaPate = 200;

        public static readonly int DCALABCSoup = 400;
        public static readonly int DCALlatteDrops = 35;
        public static readonly int DCALhematogen = 150;
        public static readonly int DCALrahatLokum = 900;

        public static bool loadedCalories;

        public static GearItem lastInspected;

        public static bool queueInventoryUpdate;

        // Misc parameters
        public static string modsPath;

        public static bool isLoaded;

        public static int currentLevel;

        private static bool addedCustomComponents;

        private static bool bearInDPDoSetup;
        private static GameObject bearInDPGO;
        private static GameObject bearInDPDummy;
        public static bool bearInDPCurrentlyInspecting;

        public static AssetBundle CWFCustomBundle;

        public static Shader vanillaShader;
        //public static bool doneWithDPBear;

        private static bool switchAnimator;

        public static GameObject holdCandlePlatePoint;

        public static bool holdingAnimationCoroutineIsRunning;

        public static List<GameObject> currentCandleList = new List<GameObject>();

        // Localized strings

        public static LocalizedString localizedCandleLeft100;
        public static LocalizedString localizedCandleLeft90;
        public static LocalizedString localizedCandleLeft75;
        public static LocalizedString localizedCandleLeft50;
        public static LocalizedString localizedCandleLeft25;
        public static LocalizedString localizedCandleLeft7;


        private static bool loadedLocalizations;

        //private static AssetBundle embeddedBundle = LoadEmbeddedAssetBundle();


        public override void OnApplicationStart()
        {
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\Mods");

            Settings.OnLoad();

            LoadEmbeddedAssetBundle();

            vanillaShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
        }

        public override void OnSceneWasInitialized(int level, string name)
        {
            currentLevel = level;

            if (level == 2) // reset everything in main menu
            {
                if (honeyConsumed != 0f)
                {
                    honeyEatEndTime = new List<float>();
                    honeyEatAmount = new List<float>();
                    eatingHoney = false;
                    honeyConsumed = 0f;
                    drankHotBeverage = false;
                    poisonChanceReduction = 0f;
                }

                // queueInventoryUpdate = true;

            }




            if (level >= 3)
            {
                isLoaded = true;
            }

            if (!loadedCalories)
            {
                UpdateFoodArray();
                UpdateCaloriesInPrefabs();
                loadedCalories = true;
            }

            if (!loadedLocalizations)
            {
                localizedCandleLeft100 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft100");
                localizedCandleLeft90 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft90");
                localizedCandleLeft75 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft75");
                localizedCandleLeft50 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft50");
                localizedCandleLeft25 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft25");
                localizedCandleLeft7 = ModComponent.Utils.NameUtils.CreateLocalizedString("CWF_candleLeft7");

                loadedLocalizations = true;
            }

            DoStuffWithGear();

            if (!Settings.options.disableCollectibleSpawns) // setup some custom objects in scene
            {
                if (name == "RiverValleyRegion")
                {
                    GameObject treeBridge = GameObject.Find("Art/TerrainObjects/GeneralShelter/OBJ_LogBridgeC_Prefab");
                    GameObject treeBridgeInstance = null;
                    if (treeBridge)
                    {
                        treeBridgeInstance = GameObject.Instantiate(treeBridge);
                    }
                    if (treeBridgeInstance)
                    {
                        treeBridgeInstance.name = "CWF_treeBridge";
                        treeBridgeInstance.transform.position = new Vector3(996.9843f, 20.7172f, 1087.934f);
                        treeBridgeInstance.transform.rotation = Quaternion.Euler(new Vector3(5.4086f, 163.8038f, 22.8595f));
                    }
                }

                if (name.Contains("WhalingStationRegion"))
                {
                    bearInDPDoSetup = true;
                }
            }

            if (!Settings.options.disableLore) // setup lore assets
            {
                if (name == "CanneryRegion")
                {
                    GameObject leg = CWFCustomBundle?.LoadAsset<GameObject>("CWF_customMesh_prosteticLeg");
                    if (!leg)
                    {
                        MelonLogger.Msg(ConsoleColor.Yellow, "Failed to load custom asset: CWF_customMesh_prosteticLeg");
                        return;
                    }
                    else
                    {
                        leg = GameObject.Instantiate(leg);
                        leg.GetComponent<MeshRenderer>().material.shader = vanillaShader;
                        leg.transform.position = new Vector3(490.5f, 73.715f, 4.2f);
                        leg.transform.eulerAngles = new Vector3(0f, 142.5f, 318f);
                        leg.layer = 9; // TerrainObject
                    }
                }
                if (name == "RuralRegion")
                {
                    GameObject collision01 = CWFCustomBundle?.LoadAsset<GameObject>("CWF_customCollision_01");
                    if (!collision01)
                    {
                        MelonLogger.Msg(ConsoleColor.Yellow, "Failed to load custom asset: CWF_customCollision_01");
                        return;
                    }
                    else
                    {
                        collision01 = GameObject.Instantiate(collision01);
                        collision01.transform.position = new Vector3(2008.25f, 49.3f, 1734.9f);
                        collision01.transform.eulerAngles = new Vector3(270f, 325f, 0f);
                        collision01.transform.localScale = new Vector3(3f, 9f, 4f);
                        collision01.GetComponent<MeshRenderer>().enabled = false;
                        collision01.layer = 9; // TerrainObject
                    }

                }
            }

        }

        public override void OnSceneWasUnloaded(int level, string name)
        {
            isLoaded = false;
            bearInDPDoSetup = false;
            //doneWithDPBear = false;
            CWMain.currentCandleList = new List<GameObject>();
        }

        /*
        private static AssetBundle LoadEmbeddedAssetBundle()
        {
            MemoryStream memoryStream;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreWorkbenches.res.workbenchb"))
            {
                memoryStream = new MemoryStream((int)stream.Length);
                stream.CopyTo(memoryStream);
            }
            if (memoryStream.Length == 0)
            {
                throw new System.Exception("No data loaded!");
            }
            return AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }

        
        internal static GameObject GetNewWorkbenchInstance()
        {
            GameObject workbenchPrefab = assetBundle.LoadAsset<GameObject>("ASSET_PATH");
            if (workbenchPrefab is null)
            {
                MelonLogger.LogError("Workbench prefab is null!");
                return null;
            }
            else return GameObject.Instantiate(workbenchPrefab);
        }
        */

        private static void LoadEmbeddedAssetBundle()
        {
            MemoryStream memoryStream;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FoodsByCalorieWaltz.Resources.cwfcustom");
            memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);

            CWFCustomBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }

        private static void DoStuffWithGear()
        {
            /*
            // DEBUG
            
            UnityEngine.Object.Destroy(HarmonyValues.line);
            HarmonyValues.line = (new GameObject("testLine")).AddComponent<LineRenderer>();
            HarmonyValues.line.material = new Material(Shader.Find("Sprites/Default"));
            HarmonyValues.line.widthMultiplier = 0.01f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            HarmonyValues.line.colorGradient = gradient;

            
            // DEBUG
            */

            if (!loadedCookingTex) // adding pot cooking textures
            {
                cookableGear.Add("glintwein"); // case-sensitive
                cookableGear.Add("lecso");
                cookableGear.Add("nannasOats");
                cookableGear.Add("ABCSoup");

                Material potMat;
                GameObject potGear;

                potMat = vanillaLiquidMaterial = new Material(Resources.Load("GEAR_CoffeeCup").TryCast<GameObject>().GetComponent<Cookable>().m_CookingPotMaterialsList[0]);

                vanillaLiquidMaterial.name = "Liquid";

                for (int i = 0; i < cookableGear.Count; i++)
                {
                    //potMat = new Material(Shader.Find("Shader Forge/TLD_Food_Liquid"));
                    potGear = Resources.Load("GEAR_" + cookableGear[i]).TryCast<GameObject>();
                    if (potGear == null) continue;

                    //potMat = new Material(Resources.Load("GEAR_CoffeeCup").TryCast<GameObject>().GetComponent<Cookable>().m_CookingPotMaterialsList[0]);

                    potMat.name = ("CKN_" + cookableGear[i] + "_MAT");
                    potMat.mainTexture = potGear.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture;

                    potGear.GetComponent<Cookable>().m_CookingPotMaterialsList = new Material[1] { potMat };
                }

                loadedCookingTex = true;
            }


            if (!injectedMeshSwapComponent) // add opened mesh variant
            {
                cannedGear.Add("lecso"); // case-sensitive
                cannedGear.Add("ABCSoup");

                GameObject gear;

                for (int i = 0; i < cannedGear.Count; i++)
                {
                    gear = Resources.Load("GEAR_" + cannedGear[i]).TryCast<GameObject>();
                    if (cannedGear == null) continue;

                    gear.AddComponent<MeshSwapItem>();
                    gear.GetComponent<MeshSwapItem>().m_GearItem = gear.GetComponent<GearItem>();
                    gear.GetComponent<MeshSwapItem>().m_MeshObjOpened = gear.transform.FindChild(cannedGear[i] + "_opened").gameObject;

                    if (gear.name.Contains("ABCSoup"))
                    {
                        bigSoup = gear.GetComponent<MeshSwapItem>().m_MeshObjOpened.GetComponent<MeshRenderer>().GetMaterialArray()[1].mainTexture;
                    }

                    gear.GetComponent<MeshSwapItem>().m_MeshObjUnopened = gear.transform.FindChild(cannedGear[i]).gameObject;
                }

                injectedMeshSwapComponent = true;
            }


            if (!addedCustomComponents)
            {
                /*
                if (!Settings.options.disableHematogen)
                {


                }
                */

                GameObject gear;

                // adding heal effect to hematogen
                string gear1 = "hematogen";


                gear = Resources.Load("GEAR_" + gear1).TryCast<GameObject>();

                gear.AddComponent<ConditionOverTimeBuff>();
                gear.GetComponent<ConditionOverTimeBuff>().m_ConditionIncreasePerHour = 1.5f;
                gear.GetComponent<ConditionOverTimeBuff>().m_NumHours = 2f;

                addedCustomComponents = true;
            }

            SwitchPryanikTexture(!Settings.options.disableCollectibles);

        }

        public static void SwitchPryanikTexture(bool collectibleInside)
        {
            GameObject gear;
            GameObject altTexHolder;

            Texture withCollectible = new Texture();
            Texture withoutCollectible = new Texture();

            gear = Resources.Load("GEAR_applesaucePryanik").TryCast<GameObject>();

            bool needChange = false;

            if (!gear) return;

            altTexHolder = gear.transform.FindChild("ALTtexture")?.gameObject;

            if (!altTexHolder) return;

            if (gear.GetComponent<MeshRenderer>().material.mainTexture.name.Contains("nopromo"))
            {
                if (collectibleInside)
                {
                    needChange = true;
                    withoutCollectible = gear.GetComponent<MeshRenderer>().material.mainTexture;
                    withCollectible = altTexHolder.GetComponent<MeshRenderer>().material.mainTexture;
                }
            }
            else
            {
                if (!collectibleInside)
                {
                    needChange = true;
                    withoutCollectible = altTexHolder.GetComponent<MeshRenderer>().material.mainTexture;
                    withCollectible = gear.GetComponent<MeshRenderer>().material.mainTexture;
                }

            }

            if (needChange)
            {
                if (collectibleInside)
                {
                    gear.GetComponent<MeshRenderer>().material.mainTexture = withCollectible;
                    altTexHolder.GetComponent<MeshRenderer>().material.mainTexture = withoutCollectible;
                }
                else
                {
                    gear.GetComponent<MeshRenderer>().material.mainTexture = withoutCollectible;
                    altTexHolder.GetComponent<MeshRenderer>().material.mainTexture = withCollectible;
                }
            }


        }

        public static void UpdateFoodArray()
        {
            allFoodItems = new object[][]
            {
                new object[] { "GEAR_applesaucePryanik", Settings.options.perItemTweak ? Settings.options.CALapplesaucePryanik : (DCALapplesaucePryanik * Settings.options.bigCalorieScale / 100)},
                new object[] { "GEAR_bananaChips", Settings.options.perItemTweak ? Settings.options.CALbananaChips : (DCALbananaChips * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_crabSnack", Settings.options.perItemTweak ? Settings.options.CALcrabSnack : (DCALcrabSnack * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_figConfiture", Settings.options.perItemTweak ? Settings.options.CALfigConfiture : (DCALfigConfiture * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_glintweinCup", Settings.options.perItemTweak ? Settings.options.CALglintweinCup : (DCALglintweinCup * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_honeyNuts", Settings.options.perItemTweak ? Settings.options.CALhoneyNuts : (DCALhoneyNuts * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_hotSpringSoda", Settings.options.perItemTweak ? Settings.options.CALhotSpringSoda : (DCALhotSpringSoda * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_icecreamCup", Settings.options.perItemTweak ? Settings.options.CALicecreamCup : (DCALicecreamCup * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_jubileeCookies", Settings.options.perItemTweak ? Settings.options.CALjubileeCookies : (DCALjubileeCookies * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_kvass", Settings.options.perItemTweak ? Settings.options.CALkvass : (DCALkvass * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_lecso", Settings.options.perItemTweak ? Settings.options.CALlecso : (DCALlecso * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_marinaraMackerel", Settings.options.perItemTweak ? Settings.options.CALmarinaraMackerel : (DCALmarinaraMackerel * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_mysteryCake", Settings.options.perItemTweak ? Settings.options.CALmysteryCake : (DCALmysteryCake * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_nordShoreChocolate", Settings.options.perItemTweak ? Settings.options.CALnordShoreChocolate : (DCALnordShoreChocolate * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_oatsBowl", Settings.options.perItemTweak ? Settings.options.CALoatsBowl : (DCALoatsBowl * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_pinenutBrittle", Settings.options.perItemTweak ? Settings.options.CALpinenutBrittle : (DCALpinenutBrittle * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_roastedAlmonds", Settings.options.perItemTweak ? Settings.options.CALroastedAlmonds : (DCALroastedAlmonds * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_solitudeCider", Settings.options.perItemTweak ? Settings.options.CALsolitudeCider : (DCALsolitudeCider * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_strawberryPlombir", Settings.options.perItemTweak ? Settings.options.CALstrawberryPlombir : (DCALstrawberryPlombir * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_tunaPate", Settings.options.perItemTweak ? Settings.options.CALtunaPate : (DCALtunaPate * Settings.options.bigCalorieScale / 100) },

                new object[] { "GEAR_ABCSoup", Settings.options.perItemTweak ? Settings.options.CALABCSoup : (DCALABCSoup * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_latteDrops", Settings.options.perItemTweak ? Settings.options.CALlatteDrops : (DCALlatteDrops * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_hematogen", Settings.options.perItemTweak ? Settings.options.CALhematogen : (DCALhematogen * Settings.options.bigCalorieScale / 100) },
                new object[] { "GEAR_rahatLokum", Settings.options.perItemTweak ? Settings.options.CALrahatLokum : (DCALrahatLokum * Settings.options.bigCalorieScale / 100) }
            };

        }

        public static void UpdateCaloriesOnPickup(GearItem item)
        {
            for (int i = 0; i < allFoodItems.Length; i++)
            {
                if ((string)allFoodItems[i][0] == item.name)
                {
                    item.m_FoodItem.m_CaloriesTotal = item.m_FoodItem.m_CaloriesRemaining = (int)allFoodItems[i][1];
                    //GameManager.GetPlayerManagerComponent().UpdateInspectGear();
                    InterfaceManager.GetPanel<Panel_HUD>().m_InspectMode_Contains.text = item.m_FoodItem.m_CaloriesRemaining.ToString("F0") + " " + Localization.Get("GAMEPLAY_Calories");
                    //InterfaceManager.GetPanel<Panel_HUD>().m_EssentialHud.active = true;
                    return;
                }
            }
        }

        public static void UpdateCaloriesInInventory()
        {
            Il2CppSystem.Collections.Generic.List<GearItem> tempList = new Il2CppSystem.Collections.Generic.List<GearItem>();

            for (int i = 0; i < allFoodItems.Length; i++)
            {
                GameManager.GetInventoryComponent().GetItems((string)allFoodItems[i][0], tempList);
                for (int t = 0; t < tempList.Count; t++)
                {
                    tempList[t].m_FoodItem.m_CaloriesTotal = tempList[t].m_FoodItem.m_CaloriesRemaining = (int)allFoodItems[i][1];
                }
            }
        }

        public static void UpdateCaloriesInPrefabs()
        {
            for (int i = 0; i < allFoodItems.Length; i++)
            {
                GameObject itemForCalorieUpdate = null;

                itemForCalorieUpdate = Resources.Load((string)allFoodItems[i][0]).TryCast<GameObject>();

                if (itemForCalorieUpdate != null)
                {
                    itemForCalorieUpdate.GetComponent<FoodItem>().m_CaloriesTotal = (int)allFoodItems[i][1];
                    itemForCalorieUpdate.GetComponent<FoodItem>().m_CaloriesRemaining = (int)allFoodItems[i][1];
                }
                else
                {
                    MelonLogger.Msg(ConsoleColor.Yellow, (string)allFoodItems[i][0] + " calories failed to update");
                }
            }

        }

        public static Texture2D GetRandomSoupTexture(string set, int id)
        {
            int blockSize = 256;

            if (!bigSoup2D)
            {
                bigSoup2D = bigSoup.ToTexture2D();
            }

            Texture2D newTexture = new Texture2D(blockSize, blockSize);

            int xInit;
            int yInit = 0;

            if (set == "common") // row 1
            {
                yInit = bigSoup.height - blockSize;
            }

            else if (set == "rare") // row 3
            {
                yInit = bigSoup.height - blockSize * 3;
            }

            else if (set == "legendary") // row 5
            {
                yInit = bigSoup.height - blockSize * 5;
            }

            else return null;

            if (id > 8) // go to next row
            {
                id = id - 8;
                yInit = yInit - blockSize;
            }

            xInit = blockSize * id - blockSize;
            Color[] pix = bigSoup2D.GetPixels(xInit, yInit, blockSize, blockSize);

            newTexture.SetPixels(pix);
            newTexture.Apply();

            return newTexture;
        }

        /*
        public static void SwitchAltTexture(GearItem gi)
        {
            Transform altTexHolder;
            Texture altTex;
            Texture ogTex;

            altTexHolder = gi.gameObject.transform.Find("ALTtexture");

            if (altTexHolder)
            {
                ogTex = gi.GetComponent<MeshRenderer>().material.mainTexture;
                altTex = altTexHolder.gameObject.GetComponent<MeshRenderer>().material.mainTexture;

                gi.GetComponent<MeshRenderer>().material.mainTexture = altTex;
                altTexHolder.gameObject.GetComponent<MeshRenderer>().material.mainTexture = ogTex;
            }


        }
        */

        private static GameObject InstantiaBearteInstant (GameObject bear)
        {
            
            GameObject instance = UnityEngine.Object.Instantiate(bear);
            instance.name = "POSHELNAHUIBLATSUKA";
            if (bear.GetComponent<GearItem>().m_WornOut) instance.GetComponent<CollectibleCandleComponent>().destroyed = true;
            MelonLogger.Msg("hello");
            return instance;
        }
       

        public static IEnumerator ManageCandleHoldingAnimation(bool start, GameObject bear)
        {
            holdingAnimationCoroutineIsRunning = true;

            Transform rigRoot = GameManager.GetTopLevelCharacterFpsPlayer()?.transform?.Find("NEW_FPHand_Rig");
            if (!rigRoot)
            {
                Utility.Log(1);
                holdingAnimationCoroutineIsRunning = false;
                yield break;
            }
            GameObject cameraPositioning = rigRoot.Find("AimingModes/Standard")?.gameObject;

            Animator customAnimator = HarmonyValues.customAnimator;
            Animator vanillaAnimator = HarmonyValues.vanillaAnimator;
            GameObject platePoint = HarmonyValues.holdCandlePlatePoint;

            if (start)
            {
                while (GameManager.GetVpFPSCamera().WeaponSwitchInProgress()) // wait for vanilla animation end
                {
                    yield return new WaitForEndOfFrame();
                }

                customAnimator.ResetTrigger("bring");
                customAnimator.ResetTrigger("putDown");

                if (!GameManager.GetPlayerManagerComponent().IsInPlacementMode()) // abort if not in placement mode
                {
                    customAnimator.enabled = false;
                    platePoint.DestroyAllImmediateChildren();
                    holdingAnimationCoroutineIsRunning = false;
                    yield break;
                }

                // setup animators
                vanillaAnimator.enabled = false;
                customAnimator.enabled = true;

                // setup plate and bear
                platePoint.transform.GetParent().gameObject.active = true;
                GameObject instancedBear = UnityEngine.Object.Instantiate(bear);
                CollectibleCandleComponent instanceComp = instancedBear.GetComponent<CollectibleCandleComponent>();

                if (bear.GetComponent<CollectibleCandleComponent>().isLit)
                {
                    LayerMask LMincludeHands = (1 << 23); //8388608
                    LayerMask LMexcludeHands = ~LMincludeHands; //-8388609;

                    // prevent DecayOverTODHours calculation
                    instancedBear.GetComponent<GearItem>().m_DailyHPDecay = 0f;
                    instancedBear.GetComponent<GearItem>().m_CurrentHP = bear.GetComponent<GearItem>().m_CurrentHP;

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

                    customAnimator.SetLayerWeight(1, 0f); // hold hand in front of candle
                }
                else
                {
                    customAnimator.SetLayerWeight(1, 1f); // use only right hand
                }
                instancedBear.transform.SetParent(platePoint.transform);
                instancedBear.transform.localPosition = Vector3.zero;
                instancedBear.transform.localEulerAngles = Vector3.zero;
                instancedBear.transform.localScale = Vector3.one * 0.25f;
                Utility.SetLayerRecursively2(instancedBear, 23); // weapon layer

                // setup camera
                if (cameraPositioning)
                {
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_CenterAngle = 20f;
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_MaxAngle = 100f;
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_JointPositionAtStart = new Vector3(0f, -0.1431f, -0.026f);
                }
                else
                {
                    Utility.Log(2);
                }

                // disable any current tool
                if (rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponLeftHand) rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponLeftHand.gameObject.active = false;
                if (rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponRightHand) rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponRightHand.gameObject.active = false;



                customAnimator.SetTrigger("bring");
                holdingAnimationCoroutineIsRunning = false;
                yield break;
            }
            else
            {
                holdingAnimationCoroutineIsRunning = true;
                customAnimator.SetTrigger("putDown");

                while (customAnimator.GetCurrentAnimatorStateInfo(0).IsName("put") || customAnimator.GetCurrentAnimatorStateInfo(0).IsName("idle")) // wait for animation to end
                {
                    yield return new WaitForEndOfFrame();
                }

                if (GameManager.GetPlayerManagerComponent().IsInPlacementMode()) // abort if still in placement mode
                {
                    customAnimator.ResetTrigger("bring");
                    customAnimator.ResetTrigger("putDown");
                }
                customAnimator.enabled = false;
                platePoint.DestroyAllImmediateChildren();
                platePoint.transform.GetParent().gameObject.active = false;

                if (cameraPositioning) // restore camera positioning
                {
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_CenterAngle = 0f;
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_Camera_MaxAngle = 90f;
                    cameraPositioning.GetComponent<CameraBasedJointPositioning>().m_JointPositionAtStart = new Vector3(0f, -0.1431f, 0f);
                }

                // restore previous tools
                if (rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponLeftHand) rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponLeftHand.gameObject.active = true;
                if (rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponRightHand) rigRoot.GetComponent<PlayerAnimation>().m_EquippedFirstPersonWeaponRightHand.gameObject.active = true;


                vanillaAnimator.enabled = true;

                holdingAnimationCoroutineIsRunning = false;
                yield break;
            }
        }

        public static IEnumerator DelayPlayerAnimatorAfterInteraction(PlayerManager pm)
        {
            //Animator customAnimator = HarmonyValues.customAnimator;

            while (holdingAnimationCoroutineIsRunning) // wait for animation to end
            {
                yield return new WaitForEndOfFrame();
            }

            holdingAnimationCoroutineIsRunning = true; // for interaction suspention

            yield return new WaitForEndOfFrame(); // prevent infinite loop
            pm.DoDelayedInteractionEnd();
            //pm.ItemInHandsDuringInteractionEndInternal();

            holdingAnimationCoroutineIsRunning = false;
            yield break;
        }

        public override void OnUpdate()
        {
            if (!isLoaded || GameManager.GetPlayerManagerComponent() == null) return;


            // add some drop chance if you're having a bad day
            if (GameManager.GetSprainPainComponent().HasSprainPain() && !godHelped && !GameManager.GetRestComponent().IsSleeping())
            {
                if (Utils.RollChance(Mathf.Ceil(GameManager.GetSprainPainComponent().GetAfflictionsCount() * 50f)))
                {
                    godChance = 10;
                }
                godHelped = true;
            }

            if (GameManager.GetRestComponent().IsSleeping() && godHelped)
            {
                godHelped = false;
            }

            // apply edited calories to item on pickup
            if (GameManager.GetPlayerManagerComponent().GearItemBeingInspected() != null)
            {
                if (GameManager.GetPlayerManagerComponent().GearItemBeingInspected() != lastInspected && GameManager.GetPlayerManagerComponent().GearItemBeingInspected().m_HasBeenOwnedByPlayer == false)
                {
                    lastInspected = GameManager.GetPlayerManagerComponent().GearItemBeingInspected();
                    UpdateCaloriesOnPickup(lastInspected);
                }
            }

            // when intentory is loaded, update calories in inventory
            // too cheesy
            /*
            if (GameManager.GetInventoryComponent().m_Items.Count >= 1 && queueInventoryUpdate)
            {
                UpdateCaloriesInInventory();
                queueInventoryUpdate = false;
            }
            */

            // when stopped eating
            if (GameManager.GetHungerComponent().GetItemBeingEaten() == null)
            {
                if (lucky && !Settings.options.disableCollectibles)
                {
                    lucky = false;

                    //if (prianik != null) return; // only drop collectible when fully eaten
                    if (prianikLastCalories != prianik.m_FoodItem.m_CaloriesTotal) return; // only drop collectible when first eaten




                    float purpleChanceFinal = Mathf.Ceil((100 + redChance) / 100 * purpleChance);
                    float blueChanceFinal = Mathf.Ceil((100 + redChance + purpleChanceFinal) / 100 * blueChance);
                    //float greenChanceFinal = Mathf.Ceil((100 + redChance + purpleChanceFinal + blueChanceFinal) / 100 * greenChance);

                    InterfaceManager.m_Panel_Inventory.Enable(false); // hide inventory to show collectible

                    // Play_VOInspectObjectImportant	 unethusiastic
                    // Play_InventoryLeave useless/ no use to me - maybe use if already collected
                    // Play_FireSuccess - sigh, sarcastic "perfect"

                    if (Utils.RollChance(redChance + (humanChance / 2f) + godChance))
                    {
                        //red
                        List<string> randomPick = new List<string>
                        {
                            "GEAR_cardRedA",
                            "GEAR_cardRedB",
                            "GEAR_cardRedC",
                            "GEAR_cardRedD"
                        };
                        System.Random r = new System.Random();
                        int pick = r.Next(randomPick.Count);

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0 && Utils.RollChance(60))
                        {
                            pick = r.Next(randomPick.Count);
                        }

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0 && Utils.RollChance(30))
                        {
                            pick = r.Next(randomPick.Count);
                        }

                        GameManager.GetPlayerManagerComponent().ProcessInspectablePickupItem(Utils.InstantiateGearFromPrefabName(randomPick[pick]));
                        humanChance = 0;
                        godChance = 0;
                        godHelped = false;

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0)
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_FireSuccess", Voice.Priority.Critical);
                        }
                        else
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_InventoryNeeded", Voice.Priority.Critical);
                        }

                        return;
                    }
                    else if (Utils.RollChance(purpleChanceFinal + humanChance + godChance))
                    {
                        //purple
                        List<string> randomPick = new List<string>
                        {
                            "GEAR_cardPurpleA",
                            "GEAR_cardPurpleB",
                            "GEAR_cardPurpleC",
                            "GEAR_cardPurpleD"
                        };
                        System.Random r = new System.Random();
                        int pick = r.Next(randomPick.Count);

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0 && Utils.RollChance(30))
                        {
                            pick = r.Next(randomPick.Count);
                        }

                        GameManager.GetPlayerManagerComponent().ProcessInspectablePickupItem(Utils.InstantiateGearFromPrefabName(randomPick[pick]));
                        humanChance = (int)Mathf.Ceil(humanChance / 5f);
                        godChance = 0;
                        godHelped = false;

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0)
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_FireSuccess", Voice.Priority.Critical);
                        }
                        else
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_InventoryNeeded", Voice.Priority.Critical);
                        }

                        return;
                    }
                    else if (Utils.RollChance(blueChanceFinal + humanChance))
                    {
                        //blue
                        List<string> randomPick = new List<string>
                        {
                            "GEAR_cardBlueA",
                            "GEAR_cardBlueB",
                            "GEAR_cardBlueC",
                            "GEAR_cardBlueD"
                        };
                        System.Random r = new System.Random();
                        int pick = r.Next(randomPick.Count);

                        GameManager.GetPlayerManagerComponent().ProcessInspectablePickupItem(Utils.InstantiateGearFromPrefabName(randomPick[pick]));

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0)
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_FireSuccess", Voice.Priority.Critical);
                        }
                        else
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_VOInspectObjectImportant", Voice.Priority.Critical);
                        }

                        return;
                    }
                    else
                    {
                        //green
                        List<string> randomPick = new List<string>
                        {
                            "GEAR_cardGreenA",
                            "GEAR_cardGreenB",
                            "GEAR_cardGreenC",
                            "GEAR_cardGreenD"
                        };
                        System.Random r = new System.Random();
                        int pick = r.Next(randomPick.Count);

                        GameManager.GetPlayerManagerComponent().ProcessInspectablePickupItem(Utils.InstantiateGearFromPrefabName(randomPick[pick]));
                        if (Utils.RollChance(75f))
                        {
                            humanChance += 1;
                        }

                        if (GameManager.GetInventoryComponent().GetNumGearWithName(randomPick[pick]) > 0)
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_FireSuccess", Voice.Priority.Critical);
                        }
                        else
                        {
                            GameManager.GetPlayerVoiceComponent().Play("Play_VOInspectObjectImportant", Voice.Priority.Critical);
                        }

                        return;
                    }
                }

                if (eatingIcecream)
                {
                    float freezeAmount;

                    if (iceCream != null)
                    {
                        freezeAmount = Mathf.Round((iceCreamLeft - iceCream.m_FoodItem.m_CaloriesRemaining) / iceCream.m_FoodItem.m_CaloriesTotal * 20f);
                    }
                    else freezeAmount = Mathf.Round(iceCreamLeft / iceCreamTotalCalories * 20f);

                    GameManager.GetFreezingComponent().AddFreezing(freezeAmount);

                    if (GameManager.GetPlayerManagerComponent().GetFreezingBuffTimeRemainingHours() != 0f)
                    {
                        GameManager.GetPlayerManagerComponent().m_FreezingBuffHoursRemaining = 0.001f;
                    }
                    GameManager.GetFreezingComponent().MaybePlayPlayerFreezingTeethChatter();

                    eatingIcecream = false;
                }

                if (eatingHoney && !Settings.options.disableHoneyPoison)
                {
                    float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();

                    // adding every honey eating instance to a list, to remove when 8 hours passed
                    if (honeyNuts != null) honeyConsumed = honeyConsumed - honeyNuts.m_FoodItem.m_CaloriesRemaining;
                    honeyEatEndTime.Add(hoursPlayedNotPaused + honeyCureTime);
                    honeyEatAmount.Add(honeyConsumed);

                    // calculate poison percentage
                    if (honeyEatAmount.Sum() > honeySafeAmount + (int)(honeySafeAmount * 0.1f))
                    {
                        float chance = Mathf.Clamp(33.3f * (honeyEatAmount.Sum() / honeySafeAmount) - 33.3f, 0f, 100f) - poisonChanceReduction;
                        // roll for poison
                        if (Utils.RollChance(chance))
                        {
                            GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(localizedName, true, false);
                        }
                    }

                    // remove tea buff after eating another portion
                    drankHotBeverage = false;
                    poisonChanceReduction = 0f;

                    eatingHoney = false;
                }
                return;
            }

            /*
            Nuts in honey is consumed by ~300calorie portions. 
            Consuming more that 1 portion at a time leads to increasing chance of food poisoning. 
            Drinking any hot beverage between each portion reduces poison chance, but doesn't eliminate it. 
            Buff reduces poison chance by 30% if you drink after eating honey. Doesn't stack. Buff removed after eating honey again.
            Chance of poison is increasing linear: ~35% at 2 portions, ~100% at 4
             */

            // checking if drank tea
            if (!Settings.options.disableHoneyPoison)
            {
                bool hotBeverageFlag = (GameManager.GetPlayerManagerComponent().m_FoodItemEaten.name == "GEAR_CoffeeCup" ||
                    GameManager.GetPlayerManagerComponent().m_FoodItemEaten.name == "GEAR_RoseHipTea" ||
                        GameManager.GetPlayerManagerComponent().m_FoodItemEaten.name == "GEAR_ReishiTea" ||
                            GameManager.GetPlayerManagerComponent().m_FoodItemEaten.name == "GEAR_GreenTeaCup") && GameManager.GetPlayerManagerComponent().m_FoodItemEaten.m_FoodItem.IsHot();

                if (hotBeverageFlag && !drankHotBeverage && honeyEatEndTime.Count > 0)
                {
                    poisonChanceReduction = 30f;
                    drankHotBeverage = true;
                }

                // eating honey
                if (GameManager.GetHungerComponent().GetItemBeingEaten().name == "GEAR_honeyNuts")
                {
                    if (!eatingHoney)
                    {
                        System.Random r = new System.Random();
                        float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                        float randomizedCalorieAmount = honeySafeAmount + r.Next(-(int)(honeySafeAmount * .1f), (int)(honeySafeAmount * 0.1f));

                        // remove from list of consumed honey if 8 hours passed
                        for (int i = honeyEatEndTime.Count - 1; i >= 0; i--)
                        {
                            if (hoursPlayedNotPaused >= honeyEatEndTime[i])
                            {
                                honeyEatEndTime.RemoveAt(i);
                                honeyEatAmount.RemoveAt(i);
                            }
                        }

                        honeyNuts = GameManager.GetPlayerManagerComponent().m_FoodItemEaten;
                        honeyConsumed = GameManager.GetPlayerManagerComponent().m_FoodItemEatenStartingCalories;

                        // reduce consumption amount by honeySafeAmount +- 10% (so it doesn't look artificial)
                        GameManager.GetPlayerManagerComponent().m_FoodItemEatenStartingCalories = (float)Math.Ceiling(GameManager.GetPlayerManagerComponent().m_FoodItemEatenStartingCalories);
                        GameManager.GetHungerComponent().m_CaloriesToAddOverTime = randomizedCalorieAmount;

                        // info for next frame
                        localizedName = GameManager.GetPlayerManagerComponent().m_FoodItemEaten.m_LocalizedDisplayName.m_LocalizationID;

                        eatingHoney = true;
                    }
                }
            }


            /*
             Icecream causes cold damage and removes "warming up"
             */
            if (GameManager.GetHungerComponent().GetItemBeingEaten().name == "GEAR_icecreamCup" || GameManager.GetHungerComponent().GetItemBeingEaten().name == "GEAR_strawberryPlombir")
            {
                if (!eatingIcecream)
                {
                    iceCream = GameManager.GetPlayerManagerComponent().m_FoodItemEaten;
                    iceCreamLeft = GameManager.GetPlayerManagerComponent().m_FoodItemEatenStartingCalories;
                    iceCreamTotalCalories = iceCream.m_FoodItem.m_CaloriesTotal;
                    eatingIcecream = true;
                }
            }



            /*
            Pryanik has a collectible inside
            */
            if (!Settings.options.disableCollectibles)
            {
                if (GameManager.GetHungerComponent().GetItemBeingEaten().name == "GEAR_applesaucePryanik")
                {
                    if (!lucky)
                    {
                        prianik = GameManager.GetPlayerManagerComponent().m_FoodItemEaten;
                        prianikLastCalories = prianik.m_FoodItem.m_CaloriesRemaining;
                        lucky = true;
                    }
                }
            }
        }

        public override void OnLateUpdate()
        {
            // attaching bear in DP to a hook
            if (bearInDPDoSetup)
            {
                GameObject winchHook = GameObject.Find("Art/Structures/OBJ_WinchHookB_Prefab");
                if (!bearInDPDummy) bearInDPDummy = new GameObject();

                if (winchHook)
                {
                    winchHook = winchHook.transform.FindChild("OBJ_WinchHookA_Prefab").gameObject;
                    winchHook.transform.localPosition = new Vector3(-0.15f, -5.9f, -0.85f);
                    winchHook.transform.localEulerAngles = new Vector3(90f, 10f, 0f);
                    bearInDPDummy.name = "dummy";
                    bearInDPDummy.transform.SetParent(winchHook.transform);
                    bearInDPDummy.transform.localPosition = new Vector3(0f, 1.05f, 0.95f);
                    bearInDPDummy.transform.localEulerAngles = new Vector3(0f, 100f, 270f);

                }
                else
                {
                    bearInDPDoSetup = false;
                    return;
                }

                UnityEngine.Object[] bears = UnityEngine.Object.FindObjectsOfType<CollectibleCandleComponent>();


                for (int i = 0; i < bears.Length; i++)
                {
                    //if (bears[i].TryCast<Component>()?.gameObject.transform.position == Vector3.zero)
                    if (bears[i].TryCast<Component>()?.gameObject.GetComponent<GearItem>().m_BeenInPlayerInventory is false)
                    {
                        bearInDPGO = bears[i].TryCast<Component>()?.gameObject;
                        break;
                    }
                }
                if (bearInDPGO)
                {
                    Physics.IgnoreCollision(winchHook.GetComponent<Collider>(), bearInDPGO.GetComponent<Collider>(), true);
                }

                bearInDPDoSetup = false;
            }

            if (bearInDPGO)
            {

                if (bearInDPCurrentlyInspecting)
                {
                    //MelonLogger.Msg("inspecting");
                    return;
                }
                if (bearInDPGO.GetComponent<GearItem>().m_InPlayerInventory)
                {
                    bearInDPGO = null;
                    //MelonLogger.Msg("beenowned");
                    //doneWithDPBear = true;
                    return;
                }
                bearInDPGO.transform.position = bearInDPDummy.transform.position;
                bearInDPGO.transform.rotation = bearInDPDummy.transform.rotation;

            }
        }
    }
}