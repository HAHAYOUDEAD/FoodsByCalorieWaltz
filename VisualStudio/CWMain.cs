using System;
using System.IO;
using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


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


        // calorie tweak
        public static object[][] allFoodItems = new object[20][];

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

        public static bool loadedCalories;

        public static GearItem lastInspected;

        public static bool queueInventoryUpdate;

        // Misc parameters
        public static string modsPath;

        public static bool isLoaded;

        public static int currentLevel;


        // opened cans

        private static bool injectedMeshSwapComponent;
        private static List<string> cannedGear = new List<string>();



        public override void OnApplicationStart()
        {
            // Get Mods folder path
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\Mods");

            // Load settings
            Settings.OnLoad();
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

                


            if (!loadedCookingTex) // adding pot cooking textures
            {
                cookableGear.Add("glintwein"); // case-sensitive
                cookableGear.Add("lecso");
                cookableGear.Add("nannasOats");

                Material potMat;
                GameObject potGear;

                for (int i = 0; i < cookableGear.Count; i++)
                {
                    //potMat = new Material(Shader.Find("Shader Forge/TLD_Food_Liquid"));
                    potGear = Resources.Load("GEAR_" + cookableGear[i]).TryCast<GameObject>();
                    if (potGear == null) continue;

                    potMat = new Material(Resources.Load("GEAR_CoffeeCup").TryCast<GameObject>().GetComponent<Cookable>().m_CookingPotMaterialsList[0]);
                    potMat.name = ("CKN_" + cookableGear[i] + "_MAT");
                    potMat.mainTexture = potGear.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture;

                    potGear.GetComponent<Cookable>().m_CookingPotMaterialsList = new Material[1] { potMat };
                }

                loadedCookingTex = true;
            }


            if (!injectedMeshSwapComponent)
            {
                cannedGear.Add("lecso"); // case-sensitive

                GameObject gear;

                for (int i = 0; i < cannedGear.Count; i++)
                {
                    gear = Resources.Load("GEAR_" + cannedGear[i]).TryCast<GameObject>();
                    if (cannedGear == null) continue;

                    gear.AddComponent<MeshSwapItem>();
                    gear.GetComponent<MeshSwapItem>().m_GearItem = gear.GetComponent<GearItem>();
                    gear.GetComponent<MeshSwapItem>().m_MeshObjOpened = gear.transform.FindChild(cannedGear[i] + "_opened").gameObject;
                    gear.GetComponent<MeshSwapItem>().m_MeshObjUnopened = gear.transform.FindChild(cannedGear[i]).gameObject;
                }

                injectedMeshSwapComponent = true;
            }
        }

        public override void OnSceneWasUnloaded(int level, string name)
        {
            isLoaded = false;
        }

        public static void UpdateFoodArray()
        {
            allFoodItems[0] = new object[] { "GEAR_applesaucePryanik", Settings.options.perItemTweak ? Settings.options.CALapplesaucePryanik : (DCALapplesaucePryanik * Settings.options.bigCalorieScale / 100)};
            allFoodItems[1] = new object[] { "GEAR_bananaChips", Settings.options.perItemTweak ? Settings.options.CALbananaChips : (DCALbananaChips * Settings.options.bigCalorieScale / 100) };
            allFoodItems[2] = new object[] { "GEAR_crabSnack", Settings.options.perItemTweak ? Settings.options.CALcrabSnack : (DCALcrabSnack * Settings.options.bigCalorieScale / 100) };
            allFoodItems[3] = new object[] { "GEAR_figConfiture", Settings.options.perItemTweak ? Settings.options.CALfigConfiture : (DCALfigConfiture * Settings.options.bigCalorieScale / 100) };
            allFoodItems[4] = new object[] { "GEAR_glintweinCup", Settings.options.perItemTweak ? Settings.options.CALglintweinCup : (DCALglintweinCup * Settings.options.bigCalorieScale / 100) };
            allFoodItems[5] = new object[] { "GEAR_honeyNuts", Settings.options.perItemTweak ? Settings.options.CALhoneyNuts : (DCALhoneyNuts * Settings.options.bigCalorieScale / 100) };
            allFoodItems[6] = new object[] { "GEAR_hotSpringSoda", Settings.options.perItemTweak ? Settings.options.CALhotSpringSoda : (DCALhotSpringSoda * Settings.options.bigCalorieScale / 100) };
            allFoodItems[7] = new object[] { "GEAR_icecreamCup", Settings.options.perItemTweak ? Settings.options.CALicecreamCup : (DCALicecreamCup * Settings.options.bigCalorieScale / 100) };
            allFoodItems[8] = new object[] { "GEAR_jubileeCookies", Settings.options.perItemTweak ? Settings.options.CALjubileeCookies : (DCALjubileeCookies * Settings.options.bigCalorieScale / 100) };
            allFoodItems[9] = new object[] { "GEAR_kvass", Settings.options.perItemTweak ? Settings.options.CALkvass : (DCALkvass * Settings.options.bigCalorieScale / 100) };
            allFoodItems[10] = new object[] { "GEAR_lecso", Settings.options.perItemTweak ? Settings.options.CALlecso : (DCALlecso * Settings.options.bigCalorieScale / 100) };
            allFoodItems[11] = new object[] { "GEAR_marinaraMackerel", Settings.options.perItemTweak ? Settings.options.CALmarinaraMackerel : (DCALmarinaraMackerel * Settings.options.bigCalorieScale / 100) };
            allFoodItems[12] = new object[] { "GEAR_mysteryCake", Settings.options.perItemTweak ? Settings.options.CALmysteryCake : (DCALmysteryCake * Settings.options.bigCalorieScale / 100) };
            allFoodItems[13] = new object[] { "GEAR_nordShoreChocolate", Settings.options.perItemTweak ? Settings.options.CALnordShoreChocolate : (DCALnordShoreChocolate * Settings.options.bigCalorieScale / 100) };
            allFoodItems[14] = new object[] { "GEAR_oatsBowl", Settings.options.perItemTweak ? Settings.options.CALoatsBowl : (DCALoatsBowl * Settings.options.bigCalorieScale / 100) };
            allFoodItems[15] = new object[] { "GEAR_pinenutBrittle", Settings.options.perItemTweak ? Settings.options.CALpinenutBrittle : (DCALpinenutBrittle * Settings.options.bigCalorieScale / 100) };
            allFoodItems[16] = new object[] { "GEAR_roastedAlmonds", Settings.options.perItemTweak ? Settings.options.CALroastedAlmonds : (DCALroastedAlmonds * Settings.options.bigCalorieScale / 100) };
            allFoodItems[17] = new object[] { "GEAR_solitudeCider", Settings.options.perItemTweak ? Settings.options.CALsolitudeCider : (DCALsolitudeCider * Settings.options.bigCalorieScale / 100) };
            allFoodItems[18] = new object[] { "GEAR_strawberryPlombir", Settings.options.perItemTweak ? Settings.options.CALstrawberryPlombir : (DCALstrawberryPlombir * Settings.options.bigCalorieScale / 100) };
            allFoodItems[19] = new object[] { "GEAR_tunaPate", Settings.options.perItemTweak ? Settings.options.CALtunaPate : (DCALtunaPate * Settings.options.bigCalorieScale / 100) };
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

                    if (prianik != null) return; // only drop collectible when fully eaten


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
                        lucky = true;
                    }
                }
            }
        }
    }
}