namespace DSAP.Models
{
    public class Gift
    {
        public byte Quantity { get; set; }
        public string DisplayName { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public Gift() 
        {
            Quantity = 1;
            DisplayName = "";
            ItemName = "";
            Description = "";
        }
    }
}
