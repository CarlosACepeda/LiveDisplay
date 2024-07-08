using Android.Media.Session;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Services.Notifications
{
    internal class CatcherHelper : Java.Lang.Object
    {

        private static List<OpenNotification> OpenNotifications { get; set; }

        public static event EventHandler<NotificationRemovedEventArgs> NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;

        public static event EventHandler<bool> EnteredZenMode;

        public static event EventHandler<OpenNotification> RequestedOpenNotificationResultGenerated;

        const string LiveDisplayAlertWindowNotificationTag= "com.android.server.wm.AlertWindowNotification - com.underground.livedisplay";
        const string AndroidPackageName = "android";
        const string LiveDisplayPackage = "com.underground.livedisplay";

        public CatcherHelper(List<OpenNotification> openNotifications)
        {
            OpenNotifications = openNotifications;


            //notificationAdapter = new NotificationAdapter(statusBarNotifications);
            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = openNotifications.Count > 0
            });
        }

        public void OnNotificationPosted(OpenNotification sbn)
        {
            if (sbn == null) { return; }
            //This is the notification of 'LiveDisplay is showing above other apps' when using floating windows.
            //Simply let's ignore it, because it's annoying. (Anyway, the user couldn't care less about this notification tbh)
            if (sbn.PackageName == AndroidPackageName && sbn.Tag == LiveDisplayAlertWindowNotificationTag)
                return;


            //WORKAROUND:
            //Here we are waiting for a FullScreenIntent notification from my app to be captured, 
            //this notification is sent when the screen is turned off, see ScreenOnOffReceiver
            //In order to avoid the user from seeing/or hearing this notification, we click it on behalf of the user as soon as it arrives.
            //1. Clicking it on behalf of the user will cause whatever PendingIntent to be Sent
            //2. The PendingIntent is "start the Lockscreen Activity"
            //3. This notification HAS to be set with High Importance, I tried setting up less importance and after clicking the notification on behalf of the user doesn't work,
            //4. Clicking this notification as soon as it arrives apparently cancels the notification sound,
            //but I haven't confirmed this to be true on all android devices.
            //5. If media is playing, the media will lower its volume to make this notification to sound, but let's remember that the notification doesn't emit a sound.

            
            //To see how it works please go to ScreenOnOffReceiver, this broadcast works as the one starting this whole workaround
            if(sbn.PackageName== LiveDisplayPackage&& sbn.Id==100)
            {
                NotificationSlave.GetInstance().ClickNotification(sbn);
            }

            int index = GetNotificationPosition(sbn); //Tries to get the index of a possible already existing notification in the list of notif.
            if (index >= 0)
            {
                //It exists within the list.
                //SO it should be updated.

                OpenNotifications.RemoveAt(index);
                OpenNotifications.Add(sbn);


                OnNotificationPosted(sbn, true);

            }
            else
            {
                OpenNotifications.Add(sbn);
                OnNotificationPosted(sbn, false);
            }
            


            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = OpenNotifications.Count > 0
            }) ;
            
        }

        public void OnNotificationRemoved(OpenNotification sbn)
        {
           
            if (sbn.PackageName == AndroidPackageName && sbn.Tag == LiveDisplayAlertWindowNotificationTag)
                return;

            if (sbn.PackageName == LiveDisplayPackage && sbn.Id == 100) //This is the workaround notification, we don't need it for anything
                return;

            int position = GetNotificationPosition(sbn);
            OpenNotification notificationToBeRemoved = null;

            if (position >= 0)
            {
                //if found, then use the Notification to be removed instead. 
                //the reason is that the 'sbn' coming from this method has less data.
                //then it makes data that I need from the notification unavailable.
                notificationToBeRemoved = OpenNotifications?[position];

                OpenNotifications.RemoveAt(position);
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = !(OpenNotifications.Where(n => n.IsClearable).ToList().Count == 0)
                });
                NotificationRemoved?.Invoke(this, new NotificationRemovedEventArgs()
                {
                    OpenNotification = notificationToBeRemoved
                });
            }
        }

        public void OnOpenNotificationRequested(Func<OpenNotification, bool> predicate)
        {
            RequestedOpenNotificationResultGenerated?.Invoke(this,OpenNotifications.Where(predicate).FirstOrDefault());
        }

        public void CancelAllNotifications()
        {
           
        }

        public static OpenNotification FindMostRecentMediaNotification()
        {
            if (OpenNotifications != null && OpenNotifications.Count >= 1)
            {
                var mediaNotifications = OpenNotifications.Where(n => n.Style == OpenNotification.MediaStyle);
                var ordered = mediaNotifications.OrderByDescending(n => n.PostTime).OrderByDescending(n => n.IsOngoing);
                return ordered.FirstOrDefault();
            }
            return null;

        }

        private int GetNotificationPosition(OpenNotification sbn)
        {
            return OpenNotifications.IndexOf(OpenNotifications.FirstOrDefault
                (o => o.Id == sbn.Id && 
                o.PackageName == sbn.PackageName));
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(this, e);
        }

        private void OnNotificationPosted(OpenNotification sbn, bool updatesPreviousNotification)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs()
            {
                ShouldCauseWakeUp = false,
                OpenNotification = sbn,
                UpdatesPreviousNotification = updatesPreviousNotification
            });
        }
        public void OnZenModeChanged(bool active)
        {
            EnteredZenMode?.Invoke(this, active);
        }
    }
}