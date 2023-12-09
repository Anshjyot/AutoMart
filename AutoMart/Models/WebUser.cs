using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;



namespace AutoMart.Models
{
    public class WebUser : IdentityUser
    {
        public virtual ICollection<Feedback>? Feedbacks { get; set; }

        public virtual ICollection<Vehicle>? Vehicles { get; set; }

        public virtual ICollection<Cart>? Carts { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
