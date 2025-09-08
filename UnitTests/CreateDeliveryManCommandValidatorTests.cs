using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.DeliveryMan;
using delivery_rental_system.Application.Validators;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class CreateDeliveryManCommandValidatorTests
{
    private readonly Mock<IDeliveryManRepository> _repoMock;
    private readonly CreateDeliveryManCommandValidator _validator;

    public CreateDeliveryManCommandValidatorTests()
    {
        _repoMock = new Mock<IDeliveryManRepository>(MockBehavior.Strict);

        _repoMock.Setup(r => r.CnpjExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        _repoMock.Setup(r => r.CnhExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        _validator = new CreateDeliveryManCommandValidator(_repoMock.Object);
    }

    [Theory]
    [InlineData("ent-1", "João Entregador", "12.345.678/0001-90", "12345678901", "A", true)]  
    [InlineData("", "João", "12345678000190", "12345678901", "A", false)]                      
    [InlineData("ent-2", "", "12345678000190", "12345678901", "A", false)]                     
    [InlineData("ent-3", "Maria", "111", "12345678901", "A", false)]                             
    [InlineData("ent-4", "Carlos", "12345678000190", "abc", "A", false)]                      
    [InlineData("ent-5", "Ana", "12345678000190", "12345678901", "Z", false)]                 
    public async Task Should_validate_basic_rules(
        string identificador,
        string nome,
        string cnpj,
        string numeroCnh,
        string tipoCnh,
        bool expectedValid)
    {
        var cmd = new CreateDeliveryManCommand(
            Identificador: identificador,
            Nome: nome,
            Cnpj: cnpj,
            DataNascimento: new DateTime(1990, 1, 1),
            NumeroCnh: numeroCnh,
            TipoCnh: tipoCnh,
            ImagemCnh: null
        );

        var result = await _validator.ValidateAsync(cmd);

        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public async Task Should_be_invalid_when_cnpj_already_exists()
    {
        _repoMock.Setup(r => r.CnpjExistsAsync("12345678000190", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var cmd = new CreateDeliveryManCommand(
            Identificador: "ent-dup-cnpj",
            Nome: "João",
            Cnpj: "12.345.678/0001-90",
            DataNascimento: new DateTime(1990, 1, 1),
            NumeroCnh: "12345678901",
            TipoCnh: "A",
            ImagemCnh: null
        );

        var result = await _validator.ValidateAsync(cmd);

        result.IsValid.Should().BeFalse();
        _repoMock.Verify(r => r.CnpjExistsAsync("12345678000190", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_be_invalid_when_cnh_already_exists()
    {
        _repoMock.Setup(r => r.CnhExistsAsync("12345678901", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var cmd = new CreateDeliveryManCommand(
            Identificador: "ent-dup-cnh",
            Nome: "Maria",
            Cnpj: "98765432000199",
            DataNascimento: new DateTime(1990, 1, 1),
            NumeroCnh: "12345678901",
            TipoCnh: "B",
            ImagemCnh: null
        );

        var result = await _validator.ValidateAsync(cmd);

        result.IsValid.Should().BeFalse();
        _repoMock.Verify(r => r.CnhExistsAsync("12345678901", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_be_valid_with_png_base64_plain()
    {
        var cmd = BaseValidCommand(BuildTinyPngBase64());
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_be_valid_with_png_data_url()
    {
        var cmd = BaseValidCommand($"data:image/png;base64,{BuildTinyPngBase64()}");
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_be_valid_with_bmp_base64_plain()
    {
        var cmd = BaseValidCommand(BuildTinyBmpBase64());
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_be_valid_with_bmp_data_url()
    {
        var cmd = BaseValidCommand($"data:image/bmp;base64,{BuildTinyBmpBase64()}");
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("data:image/jpg;base64,abcd")] 
    [InlineData("abcd")]                      
    public async Task Should_be_invalid_when_image_is_not_png_or_bmp(string badImage)
    {
        var cmd = BaseValidCommand(badImage);
        var result = await _validator.ValidateAsync(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage)
                     .Should().Contain(m => m.Contains("Imagem inválida"));
    }

    private static CreateDeliveryManCommand BaseValidCommand(string? imagem = null) =>
        new CreateDeliveryManCommand(
            Identificador: "ent-ok",
            Nome: "Fulano de Tal",
            Cnpj: "12.345.678/0001-90",
            DataNascimento: new DateTime(1990, 1, 1),
            NumeroCnh: "12345678901",
            TipoCnh: "A",
            ImagemCnh: imagem
        );

    private static string BuildTinyPngBase64()
    {
        var png = new byte[]
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
        return Convert.ToBase64String(png);
    }

    private static string BuildTinyBmpBase64()
    {
        var bmp = new byte[]
        {
            0x42,0x4D,
            0x1A,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,
            0x1A,0x00,0x00,0x00,
            0x0C,0x00,0x00,0x00,
            0x01,0x00,
            0x01,0x00,
            0x01,0x00,
            0x18,0x00,
            0xFF,0xFF,0xFF
        };
        return Convert.ToBase64String(bmp);
    }
}