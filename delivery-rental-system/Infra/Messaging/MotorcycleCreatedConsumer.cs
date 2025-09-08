using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Events;
using delivery_rental_system.Infra.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace delivery_rental_system.Infra.Messaging;

public sealed class MotorcycleCreatedConsumer : IConsumer<MotorcycleCreated>
{
    private readonly AppDbContext _context;
    public MotorcycleCreatedConsumer(AppDbContext ctx) => _context = ctx;
    public async Task Consume(ConsumeContext<MotorcycleCreated> context)
    {
        var msg = context.Message;

        if (msg.Year == 2024)
        {
            var exists = await _context.Year2024MotorcycleNotifications
            .AnyAsync(x => x.Identifier == msg.Identifier,
            context.CancellationToken);

            if (!exists)
            {
                _context.Year2024MotorcycleNotifications.Add(new
                    Year2024MotorcycleNotification
                {
                    Identifier = msg.Identifier,
                    Plate = msg.Plate,
                    Year = msg.Year,
                    ReceivedAtUtc = DateTime.UtcNow
                }
                );


                await _context.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
