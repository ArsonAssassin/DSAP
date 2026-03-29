namespace DSAP.Models
{
    public class ThiefItem
    {
        public byte Quantity { get; set; }
        public string ItemName { get; set; }
        public ThiefItem() 
        {
            Quantity = 1;
            ItemName = "";
        }
    }
}
