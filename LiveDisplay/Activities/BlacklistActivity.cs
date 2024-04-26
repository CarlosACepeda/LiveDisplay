namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Adapters;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

    [Activity(Label = "@string/blacklist", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class BlacklistActivity : AppCompatActivity
    {
        private RecyclerView blacklistRecyclerView;
        private LinearLayoutManager manager;
        private List<PackageInfo> applist;
        private Toolbar toolbar;
        private ProgressBar loadingblacklistitemsprogressbar;
        private EditText searchboxapp;
        private Spinner blocklevelfilter;
        private LevelsOfAppBlocking selectedAppBlockingFilter;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlackList);

            loadingblacklistitemsprogressbar = FindViewById<ProgressBar>(Resource.Id.loadingblacklistitemsprogressbar);
            searchboxapp = FindViewById<EditText>(Resource.Id.searchboxapp);
            searchboxapp.TextChanged += Searchboxapp_TextChanged;
            blocklevelfilter = FindViewById<Spinner>(Resource.Id.blocklevelfilter);
            var spinnerAdapter = ArrayAdapter<string>.CreateFromResource(this, Resource.Array.listentriesblockingfilter, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            blocklevelfilter.Adapter = spinnerAdapter;

            blocklevelfilter.ItemSelected += Blocklevelfilter_ItemSelected;

            using (toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar))
            {
                SetSupportActionBar(toolbar);
            }

            blacklistRecyclerView = FindViewById<RecyclerView>(Resource.Id.blacklistRecyclerView);

            manager = new LinearLayoutManager(Application.Context);

            blacklistRecyclerView.SetLayoutManager(manager);
            LoadList(LevelsOfAppBlocking.None, null);
        }

        private void Blocklevelfilter_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch (e.Id)
            {
                case 0: //None
                    selectedAppBlockingFilter = LevelsOfAppBlocking.None;
                    break;
                case 1: //Ignored
                    selectedAppBlockingFilter = LevelsOfAppBlocking.Ignore;
                    break;
                case 2: //Ignored and Removed.
                    selectedAppBlockingFilter = LevelsOfAppBlocking.Remove;
                    break;
                default:
                    selectedAppBlockingFilter = LevelsOfAppBlocking.None;
                    break;

            }
            LoadList(selectedAppBlockingFilter, searchboxapp?.Text);

        }

        private void Searchboxapp_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            LoadList(selectedAppBlockingFilter, e.Text.ToString());
        }

        void LoadList(LevelsOfAppBlocking level, string searchText)
        {
            loadingblacklistitemsprogressbar.Visibility = Android.Views.ViewStates.Visible;
            blacklistRecyclerView.Visibility = Android.Views.ViewStates.Invisible;

            ThreadPool.QueueUserWorkItem(method =>
            { 

                if (string.IsNullOrEmpty(searchText))
                {
                    applist = Blacklist.GetListOfApps(level);
                }
                else
                {
                    applist = Blacklist.GetListOfApps(level).Where(x => PackageUtils.GetTheAppName(x.PackageName).Contains(searchText)).ToList();
                }
                RunOnUiThread(() =>
                {
                    blacklistRecyclerView.SetAdapter(new AppsToBeBlacklistedAdapter(applist));
                    loadingblacklistitemsprogressbar.Visibility = Android.Views.ViewStates.Invisible;
                    blacklistRecyclerView.Visibility = Android.Views.ViewStates.Visible;
                });
            }
            );
            

        }
    }
}