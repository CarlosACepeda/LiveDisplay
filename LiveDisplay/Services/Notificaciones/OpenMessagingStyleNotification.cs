using Android.App;
using Android.OS;
using Android.Service.Notification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services.Notifications
{
    public class OpenMessagingStyleNotification : OpenNotification
    {
        public OpenMessagingStyleNotification(StatusBarNotification sbn) : base(sbn)
        {
            if (Style != MessagingStyle)
                throw new InvalidOperationException("Tried to create an instance with a wrong style");
        }

        public List<Notification.MessagingStyle.Message> Messages
        {
            get
            {
               var messagesBundle= statusbarnotification.Notification.Extras.GetParcelableArray(Notification.ExtraMessages);
                return Notification.MessagingStyle.Message.GetMessagesFromBundleArray(messagesBundle).ToList();
            }
        }
        public List<Notification.MessagingStyle.Message> HistoricMessages
        {
            get
            {
                var messagesBundle = statusbarnotification.Notification.Extras.GetParcelableArray(Notification.ExtraHistoricMessages);
                return Notification.MessagingStyle.Message.GetMessagesFromBundleArray(messagesBundle).ToList();
            }
        }

        public string ConversationTitle => statusbarnotification.Notification.Extras.GetString(Notification.ExtraConversationTitle);

        public Person SenderPerson => (Person)statusbarnotification.Notification.Extras.Get(Notification.ExtraMessagingPerson);

        public string SelfDisplayName=> statusbarnotification.Notification.Extras.GetString(Notification.ExtraSelfDisplayName);

        public bool IsGroupConvo => statusbarnotification.Notification.Extras.GetBoolean(Notification.ExtraIsGroupConversation);
    }
}