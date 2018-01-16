using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Fragments
{
    //Tratando de determinar la funcionalidad de este fragment y su axml.
    public class NotificationFragment : Fragment
    {
        TextView tv1, tv2;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view;

            view= inflater.Inflate(Resource.Layout.NotificationFragment, container, false);

            tv1 = view.FindViewById<TextView>(Resource.Id.textView1);
            tv1.Text = "lel";

            return view;

        }

    }
}