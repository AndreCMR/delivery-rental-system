using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Presentation.Responses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;


public class DeliveryMenEndpointTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public DeliveryMenEndpointTests(CustomWebAppFactory factory) => _factory = factory;

    private static string BuildTinyPngBase64()
    {
        var pngBytes = new byte[]
        {
            0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,
            0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
            0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,
            0x08,0x02,0x00,0x00,0x00,0x90,0x77,0x53,
            0xDE,0x00,0x00,0x00,0x0A,0x49,0x44,0x41,
            0x54,0x78,0xDA,0x63,0xF8,0xCF,0xC0,0x00,
            0x00,0x03,0x01,0x01,0x00,0x18,0xDD,0x8D,
            0x5C,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,
            0x44,0xAE,0x42,0x60,0x82
        };
        return Convert.ToBase64String(pngBytes);
    }

    [Fact]
    public async Task Post_should_create_persist_normalized_fields_and_store_image()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            identificador = "ent-int-1",
            nome = "João Entregador",
            cnpj = "12.345.678/0001-90",
            data_nascimento = "1995-05-10",
            numero_cnh = "123 456 789 01",
            tipo_cnh = "A",
            imagem_cnh = BuildTinyPngBase64()
        };

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = (TestFileStorage)scope.ServiceProvider.GetRequiredService<IFileStorage>();
        storage.Clear();

        var response = await client.PostAsJsonAsync("/entregadores", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);     

        var entity = await db.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identifier == "ent-int-1");

        entity.Should().NotBeNull();
        entity!.Nome.Should().Be("João Entregador");

        entity.Cnpj.Should().Be("12345678000190");
        entity.CnhNumero.Should().Be("12345678901");

        entity.CnhImagemUrl.Should().NotBeNullOrWhiteSpace();
        entity.CnhImagemUrl!.Should().StartWith("https://test-storage/cnh_");
        entity.CnhImagemUrl.Should().EndWith(".png");

        storage.SavedFiles.Should().HaveCount(1);
        var saved = storage.SavedFiles.Single();
        saved.ContentType.Should().Be("image/png");
        saved.FileName.Should().StartWith("cnh_");
        saved.FileName.Should().EndWith(".png");
        saved.Bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Post_should_return_400_when_tipoCnh_is_invalid()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            identificador = "ent-int-2",
            nome = "Maria",
            cnpj = "11.111.111/0001-11",
            data_nascimento = "1990-01-01",
            numero_cnh = "99999999999",
            tipo_cnh = "Z",
            imagem_cnh = (string?)null
        };

        var response = await client.PostAsJsonAsync("/entregadores", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
        body.Should().NotBeNull();
        body!.Mensagem.Should().Be("Dados inválidos");
    }

    [Fact]
    public async Task Post_should_create_without_image_and_not_call_storage()
    {
        var client = _factory.CreateClient();

        var payload = new
        {
            identificador = "ent-int-3",
            nome = "Carlos",
            cnpj = "22.222.222/0001-22",
            data_nascimento = "1992-02-02",
            numero_cnh = "11122233344",
            tipo_cnh = "B",
            imagem_cnh = (string?)null
        };

        var response = await client.PostAsJsonAsync("/entregadores", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = (TestFileStorage)scope.ServiceProvider.GetRequiredService<IFileStorage>();

        var entity = await db.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identifier == "ent-int-3");

        entity.Should().NotBeNull();
        entity!.CnhImagemUrl.Should().BeNull();     
        storage.SavedFiles.Should().BeEmpty();     
    }

    [Fact]
    public async Task Post_should_update_cnh_and_replace_previous_image()
    {
        var client = _factory.CreateClient();

        var createPayload = new
        {
            identificador = "ent-int-upd-1",
            nome = "Atualiza CNH",
            cnpj = "33.333.333/0001-33",
            data_nascimento = "1990-01-01",
            numero_cnh = "12345678909",
            tipo_cnh = "A",
            imagem_cnh = BuildTinyPngBase64()
        };

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = (TestFileStorage)scope2.ServiceProvider.GetRequiredService<IFileStorage>();
        storage.Clear();

        var createResponse = await client.PostAsJsonAsync("/entregadores", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        string entregadorId;
        string oldUrl;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entity = await db.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Identifier == "ent-int-upd-1");

            entity.Should().NotBeNull();
            entregadorId = entity!.Identifier;
            oldUrl = entity.CnhImagemUrl!;
            oldUrl.Should().NotBeNullOrWhiteSpace();
        }

        var updatePayload = new
        {
            imagem_cnh = BuildTinyPngBase64()
        };

        var updateResponse = await client.PostAsJsonAsync($"/entregadores/{entregadorId}/cnh", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Created);


        var updated = await db2.Set<delivery_rental_system.Domain.Entities.DeliveryMan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identifier == entregadorId);

        updated.Should().NotBeNull();
        updated!.CnhImagemUrl.Should().NotBeNullOrWhiteSpace();
        updated.CnhImagemUrl.Should().NotBe(oldUrl);
        updated.CnhImagemUrl!.Should().StartWith("https://test-storage/cnh_");
        updated.CnhImagemUrl.Should().EndWith(".png");

        storage.SavedFiles.Should().HaveCount(1);

        var hasPng = storage.SavedFiles.Any(s =>
            s.ContentType == "image/png" &&
            s.FileName.StartsWith("cnh_") &&
            s.FileName.EndsWith(".png") &&
            s.Bytes.Length > 0);

        hasPng.Should().BeTrue();
    }
}
