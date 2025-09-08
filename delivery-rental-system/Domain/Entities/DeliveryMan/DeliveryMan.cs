using delivery_rental_system.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace delivery_rental_system.Domain.Entities;


[Table("delivery_drivers")]
[Index(nameof(Cnpj), IsUnique = true)]
[Index(nameof(CnhNumero), IsUnique = true)]
public sealed class DeliveryMan
{
    [Key]
    public int Id { get; private set; }

    [Required, StringLength(64)]
    public string Identifier { get; private set; } = null!;

    [Required, StringLength(120)]
    public string Nome { get; private set; } = null!;

    [Required, StringLength(14)]
    public string Cnpj { get; private set; } = null!;

    [Required]
    public DateTime DataNascimento { get; private set; }

    [Required, StringLength(11)]
    public string CnhNumero { get; private set; } = null!;

    [Required]
    public CnhEnum CnhEnum { get; private set; }

    [StringLength(2048)]
    public string? CnhImagemUrl { get; private set; }

    [Required]
    public DateTime CreatedAt { get; private set; }

    [InverseProperty(nameof(Rental.Rental.DeliveryMan))]
    public ICollection<Rental.Rental> Rentals { get; set; } = new List<Rental.Rental>();

    private DeliveryMan() { }

    public DeliveryMan(
        string identifier, string nome, string cnpj, DateTime dataNascimento,
        string cnhNumero, CnhEnum cnhEnum, string? cnhImagemUrl)
    {
        Identifier = identifier.Trim();
        Nome = nome.Trim();
        Cnpj = cnpj.Trim();
        DataNascimento = dataNascimento;
        CnhNumero = cnhNumero.Trim();
        CnhEnum = cnhEnum;
        CnhImagemUrl = cnhImagemUrl;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCnhImageUrl(string? url)
    {
        CnhImagemUrl = url;
    }
}