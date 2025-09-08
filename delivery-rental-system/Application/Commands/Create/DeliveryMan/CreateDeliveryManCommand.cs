using MediatR;

namespace delivery_rental_system.Application.Commands.Create.DeliveryMan;

public sealed record CreateDeliveryManCommand(
    string Identificador,
    string Nome,
    string Cnpj,           
    DateTime DataNascimento,
    string NumeroCnh,       
    string TipoCnh,        
    string? ImagemCnh       
) : IRequest<Unit>;