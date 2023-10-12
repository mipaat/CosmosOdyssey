namespace BLL.CosmosOdyssey.DTO;

public class Provider
{
    public Guid Id { get; set; }
    public Company Company { get; set; } = default!;
    public decimal Price { get; set; }
    public DateTime FlightStart { get; set; }
    public DateTime FlightEnd { get; set; }
}