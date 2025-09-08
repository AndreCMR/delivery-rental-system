
using delivery_rental_system.Domain.Entities;
using FluentAssertions;

namespace UnitTests;

public class MotorcycleTests
{
    [Fact]
    public void UpdatePlate_should_change_plate_to_upper_and_trim()
    {
        var moto = new Motorcycle("moto-456", 2023, "Honda CG", "abc1d23");
        moto.UpdatePlate("xyz9a88");
        moto.Plate.Should().Be("XYZ9A88");
    }
}