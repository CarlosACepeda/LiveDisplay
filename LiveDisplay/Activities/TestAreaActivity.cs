using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using LiveDisplay.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Activities
{
    [Activity(Label = "TestAreaActivity")]
    public class TestAreaActivity : Activity
    {
        RecyclerView testList;
        RecyclerView.Adapter testAdapter;
        View notificationView;
        TextView title, text, subtext, siblingCount, childCount, isParent, isSibling, isStandalone, group_key;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            

            SetContentView(Resource.Layout.testarea);
            // Create your application here
            if (!Checkers.IsNotificationListenerEnabled())
            { Toast.MakeText(this, "Enable notifications first!", ToastLength.Long);
                Finish();
            }

            testList = FindViewById<RecyclerView>(Resource.Id.notif_group_test);
            testList.SetLayoutManager(new LinearLayoutManager(Application.Context, RecyclerView.Horizontal, true));
            testAdapter = NotificationHijackerWorker.NotificationAdapter;
            notificationView = FindViewById(Resource.Id.notif_test_view);
            title = FindViewById<TextView>(Resource.Id.test_title);
            subtext = FindViewById<TextView>(Resource.Id.test_subtitle);
            text = FindViewById<TextView>(Resource.Id.test_text);
            group_key = FindViewById<TextView>(Resource.Id.test_group_key);
            siblingCount = FindViewById<TextView>(Resource.Id.test_siblings_count);
            childCount = FindViewById<TextView>(Resource.Id.test_child_count);
            isParent = FindViewById<TextView>(Resource.Id.test_is_parent);
            isSibling = FindViewById<TextView>(Resource.Id.test_is_sibling);
            isStandalone = FindViewById<TextView>(Resource.Id.test_is_standalone);
            testList.SetAdapter(testAdapter);
            NotificationAdapter.ItemClick += NotificationAdapter_ItemClick;
        }

        private void NotificationAdapter_ItemClick(object sender, Services.Notifications.NotificationEventArgs.NotificationItemClickedEventArgs e)
        {
            title.Text = "Titulo: "+  e.StatusBarNotification.Title;
            text.Text = "Texto: " + e.StatusBarNotification.Text;
            subtext.Text = "Subtexto: " + e.StatusBarNotification.SubText;
            siblingCount.Text = "no. hermanos: " + e.SiblingCount.ToString();
            childCount.Text = "no. hijos: " + e.ChildCount.ToString();
            isParent.Text = "es Padre? " + e.IsParent.ToString() + ", es falso padre; " + e.IsFakeParent.ToString();
            isSibling.Text = "es hermano? " + e.IsSibling.ToString();
            isStandalone.Text = "es solitario? " + e.IsStandalone.ToString();
            group_key.Text = "llave de grupo: " + e.StatusBarNotification.GroupKey;


        }
    }
}