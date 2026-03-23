using System.Text.Json.Serialization;
using static DSAP.Enums;

namespace DSAP.Models
{
    public class Loadout
    {
        public string Name {  get; set; }
        public int Id { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DsrLoadoutType Type { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Faith { get; set; }
        // the below fields are item lot ids, for where this stuff is found in asylum
        public int RightWeapon { get; set; }
        public int SubRightWeapon { get; set; }
        public int LeftWeapon { get; set; }
        public int SubLeftWeapon { get; set; }
        public int Ammunition { get; set; }

        public Loadout()
        {
        }
    }
}
