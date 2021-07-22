using Android.Views;
using Android.Widget;
using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    internal class MessagingStyleNotification : NotificationStyle
    {
        public MessagingStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
            : base(openNotification, ref notificationView, notificationFragment)
        {
        }

        public override void ApplyStyle()
        {
            base.ApplyStyle();
            SetPreviousMessages();
        }

        private void SetPreviousMessages()
        {
            var previousMessages = OpenNotification.Messages;
            if (previousMessages.Length>0)
            {
                //PreviousMessages.Visibility = ViewStates.Visible;
                //PreviousMessages.Text =string.Empty;
            }
            else
            {
                //PreviousMessages.Visibility = ViewStates.Gone;
            }
        }

        protected override void Collapse_Click(object sender, EventArgs e)
        {
            //if (PreviousMessages.Text != string.Empty)
            //    PreviousMessages.Visibility = PreviousMessages.Visibility == ViewStates.Visible ? ViewStates.Gone : ViewStates.Visible;
            //else
            //    PreviousMessages.Visibility = ViewStates.Gone;
        }
    }
}