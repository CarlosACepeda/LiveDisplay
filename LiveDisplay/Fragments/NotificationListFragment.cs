using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationListFragment : Fragment
    {
        RecyclerView notification_list;
        private const bool DONT_REVERSE_LAYOUT = false;

        private List<OpenNotification> OpenNotifications { get; set; }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.test_rclv_notif_list, container, false);
            
            using (notification_list = view.FindViewById<RecyclerView>(Resource.Id.notification_list))
            {
                using (var layoutManager = new LinearLayoutManager(Application.Context, (int)Orientation.Vertical, DONT_REVERSE_LAYOUT))
                {
                    notification_list.SetLayoutManager(layoutManager);
                    notification_list.SetAdapter(new FilteredNotificationAdapter(OpenNotifications));
                }
            }
            return view;
        }
    }
}