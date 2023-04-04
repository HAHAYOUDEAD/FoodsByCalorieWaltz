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
                    MelonLogger.Msg(CC.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableSpecialSpawns && fileName == "specials")
                {
                    MelonLogger.Msg(CC.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableEasterEggSpawns && fileName == "funiBanana")
                {
                    MelonLogger.Msg(CC.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }

                if (Settings.options.disableLore && fileName == "notes")
                {
                    MelonLogger.Msg(CC.DarkYellow, "Skipping based on settings: " + fileName);
                    text = "";
                }
            }

            return true;
        } 
    }

    [HarmonyPatch(typeof(MeshSwapItem), nameof(MeshSwapItem.Update))] // random texture for ABC soup
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


                    Material liquid = CWMain.InstantiateLiquidMaterial();
                    currentSoupMaterial.shader = liquid.shader;
                    currentSoupMaterial.CopyPropertiesFromMaterial(liquid);
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
}
