namespace BLL.CosmosOdyssey.DTO;

public class PriceList
{
    public Guid Id { get; set; }
    public DateTime ValidUntil { get; set; }
    public ICollection<Leg> Legs { get; set; } = default!;
}