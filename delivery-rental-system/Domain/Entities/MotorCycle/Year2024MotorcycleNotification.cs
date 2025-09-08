using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace delivery_rental_system.Domain.Entities;

[Table("motorcycle_year2024_notifications")]
public sealed class Year2024MotorcycleNotification
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string Identifier { get; set; }
    [Required, MaxLength(16)] public string Plate { get; set; } = null!;

    public Guid MotorcycleId { get; set; }
    public int Year { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
