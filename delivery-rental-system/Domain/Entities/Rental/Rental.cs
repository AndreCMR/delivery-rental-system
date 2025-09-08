using delivery_rental_system.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace delivery_rental_system.Domain.Entities.Rental;

[Table("rentals")]
[Index(nameof(MotorcycleId), nameof(StartDate), nameof(PredictedEndDate))]
[Index(nameof(DeliveryManId), nameof(StartDate))]
public sealed class Rental
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Identifier { get; set; } = null!;

    [Required]
    public int DeliveryManId { get; set; }

    [ForeignKey(nameof(DeliveryManId))]
    [InverseProperty(nameof(DeliveryMan.Rentals))]
    public DeliveryMan DeliveryMan { get; set; }

    [Required]
    public int MotorcycleId { get; set; }

    [ForeignKey(nameof(MotorcycleId))]
    [InverseProperty(nameof(Motorcycle.Rentals))]
    public Motorcycle Motorcycle { get; set; }

    [Required]
    public RentalPlan Plan { get; set; } 

    [Required]
    public DateTime CreatedAt { get; set; } 

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime PredictedEndDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public decimal RentalValue { get; private set; }

    [Required]
    public bool Active { get; set; } = true;

    private Rental() { }

    public Rental(
      string identifier,
      int deliveryManId,
      int motorcycleId,
      RentalPlan plan,
      DateTime dataInicio,
      DateTime dataTermino,
      DateTime dataPrevisaoTermino,
      DateTime nowUtc)
    {
        Identifier = identifier;
        DeliveryManId = deliveryManId;
        MotorcycleId = motorcycleId;
        Plan = plan;

        CreatedAt = nowUtc;
        StartDate = dataInicio;
        PredictedEndDate = dataPrevisaoTermino;
        EndDate = dataTermino;
        Active = true;
    }


    public void SetReturnDate(DateTime returnDate)
    {
        EndDate = returnDate;
    }

    public void SetRentalValue(decimal value)
    {
        RentalValue = value;
    }

    public static (int Days, decimal DailyRate) GetPlanInfo(RentalPlan plan)
    {
        return plan switch
        {
            RentalPlan.Days7 => (7, 30m),
            RentalPlan.Days15 => (15, 28m),
            RentalPlan.Days30 => (30, 22m),
            RentalPlan.Days45 => (45, 20m),
            RentalPlan.Days50 => (50, 18m),
            _ => throw new ArgumentOutOfRangeException(nameof(plan), "Invalid rental plan")
        };
    }
}