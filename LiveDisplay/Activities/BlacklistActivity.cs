using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
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