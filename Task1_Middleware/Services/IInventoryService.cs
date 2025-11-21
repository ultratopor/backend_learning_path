using Task1_Middleware.Models;

namespace Task1_Middleware.Services
{
    public interface IInventoryService
    {
        Item CreateItem(Item item);
        bool DeleteItem(int id);
        IEnumerable<Item> GetAllItems();
        Item? GetItemById(int id);
        bool UpdateItem(int id, Item updatedItem);
    }
}