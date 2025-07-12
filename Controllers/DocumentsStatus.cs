


public static class DocumentStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";

    public static bool IsValid(string status)
    {
        return status == Pending || status == Approved || status == Rejected;
    }
}