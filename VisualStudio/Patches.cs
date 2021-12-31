using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace FoodsByCalorieWaltz
{
    [HarmonyPatch(typeof(PlayerManager), "ProcessPickupItemInteraction")]
    public class addBearWithCard
    {


        private static readonly List<string> allCandleTiers = new List<string>
        {
            "GreenA","GreenB","GreenC","GreenD",
            "BlueA","BlueB","BlueC","BlueD",
            "PurpleA","PurpleB","PurpleC","PurpleD",
            "RedA","RedB","RedC","RedD"
        };



        private static bool Prefix(ref GearItem item, ref bool forceEquip, ref bool skipAudio)
        {

            for (int i = 0; i < allCandleTiers.Count; i++)
            {
                if (item.name == "GEAR_card" + allCandleTiers[i])
                {
                    GameManager.GetInventoryComponent().AddGear(Utils.InstantiateGearFromPrefabName("GEAR_candle" + allCandleTiers[i]).gameObject);

                    if (GameManager.GetInventoryComponent().GetNumGearWithName("GEAR_card" + allCandleTiers[i]) > 0)
                    {
                        return false;
                    }
                    return true;
                }
            }

            return true;

        }
    }
}
