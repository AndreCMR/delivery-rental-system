using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.RentalTests;


public class RentalDomainDateRulesTests
{
    [Theory]
    [InlineData(RentalPlan.Days7, 7)]
    [InlineData(RentalPlan.Days15, 15)]
    [InlineData(RentalPlan.Days30, 30)]
    [InlineData(RentalPlan.Days45, 45)]
    [InlineData(RentalPlan.Days50, 50)]
    public void Constructor_should_set_start_to_next_day_and_predicted_end_according_to_plan(RentalPlan plan, int days)
    {
        // Arrange
        var nowUtc = new DateTime(2024, 01, 10, 13, 45, 00, DateTimeKind.Utc);
        var expectedStart = nowUtc.Date.AddDays(1);
        var expectedPredictedEnd = expectedStart.AddDays(days - 1);
        var expectedEnd = default(DateTime);
        // Act
        var rental = new Rental(
            identifier: "loc-1",
            deliveryManId: 1,
            motorcycleId: 1,
            plan: plan,                        
            dataInicio: expectedStart,
            dataTermino: expectedEnd,
            dataPrevisaoTermino: expectedPredictedEnd,
            nowUtc: nowUtc
        );

        // Assert
        rental.StartDate.Should().Be(expectedStart);
        rental.PredictedEndDate.Should().Be(expectedPredictedEnd);

        rental.StartDate.Should().NotBe(default);
        rental.PredictedEndDate.Should().NotBe(default);

        rental.EndDate.Should().Be(default);
    }
}