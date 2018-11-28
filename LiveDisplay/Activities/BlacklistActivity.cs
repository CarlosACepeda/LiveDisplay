using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;

namespace LiveDisplay.Activities
{
    [Activity(Label = "BlacklistActivity")]
    public class BlacklistActivity : AppCompatActivity
    {
        private RecyclerView blacklistRecyclerView;
        private LinearLayoutManager manager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlackList);

            var applist = Blacklist.GetListOfApps();
            // Create your application here
            blacklistRecyclerView = FindViewById<RecyclerView>(Resource.Id.blacklistRecyclerView);

            manager = new LinearLayoutManager(Application.Context);
            
                blacklistRecyclerView.SetLayoutManager(manager);
                blacklistRecyclerView.SetAdapter(new AppsToBeBlacklistedAdapter(applist));
            
            
            

        }
    }
}