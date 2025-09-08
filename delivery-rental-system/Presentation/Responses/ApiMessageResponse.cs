namespace delivery_rental_system.Presentation.Responses;
public sealed class ApiMessageResponse
{
    public string Mensagem { get; init; }

    public ApiMessageResponse(string mensagem) => Mensagem = mensagem;
}
