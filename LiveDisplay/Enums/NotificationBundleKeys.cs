namespace LiveDisplay.Enums
{
    static class NotificationBundleKeys
    {
        //Contains definitions for *Bundle* keys WITHIN a StatusBarNotification object (Extras) or within other bundles in the same StatusBarNotification object.
        public const string MESSAGE_BUNDLES = "android.messages"; //retrieves a list of bundles containing Messages.
        public const string MESSAGE_SENDER_PERSON = "sender_person"; //used to retrieve a Android.App.Person instance.
        public const string MESSAGE_SENDER = "sender"; //The string that defines who is the sender. meant to be shown.
        public const string MESSAGE_TEXT = "text"; //text (String field) of a Message instance that was within one of the Bundles within 'android.messages's Bundle.
        public const string MESSAGE_TIME = "time"; //text containing the time of this message in Ticks.
    }
}