using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Application.Handlers;
using delivery_rental_system.Application.Validators;
using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Entities.Rental;
using delivery_rental_system.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.RentalTests;

public class CreateRentalCommandHandlerCnhTests
{
    [Fact]
    public async Task Validator_should_fail_when_deliveryman_is_not_category_A()
    {
        // Arrange
        var dmRepo = new Mock<IDeliveryManRepository>();
        var motoRepo = new Mock<IMotorcycleRepository>();
        var rentRepo = new Mock<IRentalRepository>();

        var deliveryManId = "ent-1";
        var motorcycleId = "moto-1";

        dmRepo.Setup(r => r.GetByIdentifierAsync(deliveryManId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new DeliveryMan(
                  identifier: deliveryManId,
                  nome: "João",
                  cnpj: "12345678000190",
                  dataNascimento: new DateTime(1990, 1, 1),
                  cnhNumero: "12345678901",
                  cnhEnum: CnhEnum.B,   
                  cnhImagemUrl: null));

        motoRepo.Setup(r => r.GetByIdentifierAsync(motorcycleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Motorcycle(
                    identifier: motorcycleId,
                    year: 2024, model: "Honda", plate: "AAA1A11"));

        var todayUtc = DateTime.UtcNow.Date;
        var start = todayUtc.AddDays(1);
        var prev = start.AddDays(7 - 1);
        var end = prev;

        var cmd = new CreateRentalCommand(
            EntregadorId: deliveryManId,
            MotoId: motorcycleId,
            Plano: 7,
            DataInicio: start,
            DataTermino: end,
            DataPrevisaoTermino: prev);

        var validator = new CreateRentalCommandValidator(dmRepo.Object, motoRepo.Object, rentRepo.Object);

        // Act
        var result = await validator.ValidateAsync(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "entregador_id" &&
            e.ErrorMessage.Contains("Entregador não habilitado na categoria A"));
    }

    [Fact]
    public async Task Should_create_when_deliveryman_is_category_A()
    {
        // Arrange
        var dmRepo = new Mock<IDeliveryManRepository>();
        var motoRepo = new Mock<IMotorcycleRepository>();
        var rentalRepo = new Mock<IRentalRepository>();

        var deliveryManId = "ent-1";
        var motorcycleId = "moto-1";

        var deliveryMan = new DeliveryMan(
            identifier: deliveryManId,
            nome: "João Entregador",
            cnpj: "12345678000190",
            dataNascimento: new DateTime(1990, 1, 1),
            cnhNumero: "12345678901",
            cnhEnum: CnhEnum.A,    
            cnhImagemUrl: null
        );

        dmRepo.Setup(r => r.GetByIdentifierAsync(deliveryManId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(deliveryMan);

        var motorcycle = new Motorcycle(
            identifier: motorcycleId,
            year: 2024,
            model: "Honda",
            plate: "AAA1A11"
        );

        motoRepo.Setup(r => r.GetByIdentifierAsync(motorcycleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(motorcycle);

        rentalRepo.Setup(r => r.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);
        rentalRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var validator = new CreateRentalCommandValidator(dmRepo.Object, motoRepo.Object, rentalRepo.Object);

        var nowUtc = new DateTime(2025, 09, 08, 00, 00, 00, DateTimeKind.Utc);
        var start = nowUtc.Date.AddDays(1); 
        var prev = start.AddDays(7 - 1);
        var end = default(DateTime); 

        var cmd = new CreateRentalCommand(
            EntregadorId: deliveryManId,
            MotoId: motorcycleId,
            Plano: (int)RentalPlan.Days7,  
            DataInicio: start,
            DataTermino: prev,
            DataPrevisaoTermino: prev
        );

        // Act
        var result = await validator.ValidateAsync(cmd);

        // Assert
        result.IsValid.Should().BeTrue(result.ToString());
    }


}
