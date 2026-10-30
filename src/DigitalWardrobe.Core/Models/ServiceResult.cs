namespace DigitalWardrobe.Core.Models;

public enum ResultStatus
{
    Success,
    NotFound,
    Invalid
}

public class ServiceResult
{
    public ResultStatus Status { get; }
    public string? Error { get; }
    public bool Succeeded => Status == ResultStatus.Success;

    private ServiceResult(ResultStatus status, string? error)
    {
        Status = status;
        Error = error;
    }

    public static ServiceResult Ok() => new(ResultStatus.Success, null);
    public static ServiceResult NotFound() => new(ResultStatus.NotFound, null);
    public static ServiceResult Invalid(string error) => new(ResultStatus.Invalid, error);
}