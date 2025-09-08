namespace delivery_rental_system.Domain.Events;

public sealed record MotorcycleCreated(
string Identifier,
int Year,
string Model,
string Plate,
DateTime OccurredAtUtc);

