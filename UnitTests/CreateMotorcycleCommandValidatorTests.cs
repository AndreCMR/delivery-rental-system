using delivery_rental_system.Application.Commands.Create;
using delivery_rental_system.Application.Validators;
using FluentAssertions;

namespace UnitTests;

public class CreateMotorcycleCommandValidatorTests
{
    private readonly CreateMotorcycleCommandValidator _validator = new();

    [Theory]
    [InlineData(2020, "Mottu Sport", "ABC-1234", true)]
    [InlineData(1899, "Mottu", "AAA-0000", false)] 
    [InlineData(2024, "", "AAA-0000", false)]       
    public void Should_validate_basic_rules(int year, string model, string plate, bool expectedValid)
    {
        var cmd = new CreateMotorcycleCommand("moto123", year, model, plate);
        var result = _validator.Validate(cmd);

        result.IsValid.Should().Be(expectedValid);
    }
}