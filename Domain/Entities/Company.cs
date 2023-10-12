using Domain.Base;

namespace Domain.Entities;

public class Company : AbstractExternalDatabaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<LegProvider>? LegProviders { get; set; }
}