public sealed class Permissions
{
    public const string PermissionKey = "PermissionsKey"; //to be used when sending an intent.

    public const int None = 0;
    public const int PostNotifications = 1;
    public const int EnableAccessibilityService = 2;
    public const int ReadNotifications = 3;
    public const int DeviceAdmin = 4;
    public const int Location = 5;
}