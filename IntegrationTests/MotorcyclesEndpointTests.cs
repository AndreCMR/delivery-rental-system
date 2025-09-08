using delivery_rental_system.Application.Queries.Motorcycles;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Domain.Enums;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Presentation.Responses;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using static IntegrationTests.PriorityOrderer;

namespace IntegrationTests;

[TestCaseOrderer("IntegrationTests.PriorityOrderer", "AssemblyDeTestes")]
public class MotorcyclesEndpointTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public MotorcyclesEndpointTests(CustomWebAppFactory factory) => _factory = factory;


    [Fact, TestPriority(1)]
    public async Task Post_should_create_and_publish_event_and_persist_notification_when_2024()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            identificador = "moto-integ-1",
            ano = 2024,
            modelo = "Mottu Sport",
            placa = "INX-2024"
        };

        var response = await client.PostAsJsonAsync("/motos", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();
        (await harness.Published.Any<delivery_rental_system.Domain.Events.MotorcycleCreated>()).Should().BeTrue();
        (await harness.Consumed.Any<delivery_rental_system.Domain.Events.MotorcycleCreated>()).Should().BeTrue();

        var db = scope.ServiceProvider.GetRequiredService<delivery_rental_system.Infra.Persistence.AppDbContext>();
        var notif = await db.Year2024MotorcycleNotifications.FirstOrDefaultAsync(x => x.Plate == "INX-2024");
        notif.Should().NotBeNull();
        notif!.Year.Should().Be(2024);
    }

    [Fact, TestPriority(2)]
    public async Task Post_should_return_400_when_plate_duplicates()
    {
        var client = _factory.CreateClient();

        var ok = await client.PostAsJsonAsync("/motos", new { identificador = "m1", ano = 2023, modelo = "X", placa = "DUP-001" });
        ok.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicade = await client.PostAsJsonAsync("/motos", new { identificador = "m2", ano = 2023, modelo = "Y", placa = "DUP-001" });
        duplicade.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await duplicade.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(3)]
    public async Task Patch_ShouldReturn200AndSuccessMessage_WhenPlateIsUpdated()
    {
        var client = _factory.CreateClient();

        var create = new { identificador = "moto-integ-placa-1", ano = 2024, modelo = "Mottu Sport", placa = "INT-2024" };

        var createdResp = await client.PostAsJsonAsync("/motos", create);
        createdResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var motosResp = await client.GetFromJsonAsync<List<MotorcycleDto>>("/motos?placa=INT-2024");
        var created = motosResp!.Single();
        var id = created!.Identifier;

        var update = new { placa = "INT-2025" };
        var patch = await client.PutAsJsonAsync($"/motos/{id}/placa", update);

        patch.StatusCode.Should().Be(HttpStatusCode.OK);

        var okBody = await patch.Content.ReadFromJsonAsync<ApiMessageResponse>();
        okBody!.Mensagem.Should().Be("Placa modificada com sucesso");
    }

    [Fact, TestPriority(4)]
    public async Task Patch_ShouldReturn400AndInvalidMessage_WhenPlateIsDuplicated()
    {
        var client = _factory.CreateClient();

        var m1 = new { identificador = "moto-integ-placa-2", ano = 2023, modelo = "Honda", placa = "AAA1A11" };
        var m2 = new { identificador = "moto-integ-placa-3", ano = 2024, modelo = "Yamaha", placa = "BBB2B22" };

        var r1 = await client.PostAsJsonAsync("/motos", m1); r1.StatusCode.Should().Be(HttpStatusCode.Created);
        var r2 = await client.PostAsJsonAsync("/motos", m2); r2.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var motosResp = await client.GetFromJsonAsync<List<MotorcycleDto>>("/motos?placa=BBB2B22");
        var moto2 = motosResp!.Single();
        var id = moto2!.Identifier;

        var patch = await client.PutAsJsonAsync($"/motos/{id}/placa", new { placa = "AAA1A11" });

        patch.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await patch.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(5)]
    public async Task Delete_ShouldReturn200WithoutBody_WhenMotorcycleExists()
    {
        var client = _factory.CreateClient();

        var createPayload = new
        {
            identificador = "moto-integ-del-1",
            ano = 2024,
            modelo = "Mottu Sport",
            placa = "DEL-200"
        };

        var createdResp = await client.PostAsJsonAsync("/motos", createPayload);
        createdResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var motosResp = await client.GetFromJsonAsync<List<MotorcycleDto>>("/motos?placa=DEL-200");
        var created = motosResp!.Single();

        var deleteResp = await client.DeleteAsync($"/motos/{created!.Identifier}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await deleteResp.Content.ReadAsStringAsync();
        text.Should().BeNullOrWhiteSpace();
    }

    [Fact, TestPriority(6)]
    public async Task Delete_ShouldReturn400WithInvalidMessage_WhenMotorcycleNotFound()
    {
        var client = _factory.CreateClient();

        var deleteResp = await client.DeleteAsync($"/motos/{Guid.NewGuid()}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await deleteResp.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(7)]
    public async Task Delete_ShouldReturn400WithInvalidMessage_WhenDeletingTwice()
    {
        var client = _factory.CreateClient();

        var createPayload = new
        {
            identificador = "moto-integ-del-2",
            ano = 2023,
            modelo = "Honda",
            placa = "DEL-400"
        };

        var createdResp = await client.PostAsJsonAsync("/motos", createPayload);
        createdResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var motosResp = await client.GetFromJsonAsync<List<MotorcycleDto>>("/motos?placa=DEL-400");
        var created = motosResp!.Single();

        var firstDelete = await client.DeleteAsync($"/motos/{created!.Identifier}");
        firstDelete.StatusCode.Should().Be(HttpStatusCode.OK);
        (await firstDelete.Content.ReadAsStringAsync()).Should().BeNullOrWhiteSpace();

        var secondDelete = await client.DeleteAsync($"/motos/{created!.Identifier}");
        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await secondDelete.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact, TestPriority(8)]
    public async Task Delete_ShouldReturn400_WhenMotorcycleHasRentals()
    {
        var client = _factory.CreateClient();

        var createPayload = new
        {
            identificador = "moto-integ-del-2",
            ano = 2024,
            modelo = "Mottu Sport",
            placa = "DEL-409"
        };
        var createdResp = await client.PostAsJsonAsync("/motos", createPayload);
        createdResp.StatusCode.Should().Be(HttpStatusCode.Created);

        Motorcycle moto;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            moto = await db.Motorcycles
                .Where(d => d.Plate == "DEL-409")
                .SingleAsync();
        }

        var entregadorPayload = new
        {
            identificador = "ent-int-del-1",
            nome = "Entregador Delete",
            cnpj = "33.333.333/0001-90",
            data_nascimento = "1990-01-01",
            numero_cnh = "12345678901",
            tipo_cnh = "A"
        };
        var entResp = await client.PostAsJsonAsync("/entregadores", entregadorPayload);
        entResp.EnsureSuccessStatusCode();

        int deliveryManId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            deliveryManId = await db.DeliveryMan
                .Where(d => d.Cnpj == "33333333000190") 
                .Select(d => d.Id)
                .SingleAsync();
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rentalIdentifier = $"locacao{Random.Shared.Next(100, 999)}";

            var inicio = new DateTime(2025, 10, 09);
            var termino = new DateTime(2025, 10, 16, 23, 59, 59);
            var previsao = new DateTime(2025, 10, 16, 23, 59, 59);

            var rental = new Rental(
                identifier: rentalIdentifier,
                deliveryManId: deliveryManId,
                motorcycleId: moto.Id,
                plan: RentalPlan.Days7,
                dataInicio: inicio,
                dataTermino: termino,
                dataPrevisaoTermino: previsao,
                nowUtc: DateTime.UtcNow
            );

            await db.Rental.AddAsync(rental);
            await db.SaveChangesAsync();
        }

        var deleteResp = await client.DeleteAsync($"/motos/{moto.Identifier}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await deleteResp.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }


}

