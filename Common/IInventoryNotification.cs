namespace Common
{
    public interface IInventoryNotification
    {
        void OnInventoryChanged(InventoryChangedMessage message);
    }
}
