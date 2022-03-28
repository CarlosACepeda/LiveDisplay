using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
    internal class AppsToBeBlacklistedAdapter : RecyclerView.Adapter
    {
        private readonly List<PackageInfo> items;
        private string currentSelectedAppPackage = "";
        private LevelsOfAppBlocking level;

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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            // Replace the contents of the view with that element
            var viewholder = holder as AppsToBeBlacklistedAdapterViewHolder;
            viewholder.App.Text = PackageUtils.GetTheAppName(items[position].PackageName);
            viewholder.App.SetTag(Resource.String.defaulttag, items[position].PackageName);
        }

        private void App_Click(AppClickedEventArgs e)
        {
            var textView = e.View;
            currentSelectedAppPackage = textView.GetTag(Resource.String.defaulttag).ToString();
            using (AlertDialog.Builder builder = new AlertDialog.Builder(textView.Context))
            {
                builder.SetTitle(textView.Text);
                builder.SetSingleChoiceItems(new string[] {textView.Context.GetString(Resource.String.none), textView.Context.GetString(Resource.String.ignore_notification), 
                    textView.Context.GetString(Resource.String.remove_notification) }, GetLevelOfBlocking(currentSelectedAppPackage) , DialogChoiceClick);
                builder.SetPositiveButton(textView.Context.GetString(Resource.String.ok), OnDialogPositiveButtonEventArgs);

                builder.Create();
                builder.Show();
            }
        }

        private void DialogChoiceClick(object sender, DialogClickEventArgs e)
        {
            switch (e.Which)
            {
                case 0:
                    level = LevelsOfAppBlocking.None;
                    break;
                case 1: 
                    level = LevelsOfAppBlocking.Ignore;
                    break;
                case 2:
                    level = LevelsOfAppBlocking.Remove;
                    break;
            }
        }

        private int GetLevelOfBlocking(string forWhichApp)
        {
            using (ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default))
            {
                var flag = configurationManager.RetrieveAValue(forWhichApp, 0);
                return flag;
            }
        }

        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            using (ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default))
            {
                configurationManager.SaveAValue(currentSelectedAppPackage, (int)level);
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