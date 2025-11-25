using System.ComponentModel.DataAnnotations;

namespace Task1_Middleware.Models
{
    
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Range(0.01,double.MaxValue)]
        public decimal Price { get; set; }
    }
}
