using System.ComponentModel.DataAnnotations;

namespace AutoMart.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; }
        public virtual ICollection<Vehicle>? Vehicles { get; set; }

    }
}
