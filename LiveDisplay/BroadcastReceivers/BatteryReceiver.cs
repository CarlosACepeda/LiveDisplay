using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using LiveDisplay.Servicios.Battery.BatteryEventArgs;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "Battery Receiver")]
    [IntentFilter(new[] { Intent.ActionBatteryChanged })]
    public class BatteryReceiver : BroadcastReceiver
    {
        private LevelListDrawable levelListDrawable;

        public static event EventHandler<BatteryChangedEventArgs> BatteryInfoChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            int batterylevel = intent.GetIntExtra(BatteryManager.ExtraLevel, 0);
            int batteryIcon = intent.GetIntExtra(BatteryManager.ExtraIconSmall, 100);
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch)
            {
                levelListDrawable = Application.Context.Resources.GetDrawable(batteryIcon, Application.Context.Resources.NewTheme()) as LevelListDrawable;
            }
            else
            {
#pragma warning disable
                levelListDrawable = Application.Context.Resources.GetDrawable(batteryIcon) as LevelListDrawable;
#pragma warning restore
            }

            OnBatteryInfoChanged(batterylevel, levelListDrawable);
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