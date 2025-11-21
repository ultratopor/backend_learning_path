using Task1_Middleware.Models;

namespace Task1_Middleware.Services
{
    public class InventoryService(List<Item> items) : IInventoryService
    {
        private List<Item> _items = items;

        public Item CreateItem(Item item) // странный метод и называется странно
        {
            var maxId = _items.Count != 0 ? _items.Max(i => i.Id) : 0;
            item.Id = maxId + 1;
            _items.Add(item);
            return item;
        }

        public bool DeleteItem(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return false;
            }
            _items.Remove(item);
            return true;
        }

        public IEnumerable<Item> GetAllItems()
        {
            return _items;
        }

        public Item? GetItemById(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            
            return item;
        }

        public bool UpdateItem(int id, Item updatedItem)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return false;
            }
            item.Name = updatedItem.Name;
            item.Price = updatedItem.Price;
            return true;
        }
    }
}
