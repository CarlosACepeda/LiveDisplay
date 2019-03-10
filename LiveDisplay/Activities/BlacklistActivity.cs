using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;
using System.Collections.Generic;
using System.Threading;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/blacklist", Theme = "@style/LiveDisplayThemeDark")]
    public class BlacklistActivity : AppCompatActivity
    {
        private RecyclerView blacklistRecyclerView;
        private LinearLayoutManager manager;
        private List<PackageInfo> applist;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlackList);

            // Create your application here
            blacklistRecyclerView = FindViewById<RecyclerView>(Resource.Id.blacklistRecyclerView);

            manager = new LinearLayoutManager(Application.Context);

            blacklistRecyclerView.SetLayoutManager(manager);
            ThreadPool.QueueUserWorkItem(m =>
            {
                applist = Blacklist.GetListOfApps();
                RunOnUiThread(() => blacklistRecyclerView.SetAdapter(new AppsToBeBlacklistedAdapter(applist)));
            }
            );
        }
    }
}