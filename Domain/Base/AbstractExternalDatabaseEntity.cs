namespace Domain.Base;

public class AbstractExternalDatabaseEntity : AbstractIdDatabaseEntity
{
    public Guid ExternalId { get; set; }
}