namespace Tracksys.Modules.Citizen.Domain.Enums;

public enum ComplaintStatus
{
    Received,
    InProgress,
    Resolved,
}

public static class ComplaintStatusExtensions
{
    public static string ToCode(this ComplaintStatus status) => status switch
    {
        ComplaintStatus.Received => "received",
        ComplaintStatus.InProgress => "inprogress",
        ComplaintStatus.Resolved => "resolved",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    public static ComplaintStatus FromCode(string code) => code switch
    {
        "received" => ComplaintStatus.Received,
        "inprogress" => ComplaintStatus.InProgress,
        "resolved" => ComplaintStatus.Resolved,
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
