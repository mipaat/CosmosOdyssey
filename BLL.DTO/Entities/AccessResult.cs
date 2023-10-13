namespace BLL.DTO.Entities;

public class AccessResult<TResult>
{
    public EAccessResultType Type { get; set; }
    public TResult? Result { get; set; }

    public static AccessResult<TResult> From(TResult? result) => new()
    {
        Type = result != null ? EAccessResultType.Success : EAccessResultType.NotFound,
        Result = result,
    };

    public static implicit operator AccessResult<TResult>(TResult? result) => From(result);

    public static implicit operator AccessResult<TResult>(EAccessResultType type) => new()
    {
        Type = type,
    };
}