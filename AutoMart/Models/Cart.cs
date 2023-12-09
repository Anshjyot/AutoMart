using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMart.Models
{
    public class Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public string? UserId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual WebUser? User { get; set; }
        public DateTime DateSubmitted { get; set; }
    }
}