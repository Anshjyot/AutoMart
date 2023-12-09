using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMart.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle name is required!")]
        [StringLength(50, ErrorMessage = "The title cannot have more than 50 characters!")]
        [MinLength(5, ErrorMessage = "The title must have more than 5 characters!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vehicle description is required!")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "Vehicle picture is required!")]
        public string? Photo { get; set; }

        [Required(ErrorMessage = "Vehicle price is required!")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Vehicle category is required!")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public float? Rating { get; set; }

        public string? UserId { get; set; }

        public bool Approved { get; set; }
        public virtual WebUser? User { get; set; }

        public virtual ICollection<Feedback>? Feedbacks { get; set; }

        public virtual ICollection<Cart>? Carts { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
