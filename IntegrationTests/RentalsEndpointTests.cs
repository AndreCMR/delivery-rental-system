

using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Presentation.Responses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using static IntegrationTests.PriorityOrderer;

namespace IntegrationTests;

[TestCaseOrderer("IntegrationTests.PriorityOrderer", "AssemblyDeTestes")]
public class RentalsEndpointTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;
    public RentalsEndpointTests(CustomWebAppFactory factory) => _factory = factory;
    
    private static (DateTime inicio, DateTime previsto, DateTime termino) BuildDatesForPlan(int plano)
    {
        var hojeUtc = DateTime.UtcNow.Date;
        var inicio = hojeUtc.AddDays(1);
        var previsto = inicio.AddDays(plano - 1);
        var termino = previsto;
        return (
            DateTime.SpecifyKind(inicio, DateTimeKind.Utc),
            DateTime.SpecifyKind(previsto, DateTimeKind.Utc),
            DateTime.SpecifyKind(termino, DateTimeKind.Utc)
        );
    }

    private static async Task<string> CreateDeliveryManAsync(
        HttpClient client,
        string identificador,
        string cnpj,
        string numeroCnh,
        string tipoCnh,
        IServiceProvider services)
    {
        var payload = new
        {
            identificador,
            nome = "Entregador " + identificador,
            cnpj,
            data_nascimento = "1990-01-01",
            numero_cnh = numeroCnh,
            tipo_cnh = tipoCnh,
            imagem_cnh = (string?)null
        };

        var resp = await client.PostAsJsonAsync("/entregadores", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
            .AsNoTracking()
            .FirstAsync(x => x.Identifier == identificador);

        return entity.Identifier;
    }

    private static async Task<string> CreateMotorcycleAsync(
        HttpClient client,
        string identificador,
        int ano,
        string modelo,
        string placa,
        IServiceProvider services)
    {
        var payload = new
        {
            identificador,
            ano,
            modelo,
            placa
        };

        var resp = await client.PostAsJsonAsync("/motos", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Set<delivery_rental_system.Domain.Entities.Motorcycle>()
            .AsNoTracking()
            .FirstAsync(x => x.Identifier == identificador);

        return entity.Identifier;
    }



    [Fact, TestPriority(1)]
    public async Task Post_should_create_rental_and_persist_fields()
    {
        var client = _factory.CreateClient();

        var entregadorId = await CreateDeliveryManAsync(client, "ent-rent-1", "12.345.678/0001-90", "12345678901", "A", _factory.Services);

        var motoId = await CreateMotorcycleAsync( client, "moto-rent-1", 2024, "Mottu Sport", "REN-001", _factory.Services);

        const int plano = 7;
        var (inicio, previsto, termino) = BuildDatesForPlan(plano);

        var payload = new
        {
            entregador_id = entregadorId,
            moto_id = motoId,
            data_inicio = inicio.ToString("O"),
            data_termino = termino.ToString("O"),
            data_previsao_termino = previsto.ToString("O"),
            plano
        };

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var response = await client.PostAsJsonAsync("/locacoes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var text = await response.Content.ReadAsStringAsync();
        text.Should().BeNullOrWhiteSpace();

        var delivery = await db.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
            .AsNoTracking()
            .FirstAsync(x => x.Identifier == entregadorId);

        var moto = await db.Set<delivery_rental_system.Domain.Entities.Motorcycle>()
            .AsNoTracking()
            .FirstAsync(x => x.Identifier == motoId);

        var rental = await db.Set<Rental>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.DeliveryManId == delivery.Id &&
                r.MotorcycleId == moto.Id &&
                r.StartDate == inicio);

        rental.Should().NotBeNull();
        rental!.Active.Should().BeTrue();
        ((int)rental.Plan).Should().Be(plano);
        rental.PredictedEndDate.Date.Should().Be(previsto.Date);
        rental.EndDate.Should().Be(termino.Date);
    }

    [Fact, TestPriority(2)]
    public async Task Post_should_return_400_when_deliveryman_not_category_A()
    {
        var client = _factory.CreateClient();

        var entregadorId = await CreateDeliveryManAsync(client, "ent-rent-1", "11.111.111/0001-11", "99999999999", "B", _factory.Services);

        var motoId = await CreateMotorcycleAsync(client, "moto-rent-1", 2024, "Mottu City", "REN-002", _factory.Services);

        const int plano = 7;
        var (inicio, previsto, termino) = BuildDatesForPlan(plano);

        var payload = new
        {
            entregador_id = entregadorId,
            moto_id = motoId,
            data_inicio = inicio.ToString("O"),
            data_termino = termino.ToString("O"),
            data_previsao_termino = previsto.ToString("O"),
            plano
        };

        // Act
        var response = await client.PostAsJsonAsync("/locacoes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(3)]
    public async Task Post_should_return_400_when_overlapping_period()
    {
        var client = _factory.CreateClient();

        var entregadorId = await CreateDeliveryManAsync(client, "ent-rent-1", "22.222.222/0001-22", "11122233344", "A", _factory.Services);

        var motoId = await CreateMotorcycleAsync(client, "moto-rent-1", 2024, "Mottu Pro", "REN-003", _factory.Services);

        const int plano = 15;
        var (inicio, previsto, termino) = BuildDatesForPlan(plano);

        var payload = new
        {
            entregador_id = entregadorId,
            moto_id = motoId,
            data_inicio = inicio.ToString("O"),
            data_termino = termino.ToString("O"),
            data_previsao_termino = previsto.ToString("O"),
            plano
        };

        var resp1 = await client.PostAsJsonAsync("/locacoes", payload);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        var resp2 = await client.PostAsJsonAsync("/locacoes", payload);
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp2.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(4)]
    public async Task Post_should_return_400_when_dates_are_inconsistent()
    {
        var client = _factory.CreateClient();

        var entregadorId = await CreateDeliveryManAsync(client, "ent-rent-1", "33.333.333/0001-33", "12312312300", "A", _factory.Services);

        var motoId = await CreateMotorcycleAsync(client, "moto-rent-1", 2024, "Mottu Max", "REN-004", _factory.Services);

        const int plano = 7;
        var (inicio, previsto, termino) = BuildDatesForPlan(plano);

        var inicioInvalido = DateTime.UtcNow.Date;

        var payload = new
        {
            entregador_id = entregadorId,
            moto_id = motoId,
            data_inicio = inicioInvalido.ToString("O"),
            data_termino = termino.ToString("O"),
            data_previsao_termino = previsto.ToString("O"),
            plano
        };

        var response = await client.PostAsJsonAsync("/locacoes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body!.Mensagem.Should().Be("Dados inválidos");
    }
}