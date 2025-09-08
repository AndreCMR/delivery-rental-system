using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Application.Handlers.Rental;
using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Domain.Enums;
using FluentAssertions;
using Moq;

namespace UnitTests.RentalTests;


public class RentalBillingTests
{
    private static decimal DailyRate(RentalPlan plan) => plan switch
    {
        RentalPlan.Days7 => 30m,
        RentalPlan.Days15 => 28m,
        RentalPlan.Days30 => 22m,
        RentalPlan.Days45 => 20m,
        RentalPlan.Days50 => 18m,
        _ => throw new ArgumentOutOfRangeException(nameof(plan))
    };

    private static (Rental rental, DateTime start, DateTime predicted) BuildRental(RentalPlan plan)
    {
        var nowUtc = new DateTime(2025, 09, 08, 0, 0, 0, DateTimeKind.Utc);
        var start = nowUtc.Date.AddDays(1);
        var predicted = start.AddDays((int)plan - 1);

        var rental = new Rental(
            identifier: "loc-1",
            deliveryManId: 1,
            motorcycleId: 1,
            plan: plan,
            dataInicio: start,
            dataTermino: default,    
            dataPrevisaoTermino: predicted,
            nowUtc: nowUtc
        );

        return (rental, start, predicted);
    }

    [Theory]
    [InlineData(RentalPlan.Days7)]
    [InlineData(RentalPlan.Days15)]
    [InlineData(RentalPlan.Days30)]
    [InlineData(RentalPlan.Days45)]
    [InlineData(RentalPlan.Days50)]
    public async Task SetReturnDate_Should_charge_full_plan_when_on_time(RentalPlan plan)
    {
        // Arrange
        var repo = new Mock<IRentalRepository>();
        var (rental, _, predicted) = BuildRental(plan);

        repo.Setup(r => r.GetByIdentifierAsync("loc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SetReturnDateCommandHandler(repo.Object);

        var cmd = new SetReturnDateCommand(
            RentalId: "loc-1",
            ReturnDateUtc: predicted
        );

        var daily = DailyRate(plan);
        var expectedTotal = ((int)plan) * daily;

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        rental.EndDate.Should().Be(predicted);
        rental.RentalValue.Should().Be(expectedTotal);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(RentalPlan.Days7, 3, 0.20)] 
    [InlineData(RentalPlan.Days15, 5, 0.40)] 
    public async Task SetReturnDate_Should_apply_penalty_when_return_is_early(
        RentalPlan plan, int usedDays, decimal penaltyPct)
    {
        // Arrange
        var repo = new Mock<IRentalRepository>();
        var (rental, start, predicted) = BuildRental(plan);

        var returnDate = start.AddDays(usedDays - 1);

        repo.Setup(r => r.GetByIdentifierAsync("loc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SetReturnDateCommandHandler(repo.Object);

        var cmd = new SetReturnDateCommand(
            RentalId: "loc-1",
            ReturnDateUtc: returnDate
        );

        var daily = DailyRate(plan);
        var planDays = (int)plan;
        var notUsedDays = Math.Max(0, planDays - usedDays);
        var baseValue = usedDays * daily;
        var expectedPenalty = notUsedDays * daily * (decimal)penaltyPct;
        var expectedTotal = baseValue + expectedPenalty;

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        rental.EndDate.Should().Be(returnDate);
        rental.RentalValue.Should().Be(expectedTotal);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(RentalPlan.Days7, 1)]
    [InlineData(RentalPlan.Days7, 3)]
    [InlineData(RentalPlan.Days15, 2)]
    [InlineData(RentalPlan.Days30, 5)]
    public async Task SetReturnDate_Should_charge_full_plan_plus_50_per_extra_day_when_late(RentalPlan plan, int extraDays)
    {
        // Arrange
        var repo = new Mock<IRentalRepository>();
        var (rental, _, predicted) = BuildRental(plan);

        var returnDate = predicted.AddDays(extraDays);

        repo.Setup(r => r.GetByIdentifierAsync("loc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SetReturnDateCommandHandler(repo.Object);

        var cmd = new SetReturnDateCommand(
            RentalId: "loc-1",
            ReturnDateUtc: returnDate
        );

        var daily = DailyRate(plan);
        var fullPlan = ((int)plan) * daily;
        var extraValue = extraDays * 50m;
        var expectedTotal = fullPlan + extraValue;

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        rental.EndDate.Should().Be(returnDate);
        rental.RentalValue.Should().Be(expectedTotal);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}