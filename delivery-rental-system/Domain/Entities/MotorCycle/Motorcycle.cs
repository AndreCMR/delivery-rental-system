

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Domain.Entities;

[Table("motorcycles")]
[Index(nameof(Plate), IsUnique = true)]
public sealed class Motorcycle
{

    [Key] 
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Identifier { get; set; } = null!; 

    [Required] public int Year { get; set; }

    [Required, MaxLength(120)]
    public string Model { get; set; } = null!;

    [Required, MaxLength(16)]
    public string Plate { get; set; } = null!;

    [Required] public DateTime CreatedAt { get; set; }

    [InverseProperty(nameof(Rental.Rental.Motorcycle))]
    public ICollection<Rental.Rental> Rentals { get; set; } = new List<Rental.Rental>();

    private Motorcycle() { }

    public Motorcycle(string identifier, int year, string model, string plate)
    {
        Identifier = identifier;
        Year = year;
        Model = model;
        Plate = plate;
        CreatedAt = DateTime.UtcNow;
    }

    public bool UpdatePlate(string newPlate)
    {
        if (string.IsNullOrWhiteSpace(newPlate))
            return false;

        Plate = newPlate.ToUpper();

        return true;
    }
}

