using Microsoft.AspNetCore.Mvc;
using Task1_Middleware.Models;
using Task1_Middleware.Services;

namespace Task1_Middleware.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        [HttpGet]
        public IEnumerable<Item> GetAllItems()
        {
            return _inventoryService.GetAllItems();
        }

        [HttpPost]
        public ActionResult<Item> CreateItem(Item item)
        {
            _inventoryService.CreateItem(item);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
        }

        [HttpGet("{id}")]
        public ActionResult<Item> GetItemById(int id)
        {
            var item = _inventoryService.GetItemById(id);
            if (item == null)
            { 
                return NotFound();
            }
            return item;
        }

        [HttpPut("{id}")]
        public IActionResult UpdateItem(int id, Item updatedItem)
        {
            var result = _inventoryService.UpdateItem(id, updatedItem);
            if (!result)
                return NotFound();
            else
                return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItem(int id)
        {
            var result = _inventoryService.DeleteItem(id);
            if (!result)
                return NotFound();
            else
                return NoContent();
        }
    }
}
