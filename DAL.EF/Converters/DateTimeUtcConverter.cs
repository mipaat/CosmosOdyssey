using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DAL.EF.Converters;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter() : base(
        dt => dt.ToUniversalTime(),
        dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc))
    {
    }
}