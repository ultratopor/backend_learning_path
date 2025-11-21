using Microsoft.AspNetCore.Mvc;
using Task1_Middleware.Models;
using System.Linq;

namespace Task1_Middleware.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController(List<Item> items) : ControllerBase
    {
        private List<Item> _items = items;

        [HttpGet]
        public IEnumerable<Item> GetAllItems()
        {
            return _items;
        }

        [HttpPost]
        public ActionResult<Item> CreateItem(Item item)
        {
            var maxId = _items.Count != 0 ? _items.Max(i => i.Id) : 0;
            item.Id = maxId + 1;
            _items.Add(item);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
        }

        [HttpGet("{id}")]
        public ActionResult<Item> GetItemById(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        [HttpPut("{id}")]
        public IActionResult UpdateItem(int id, Item updatedItem)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            item.Name = updatedItem.Name;
            item.Price = updatedItem.Price;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItem(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            _items.Remove(item);
            return NoContent();
        }
    }
}
