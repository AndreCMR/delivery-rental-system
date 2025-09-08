namespace delivery_rental_system.Presentation.Requests;

public sealed record CreateMotorcycleRequest(
string Identificador,
int Ano,
string Modelo,
string Placa);
