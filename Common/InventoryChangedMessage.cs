namespace Common
{
    public class InventoryChangedMessage
    {
        public string EntityName { get; set; }
        public string ChangeType { get; set; } // e.g., Added, Updated, Deleted
        public string EntityId { get; set; }
        public string Details { get; set; } // Optional: JSON or other details
    }
}
