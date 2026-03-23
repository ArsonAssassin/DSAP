namespace DSAP.Models
{
    internal class MagicParam : IParam
    {
        public static uint Size { get; set; } = 0x30;
        public static int spOffset = 0x408;
        // offsets into the Param for these fields
        public const int Int_Requirement = 0x1e;
        public const int Faith_Requirement = 0x1f;
        public const int VOW_00_07 = 0x2a;
        public const int VOW_08_15 = 0x2d;
    }
}
