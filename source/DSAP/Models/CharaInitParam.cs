namespace DSAP.Models
{
    internal class CharaInitParam : IParam
    {
        public static uint Size { get; set; } = 0xf0;
        public static int spOffset = 0x600;
        // offsets into the Param for these fields
        public const int WEAPON_RIGHT = 0x10;
        public const int SUBWEAPON_RIGHT = 0x14;
        public const int WEAPON_LEFT = 0x18;
        public const int SUBWEAPON_LEFT = 0x1c;
        public const int EQUIP_HEAD = 0x20;
        public const int EQUIP_BODY = 0x24;
        public const int EQUIP_ARMS = 0x28;
        public const int EQUIP_LEGS = 0x2c;
        public const int ARROW = 0x30;
        public const int BOLT = 0x34;
        public const int SPELL_01 = 0x60;
        public const int ITEM_01 = 0x7c;
        public const int ITEMNUM_01 = 0xcc;
        public const int ACCESSORY_01 = 0x40;
    }
}
