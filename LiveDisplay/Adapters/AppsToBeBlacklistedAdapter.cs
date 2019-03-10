using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V7.Preferences;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
    internal class AppsToBeBlacklistedAdapter : RecyclerView.Adapter
    {
        private List<PackageInfo> items;
        private int selectedItem = -1;
        private string currentSelectedAppPackage = "";
        private LevelsOfAppBlocking levels;

        public AppsToBeBlacklistedAdapter(List<PackageInfo> listofapppackages)
        {
            items = listofapppackages;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.BlacklistItemRow, parent, false);
            return new AppsToBeBlacklistedAdapterViewHolder(itemView, App_Click);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            selectedItem = position;

            // Replace the contents of the view with that element
            var holder = viewHolder as AppsToBeBlacklistedAdapterViewHolder;
            holder.App.Text = PackageUtils.GetTheAppName(items[position].PackageName);
            holder.App.SetTag(Resource.String.blacklistitemtag, items[position].PackageName);
        }

        private void App_Click(AppClickedEventArgs e)
        {
            var textView = e.View;
            currentSelectedAppPackage = textView.GetTag(Resource.String.blacklistitemtag).ToString();
            using (Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(textView.Context))
            {
                builder.SetTitle(textView.Text);
                builder.SetMultiChoiceItems(new string[] { textView.Context.GetString(Resource.String.blacklisted), textView.Context.GetString(Resource.String.nonallowedtoturnonscreen) }, GetLevelOfBlocking(currentSelectedAppPackage), DialogMultichoiceClick);
                builder.SetPositiveButton("Ok", OnDialogPositiveButtonEventArgs);

                builder.Create();
                builder.Show();
            }
        }

        private void DialogMultichoiceClick(object sender, DialogMultiChoiceClickEventArgs e)
        {
            switch (e.Which)
            {
                case 0: //0 is 'Blacklisted'
                    if (e.IsChecked)
                        levels |= LevelsOfAppBlocking.Blacklisted;
                    else
                    {
                        levels &= ~LevelsOfAppBlocking.Blacklisted;
                    }

                    break;

                case 1: //1 is 'Non allowed to turn on screen'
                    if (e.IsChecked)
                        levels |= LevelsOfAppBlocking.NonAllowedToTurnScreenOn;
                    else
                    {
                        levels &= ~LevelsOfAppBlocking.NonAllowedToTurnScreenOn;
                    }
                    break;
            }
        }

        private bool[] GetLevelOfBlocking(string forWhichApp)
        {
            bool blacklisted = false;
            bool nonallowedtoturnscreenon = false;
            using (ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                var flag = configurationManager.RetrieveAValue(forWhichApp, 0);

                switch ((LevelsOfAppBlocking)flag)
                {
                    case LevelsOfAppBlocking.None:
                        //the booleans are alredy false.
                        break;

                    case LevelsOfAppBlocking.Blacklisted:
                        blacklisted = true;
                        break;

                    case LevelsOfAppBlocking.NonAllowedToTurnScreenOn:
                        nonallowedtoturnscreenon = true;
                        break;

                    case LevelsOfAppBlocking.TotallyBlocked:
                        blacklisted = true;
                        nonallowedtoturnscreenon = true;
                        break;

                    default:
                        break;
                }
            }
            return new bool[2] { blacklisted, nonallowedtoturnscreenon };
        }

        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            using (ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context)))
            {
                configurationManager.SaveAValue(currentSelectedAppPackage, (int)levels);
            }
        }

        public override int ItemCount => items.Count;
    }

    public class AppsToBeBlacklistedAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView App { get; set; }

        public AppsToBeBlacklistedAdapterViewHolder(View itemView, Action<AppClickedEventArgs> appClick) : base(itemView)
        {
            App = itemView.FindViewById<TextView>(Resource.Id.applicationname);
            App.Click += (sender, e) => appClick(new AppClickedEventArgs { View = App as AppCompatTextView, LayoutPosition = LayoutPosition });
        }
    }

    public class AppClickedEventArgs : EventArgs
    {
        public AppCompatTextView View { get; set; }
        public int LayoutPosition { get; set; }
    }
}