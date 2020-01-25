namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Support.V7.Widget;
    using Android.Widget;
    using LiveDisplay.Adapters;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Toolbar = Android.Support.V7.Widget.Toolbar;

    [Activity(Label = "@string/blacklist", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BlacklistActivity : AppCompatActivity
    {
        private RecyclerView blacklistRecyclerView;
        private LinearLayoutManager manager;
        private List<PackageInfo> applist;
        private Toolbar toolbar;
        private ProgressBar loadingblacklistitemsprogressbar;
        private EditText searchboxapp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlackList);

            loadingblacklistitemsprogressbar = FindViewById<ProgressBar>(Resource.Id.loadingblacklistitemsprogressbar);
            searchboxapp = FindViewById<EditText>(Resource.Id.searchboxapp);
            searchboxapp.TextChanged += Searchboxapp_TextChanged;

            using (toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar))
            {
                SetSupportActionBar(toolbar);
            }

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

        private void Searchboxapp_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            bool searching = false;
            if (!searching) //Is this valid? It does not seem to do anything.
            {
                searching = true;
                loadingblacklistitemsprogressbar.Visibility = Android.Views.ViewStates.Visible;
                blacklistRecyclerView.Visibility = Android.Views.ViewStates.Invisible;
                ThreadPool.QueueUserWorkItem(method =>
                {
                    if (e.Text.ToString() != string.Empty || e.Text.ToString() != null)
                    {
                        applist = Blacklist.GetListOfApps().Where(x => PackageUtils.GetTheAppName(x.PackageName).Contains(e.Text.ToString())).ToList();
                    }
                    else
                    {
                        applist = Blacklist.GetListOfApps();
                    }
                }
                );
                RunOnUiThread(() =>
                {
                    blacklistRecyclerView.SetAdapter(new AppsToBeBlacklistedAdapter(applist));
                    loadingblacklistitemsprogressbar.Visibility = Android.Views.ViewStates.Invisible;
                    blacklistRecyclerView.Visibility = Android.Views.ViewStates.Visible;
                    searching = false;
                });
            }
        }
    }
}