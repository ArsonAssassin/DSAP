using Serilog;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DSAP.Models
{
    public class DarkSoulsOptions
    {
        public uint UpgradedWeaponsPercentage { get; set; }
        public List<String> UpgradedWeaponsAllowedInfusionTypes { get; set; } = [];
        public bool UpgradedWeaponsAdjustedLevels { get; set; }
        public uint UpgradedWeaponsMinLevel { get; set; }
        public uint UpgradedWeaponsMaxLevel { get; set; }
        public DarkSoulsOptions(Dictionary<string, object> optionsDict)
        {
            if (App.Client.Options.ContainsKey("upgraded_weapons_percentage"))
                UpgradedWeaponsPercentage = ((JsonElement)App.Client.Options["upgraded_weapons_percentage"]).GetUInt32();
            else
            {
                Log.Logger.Warning("No upgraded weapons percentage found.");
                Log.Logger.Warning("Warning: Only dsr apworld 0.0.19.1+ are compatible with this release.");
                Log.Logger.Warning("Continuing without upgrade behavior.");
            }
                

            if (App.Client.Options.ContainsKey("upgraded_weapons_adjusted_levels"))
            {
                if (((JsonElement)App.Client.Options["upgraded_weapons_adjusted_levels"]).GetUInt32() > 0)
                    UpgradedWeaponsAdjustedLevels = true;
                else
                    UpgradedWeaponsAdjustedLevels = false;
            }

            if (App.Client.Options.ContainsKey("upgraded_weapons_min_level"))
                UpgradedWeaponsMinLevel = ((JsonElement)App.Client.Options["upgraded_weapons_min_level"]).GetUInt32();

            if (App.Client.Options.ContainsKey("upgraded_weapons_max_level"))
                UpgradedWeaponsMaxLevel = ((JsonElement)App.Client.Options["upgraded_weapons_max_level"]).GetUInt32();

            if (App.Client.Options.TryGetValue("upgraded_weapons_allowed_infusions", out object allowed_infusions))
            {
                UpgradedWeaponsAllowedInfusionTypes.AddRange(JsonSerializer.Deserialize<string[]>(allowed_infusions.ToString()));
            }
        }

        public string ToString()
        {
            string result = $"[UpgradedWeaponsPercentage={UpgradedWeaponsPercentage},";
            result += $"UpgradedWeaponsAllowedInfusionTypes={UpgradedWeaponsAllowedInfusionTypes.ToString()}";
            result += $"UpgradedWeaponsAdjustedLevels={(UpgradedWeaponsAdjustedLevels ? "true" : "false")}";
            result += $"UpgradedWeaponsMinLevel={UpgradedWeaponsMinLevel},";
            result += $"UpgradedWeaponsMaxLevel ={UpgradedWeaponsMaxLevel}";
            return result;
        }
    }
}
