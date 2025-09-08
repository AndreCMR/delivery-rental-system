namespace delivery_rental_system.Domain.Enums;


public static class RentalPlanExtensions
{
    public static (int Days, decimal DailyRate) GetPlanInfo(this RentalPlan plan)
    {
        return plan switch
        {
            RentalPlan.Days7 => (7, 30m),
            RentalPlan.Days15 => (15, 28m),
            RentalPlan.Days30 => (30, 22m),
            RentalPlan.Days45 => (45, 20m),
            RentalPlan.Days50 => (50, 18m),
            _ => throw new ArgumentOutOfRangeException(nameof(plan), "Invalid rental plan")
        };
    }

    public static int GetDays(this RentalPlan plan)
    {
        return plan switch
        {
            RentalPlan.Days7 => 7,
            RentalPlan.Days15 => 15,
            RentalPlan.Days30 => 30,
            RentalPlan.Days45 => 45,
            RentalPlan.Days50 => 50,
            _ => throw new ArgumentOutOfRangeException(nameof(plan))
        };
    }

    public static decimal GetDailyRate(this RentalPlan plan)
    {
        return plan switch
        {
            RentalPlan.Days7 => 30m,
            RentalPlan.Days15 => 28m,
            RentalPlan.Days30 => 22m,
            RentalPlan.Days45 => 20m,
            RentalPlan.Days50 => 18m,
            _ => throw new ArgumentOutOfRangeException(nameof(plan))
        };
    }
}
