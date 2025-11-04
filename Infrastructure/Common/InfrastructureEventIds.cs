namespace SMS_Infrastructure.Common;
public static class InfrastructureEventIds
{
    public const int Critical = 1000;
    public const int Debug = 1001;
    public const int Error = 1002;
    public const int Information = 1003;
    public const int None = 1004;
    public const int Trace = 1005;
    public const int Warning = 1006;

    public const int InfrastructureEvent = 2000;
    public const int GetItems = 2001;
    public const int GetItem = 2002;
    public const int PostItem = 2003;
    public const int PutItem = 2004;
    public const int DeleteItem = 2005;
    public const int PutStatus = 2006;

    public const int TestItem = 3000;

    public const int GetItemError = 4000;
    public const int GetItemsError = 4001;
    public const int PutItemError = 4002;
    public const int PostItemError = 4003;
    public const int PutStatusError = 4004;
    public const int DeleteItemError = 4005;
    public const int InfrastructureError = 4006;
}
