using Serilog;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DSAP.Models
{
    public class DarkSoulsOptions
    {
        const string curr_version = "0.0.20.2";
        public bool outofdate = false;
        public uint apiver_major;
        public uint apiver_minor;
        public uint apiver_revision;
        public uint apiver_build;

        public uint UpgradedWeaponsPercentage { get; set; }
        public List<String> UpgradedWeaponsAllowedInfusionTypes { get; set; } = [];
        public bool UpgradedWeaponsAdjustedLevels { get; set; }
        public uint UpgradedWeaponsMinLevel { get; set; }
        public uint UpgradedWeaponsMaxLevel { get; set; }
        public DarkSoulsOptions(Dictionary<string, object> optionsDict, Dictionary<string, object> slotData)
        {
            if (slotData.ContainsKey("apworld_api_version"))
            {
                string apworld_api_version = Convert.ToString(slotData["apworld_api_version"]);
                string[] substrs = apworld_api_version.Split(".");
                /* parse version string (M.m.r.b} into variables with M m r b */
                apiver_major     = uint.Parse(substrs[0]);
                apiver_minor     = uint.Parse(substrs[1]);
                apiver_revision  = uint.Parse(substrs[2]);
                apiver_build = 0;
                if (substrs.Length > 3)
                    apiver_build = uint.Parse(substrs[3]);

                string[] substrs2 = curr_version.Split(".");
                uint currmajor = uint.Parse(substrs2[0]);
                uint currminor = uint.Parse(substrs2[1]);
                uint currrevision = uint.Parse(substrs2[2]);
                uint currbuild = 0;
                if (substrs2.Length > 3)
                    currbuild = uint.Parse(substrs2[3]);

                /* While still in alpha, bumping up "revision" (or anything higher) will indicate breaking compatibility */
                /* Updating revision indicates no change. */
                if ((apiver_major > currmajor) ||
                    (apiver_major == currmajor && apiver_minor > currminor) ||
                    (apiver_major == currmajor && apiver_minor == currminor && apiver_revision > currrevision))
                {
                    Log.Logger.Error("Apworld detected that is too advanced for the DSAP client. Upgrade your client.");
                    Log.Logger.Error("Otherwise, expect errors and instability.");
                    outofdate = true;
                }
                if ((apiver_major < currmajor) ||
                    (apiver_major == currmajor && apiver_minor < currminor) ||
                    (apiver_major == currmajor && apiver_minor == currminor && apiver_revision < currrevision))
                {
                    Log.Logger.Error("Apworld detected that is too old for this version of the DSAP client.");
                    Log.Logger.Error("Otherwise, expect errors and instability.");
                    outofdate = true;
                }
                Log.Logger.Information($"Client api level {currmajor}.{currminor}.{currrevision}.{currbuild}, " +
                    $"apworld api level {apiver_major}.{apiver_minor}.{apiver_revision}.{apiver_build}");
                /* 1 =  v0.0.20 - current */
                /* 0 or null =  < v0.0.20 */
            }
            else
            {
                Log.Logger.Error("Seed generated on Apworld pre-v0.0.20.0 detected. Expect failures.");
                Log.Logger.Error("Only seeds generated with DSR apworld version 0.0.20.0+ are compatible with this client.");
                apiver_major    = 0;
                apiver_minor    = 0;
                apiver_revision = 19;
                apiver_build    = 1;
                outofdate = true;
            }


            if (App.Client.Options.ContainsKey("upgraded_weapons_percentage"))
                UpgradedWeaponsPercentage = ((JsonElement)App.Client.Options["upgraded_weapons_percentage"]).GetUInt32();
            else
            {
                Log.Logger.Warning("No 'upgraded weapons percentage' found. 'Weapon Upgrade' behavior will not occur.");
                UpgradedWeaponsPercentage = 0;
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
        
        public string VersionInfoString()
        {
            const string curr_version = "0.0.20.0";
            string[] substrs2 = curr_version.Split(".");
            uint currmajor = uint.Parse(substrs2[0]);
            uint currminor = uint.Parse(substrs2[1]);
            uint currrevision = uint.Parse(substrs2[2]);
            uint currbuild = 0;
            if (substrs2.Length > 3)
                currbuild = uint.Parse(substrs2[3]);
            return $"Client api level {currmajor}.{currminor}.{currrevision}.{currbuild}, " +
                    $"apworld api level {apiver_major}.{apiver_minor}.{apiver_revision}.{apiver_build}";
        }
        /// <summary>
        /// Compares the given checkver to the apworld version.
        /// </summary>
        /// <param name="checkver"></param>
        /// <returns>0 if equal, 1 if apworld > checkver, -1 if apworld < checkver.</returns>
        public int ApworldCompare(string checkver)
        {
            string[] substrs = checkver.Split(".");
            uint checkmajor = uint.Parse(substrs[0]);
            uint checkminor = uint.Parse(substrs[1]);
            uint checkrevision = uint.Parse(substrs[2]);
            uint checkbuild = 0;
            if (substrs.Length > 3)
                checkbuild = uint.Parse(substrs[3]);

            if (apiver_major == checkmajor && apiver_minor == checkminor && apiver_revision == checkrevision && apiver_build == checkbuild)
                return 0; /* checkver and apiver are equal */

            if ((apiver_major > checkmajor) ||
                (apiver_major == checkmajor && apiver_minor > checkminor) ||
                (apiver_major == checkmajor && apiver_minor == checkminor && apiver_revision > checkrevision) ||
                (apiver_major == checkmajor && apiver_minor == checkminor && apiver_revision == checkrevision && apiver_build > checkbuild))
            {
                return 1; /* apiver is > checkver */
            }
            else
                return -1; /* apiver is < checkver */

        }
    }
}
