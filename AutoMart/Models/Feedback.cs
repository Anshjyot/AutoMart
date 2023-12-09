using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AutoMart.Models
{
    public class Feedback
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "The content of the review is mandatory!")]
        public string ReviewText { get; set; }

        [Required(ErrorMessage = "Rating is mandatory!")]
        [Range(1, 5, ErrorMessage = "Rating must be a natural number between 1 and 5!")]
        public int Rating { get; set; }

        public string? UserId { get; set; }
        public virtual WebUser? User { get; set; }

        public int? VehicleId { get; set; }
        public virtual Vehicle? Vehicles { get; set; }

        public DateTime DateSubmitted { get; set; }
    }
}
