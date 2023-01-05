using WebApplicationWithMongo.Pages.Orders;

namespace WebApplicationWithMongo.Application;

internal static class OrderHelper
{
    private static readonly string[] Names = { "Dinner table {0}", "Hummer {0}", "Computer monitor {0}", "Last year snow {0}", "Patterns from {0} SOLID", "Infrastructure {0}", "Dependency injection container {0}", "Some helpful nuget {0}" };
    private static readonly string[] Descriptions = { "Small dining table model #{0} for your needs", "Big iron hummer weight #{0} tons", "Yellow computer monitor with diagonal #{0}", "White as last year snow from year {0}",
        "Programming patterns from SOLID. Extended edition #{0}", "Folder from infrastructure (total {0})", "Dependency injection container for some special injection for your code line #{0}", "There are some helpful nugets, but this one is huge help number #{0}" };


    internal static Order GetRandomOrder()
    {
        var id = Random.Shared.Next(1, 1000);
        var items = GetRandomOrderItems(id, Random.Shared.Next(4, 10));
        return new Order
        {
            Id = id,
            Description = GetDescriptions(id),
            Number = Random.Shared.Next(10000, 99999),
            Title = GetNames(id),
            Items = items.ToList()
        };
    }

    private static IEnumerable<OrderItem> GetRandomOrderItems(int id, int count)
    {
        return Enumerable.Range(1, count).Select(x => new OrderItem
        {
            Id = Random.Shared.Next(1, 1000),
            Name = $"Order item {x} for Order #{id}",
            Price = Random.Shared.Next(100, 900),
            Quantity = Random.Shared.Next(1, 10)

        });
    }

    private static string GetNames(int id)
    {
        var index = Random.Shared.Next(0, Names.Length - 1);
        return string.Format(Names[index], id);
    }
    private static string GetDescriptions(int id)
    {
        var index = Random.Shared.Next(0, Descriptions.Length - 1);
        return string.Format(Descriptions[index], id);
    }
}