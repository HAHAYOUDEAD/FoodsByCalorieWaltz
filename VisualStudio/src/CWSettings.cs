using ModSettings;

namespace CalorieWaltz
{
    internal class CWSettings : JsonModSettings
    {
        [Section("General settings")]

        [Name("Disable collectibles")]
        [Description("Disable collectible drops from Applesause Pryanik if you want a pure food mod")]
        public bool disableCollectibles = false;

        [Name("Disable lore")]
        [Description("A dozen well hidden notes, that tell a story about a man stranded on the island. He was the same as you once, but had a different kind of end. \n\nTurning this on will disable everything lore related. Requires restart to clean up spawns. Will not remove already existing items")]
        public bool disableLore = false;

        [Section("Special items settings")]

        [Name("Disable custom honey behavior")]
        [Description("By default, honey is eaten in small portions and can cause poisoning if eaten too much. Hot tea can be consumed after honey to lower the poisoning chance\n\nSetting this option to true will revert honey to normal food")]
        public bool disableHoneyPoison = false;

        /*
        [Name("Adjust latte drops behavior")]
        [Description("")]
        public bool disableLatteDrops = false;

        [Name("Disable hematogen behavior")]
        [Description("By degfault, hematogen helps to heal your condition. Setting change should apply after restart")]
        public bool disableHematogen = false;
        */

        [Section("Spawn settings")]
        
        [Name("Disable loose collectible spawns")]
        [Description("There is 1 of each bear hidden around the island. Disable this in case you want to only get bears from pryanik packages. \n\n'Disable collectibles' does not affect this to provide more options. \n\n!!! CHANGE REQUIRES GAME RESTART !!! This is because spawns are handled by ModComponent, and I only can intercept it at game launch")]
        public bool disableCollectibleSpawns = false;

        [Name("Disable special spawns")]
        [Description("There are some 'lore' spawns that may look strange, so I split most of them into a separate category. \n\nDisabled by default, I don't really like them anymore c: \n\n!!! CHANGE REQUIRES GAME RESTART !!! This is because spawns are handled by ModComponent, and I only can intercept it at game launch")]
        public bool disableSpecialSpawns = true;

        [Name("Disable easter egg spawns")]
        [Description("Disable this if you want serious experience \n\n!!! CHANGE REQUIRES GAME RESTART !!! This is because spawns are handled by ModComponent, and I only can intercept it at game launch")]
        public bool disableEasterEggSpawns = false;
        
        [Section("Calorie management")]
        
        [Name("Per-item calorie tweak")]
        [Description("Switch between per-item and general calorie management. Per-item and general are independant and will not affect each other\n\nThis option controls which method is used")]
        public bool perItemTweak = false;

        [Name("General scale")]
        [Description("Changing this will alter calorie value for all items. It's percentage based. Default = 100")]
        [Slider(25, 150, 26)]
        public int bigCalorieScale = 100;

        [Name("Applesauce Prianik")]
        [Description("Default = 300")]
        [Slider(160, 460, 16)]
        public int CALapplesaucePryanik = CWMain.DCALapplesaucePryanik;

        [Name("Banana Chips")]
        [Description("Default = 500")]
        [Slider(260, 760, 26)]
        public int CALbananaChips = CWMain.DCALbananaChips;

        [Name("Crab Snack")]
        [Description("Default = 320")]
        [Slider(160, 480, 17)]
        public int CALcrabSnack = CWMain.DCALcrabSnack;

        [Name("Fig Confiture")]
        [Description("Default = 900")]
        [Slider(400, 1200, 41)]
        public int CALfigConfiture = CWMain.DCALfigConfiture;

        [Name("Glintwein Glass")]
        [Description("Default = 200")]
        [Slider(100, 300, 11)]
        public int CALglintweinCup = CWMain.DCALglintweinCup;

        [Name("Honey Nuts")]
        [Description("Default = 3000")]
        [Slider(1500, 3000, 16)]
        public int CALhoneyNuts = CWMain.DCALhoneyNuts;

        [Name("Hot Spring Soda")]
        [Description("Default = 120")]
        [Slider(60, 200, 8)]
        public int CALhotSpringSoda = CWMain.DCALhotSpringSoda;

        [Name("Icecream Cup")]
        [Description("Default = 300")]
        [Slider(160, 460, 16)]
        public int CALicecreamCup = CWMain.DCALicecreamCup;

        [Name("Jubilee Cookies")]
        [Description("Default = 800")]
        [Slider(400, 1200, 41)]
        public int CALjubileeCookies = CWMain.DCALjubileeCookies;

        [Name("Kvass")]
        [Description("Default = 150")]
        [Slider(90, 250, 9)]
        public int CALkvass = CWMain.DCALkvass;

        [Name("Lecso")]
        [Description("Default = 300")]
        [Slider(160, 460, 16)]
        public int CALlecso = CWMain.DCALlecso;

        [Name("Marinara Mackerel")]
        [Description("Default = 250")]
        [Slider(130, 370, 13)]
        public int CALmarinaraMackerel = CWMain.DCALmarinaraMackerel;

        [Name("Mystery Cake")]
        [Description("Default = 600")]
        [Slider(300, 900, 31)]
        public int CALmysteryCake = CWMain.DCALmysteryCake;

        [Name("Nordshore Chocolate")]
        [Description("Default = 500")]
        [Slider(260, 760, 26)]
        public int CALnordShoreChocolate = CWMain.DCALnordShoreChocolate;

        [Name("Oats Bowl")]
        [Description("Default = 250")]
        [Slider(130, 370, 13)]
        public int CALoatsBowl = CWMain.DCALoatsBowl;

        [Name("Pinenut Brittle")]
        [Description("Default = 1200")]
        [Slider(600, 1400, 41)]
        public int CALpinenutBrittle = CWMain.DCALpinenutBrittle;

        [Name("Roasted Almonds")]
        [Description("Default = 240")]
        [Slider(120, 360, 13)]
        public int CALroastedAlmonds = CWMain.DCALroastedAlmonds;

        [Name("Solitude Cider")]
        [Description("Default = 180")]
        [Slider(100, 240, 8)]
        public int CALsolitudeCider = CWMain.DCALsolitudeCider;

        [Name("Strawberry Plombir")]
        [Description("Default = 200")]
        [Slider(100, 300, 11)]
        public int CALstrawberryPlombir = CWMain.DCALstrawberryPlombir;

        [Name("Tuna Pate")]
        [Description("Default = 200")]
        [Slider(100, 300, 11)]
        public int CALtunaPate = CWMain.DCALtunaPate;



        [Name("Alphabet Soup")]
        [Description("Default = 400")]
        [Slider(200, 600, 21)]
        public int CALABCSoup = CWMain.DCALABCSoup;

        [Name("Latte Drops")]
        [Description("Default = 35")]
        [Slider(15, 60, 10)]
        public int CALlatteDrops = CWMain.DCALlatteDrops;

        [Name("Hematogen")]
        [Description("Default = 150")]
        [Slider(90, 250, 9)]
        public int CALhematogen = CWMain.DCALhematogen;

        [Name("Turkish Delight")]
        [Description("Default = 900")]
        [Slider(600, 1200, 31)]
        public int CALrahatLokum = CWMain.DCALrahatLokum;


        [Section("Reset")]
        
        [Name("Reset calorie values")]
        [Description("Reset calorie values to default when pressing CONFIRM")]
        public bool resetCalories = false;


        [Section("Cheat settings")]

        [Name("Keep bears as figurines after burn out")]
        [Description("Ruined bears won't disappear. They will behave like regular ruined items though, meaning they will disappear from containers")]
        public bool keepFigurines = false;

        [Name("No damage to candles from shooting")]
        [Description("Candles can take about 3 shots from guns and 6 from arrows")]
        public bool noDamageOnShot = false;

        [Section("Debug")]

        [Name("Enable debug messages")]
        [Description("")]
        public bool debugLog = false;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(perItemTweak)) Settings.SetKeySettingsVisible((bool)newValue);


        }


        protected override void OnConfirm()
        {
            // ONLY SET WHEN CHANGING CALORIE RELATED SETTINGS!!! whatever
            //if (CWMain.currentLevel <= 2) CWMain.queueInventoryUpdate = true;

            base.OnConfirm();

            if (Settings.options.resetCalories)
            {
                Settings.options.bigCalorieScale = 100;

                Settings.options.CALapplesaucePryanik = CWMain.DCALapplesaucePryanik;
                Settings.options.CALbananaChips = CWMain.DCALbananaChips;
                Settings.options.CALcrabSnack = CWMain.DCALcrabSnack;
                Settings.options.CALfigConfiture = CWMain.DCALfigConfiture;
                Settings.options.CALglintweinCup = CWMain.DCALglintweinCup;
                Settings.options.CALhoneyNuts = CWMain.DCALhoneyNuts;
                Settings.options.CALhotSpringSoda = CWMain.DCALhotSpringSoda;
                Settings.options.CALicecreamCup = CWMain.DCALicecreamCup;
                Settings.options.CALjubileeCookies = CWMain.DCALjubileeCookies;
                Settings.options.CALkvass = CWMain.DCALkvass;
                Settings.options.CALlecso = CWMain.DCALlecso;
                Settings.options.CALmarinaraMackerel = CWMain.DCALmarinaraMackerel;
                Settings.options.CALmysteryCake = CWMain.DCALmysteryCake;
                Settings.options.CALnordShoreChocolate = CWMain.DCALnordShoreChocolate;
                Settings.options.CALoatsBowl = CWMain.DCALoatsBowl;
                Settings.options.CALpinenutBrittle = CWMain.DCALpinenutBrittle;
                Settings.options.CALroastedAlmonds = CWMain.DCALroastedAlmonds;
                Settings.options.CALsolitudeCider = CWMain.DCALsolitudeCider;
                Settings.options.CALstrawberryPlombir = CWMain.DCALstrawberryPlombir;
                Settings.options.CALtunaPate = CWMain.DCALtunaPate;

                Settings.options.CALABCSoup = CWMain.DCALABCSoup;
                Settings.options.CALlatteDrops = CWMain.DCALlatteDrops;
                Settings.options.CALhematogen = CWMain.DCALhematogen;
                Settings.options.CALrahatLokum = CWMain.DCALrahatLokum;

                Settings.options.resetCalories = false;
            }

            CWMain.UpdateFoodArray();
            CWMain.UpdateCaloriesInPrefabs();
            //if (!CWMain.queueInventoryUpdate) CWMain.UpdateCaloriesInInventory();
            CWMain.lastInspected = null;

            CWMain.SwitchPryanikTexture(!Settings.options.disableCollectibles);

        }
    }


    internal static class Settings
    {
        

        internal static CWSettings options = new CWSettings();
    
        public static void OnLoad()
        {
            options.AddToModSettings("Foods by 'Calorie Waltz' Settings");
            SetKeySettingsVisible(options.perItemTweak);
        }
        internal static void SetKeySettingsVisible(bool visible)
        {
            options.SetFieldVisible(nameof(options.bigCalorieScale), !visible);


            options.SetFieldVisible(nameof(options.CALapplesaucePryanik), visible);
            options.SetFieldVisible(nameof(options.CALbananaChips), visible);
            options.SetFieldVisible(nameof(options.CALcrabSnack), visible);
            options.SetFieldVisible(nameof(options.CALfigConfiture), visible);
            options.SetFieldVisible(nameof(options.CALglintweinCup), visible);
            options.SetFieldVisible(nameof(options.CALhoneyNuts), visible);
            options.SetFieldVisible(nameof(options.CALhotSpringSoda), visible);
            options.SetFieldVisible(nameof(options.CALicecreamCup), visible);
            options.SetFieldVisible(nameof(options.CALjubileeCookies), visible);
            options.SetFieldVisible(nameof(options.CALkvass), visible);
            options.SetFieldVisible(nameof(options.CALlecso), visible);
            options.SetFieldVisible(nameof(options.CALmarinaraMackerel), visible);
            options.SetFieldVisible(nameof(options.CALmysteryCake), visible);
            options.SetFieldVisible(nameof(options.CALnordShoreChocolate), visible);
            options.SetFieldVisible(nameof(options.CALoatsBowl), visible);
            options.SetFieldVisible(nameof(options.CALpinenutBrittle), visible);
            options.SetFieldVisible(nameof(options.CALroastedAlmonds), visible);
            options.SetFieldVisible(nameof(options.CALsolitudeCider), visible);
            options.SetFieldVisible(nameof(options.CALstrawberryPlombir), visible);
            options.SetFieldVisible(nameof(options.CALtunaPate), visible);

            options.SetFieldVisible(nameof(options.CALABCSoup), visible);
            options.SetFieldVisible(nameof(options.CALlatteDrops), visible);
            options.SetFieldVisible(nameof(options.CALhematogen), visible);
            options.SetFieldVisible(nameof(options.CALrahatLokum), visible);


        }
    }
    
    


}
