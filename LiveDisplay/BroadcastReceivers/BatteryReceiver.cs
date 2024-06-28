using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using LiveDisplay.Services.Battery.BatteryEventArgs;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "Battery Receiver", Exported = true)]
    [IntentFilter(new[] { Intent.ActionBatteryChanged })]
    public class BatteryReceiver : BroadcastReceiver
    {
        private LevelListDrawable levelListDrawable;

        public static event EventHandler<BatteryChangedEventArgs> BatteryInfoChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            int batterylevel = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
            int batteryIcon = intent.GetIntExtra(BatteryManager.ExtraIconSmall, -1);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                levelListDrawable = Application.Context.Resources.GetDrawable(batteryIcon, Application.Context.Resources.NewTheme()) as LevelListDrawable;
            }
            else
            {
                levelListDrawable = Application.Context.Resources.GetDrawable(batteryIcon) as LevelListDrawable;
            }
            levelListDrawable.SetLevel(batterylevel);
            OnBatteryInfoChanged(batterylevel, levelListDrawable.Current);
        }

        private void OnBatteryInfoChanged(int batterylevel, Drawable batteryIcon)
        {
            BatteryInfoChanged?.Invoke(this, new BatteryChangedEventArgs
            {
                BatteryLevel = batterylevel,
                BatteryIcon = batteryIcon
            });
        }
    }
}