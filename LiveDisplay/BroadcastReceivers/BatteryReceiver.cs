using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using LiveDisplay.Services.Battery.BatteryEventArgs;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "Battery Receiver", Exported = false)]
    [IntentFilter(new[] { Intent.ActionBatteryChanged, Intent.ActionPowerConnected, Intent.ActionPowerDisconnected })]
    public class BatteryReceiver : BroadcastReceiver
    {
        public static event EventHandler<BatteryChangedEventArgs> BatteryInfoChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            int batterylevel = intent.GetIntExtra(BatteryManager.ExtraLevel, 0);
            BatteryStatus status = (BatteryStatus)intent.GetIntExtra(BatteryManager.ExtraStatus, -1);
            bool isCharging = status == BatteryStatus.Charging ||
                            status == BatteryStatus.Full;

            OnBatteryInfoChanged(batterylevel, GetBatteryIcon(isCharging, batterylevel));
        }

        private void OnBatteryInfoChanged(int batterylevel, Drawable batteryIcon)
        {
            BatteryInfoChanged?.Invoke(null, new BatteryChangedEventArgs
            {
                BatteryLevel = batterylevel,
                BatteryIcon = batteryIcon
            });
        }
        Drawable GetBatteryIcon(bool isCharging, int batterylevel)
        {

            Drawable batteryIcon =null;
            if (batterylevel <= 20)
            {
                batteryIcon= 
                    !isCharging? 
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_10_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_10_white_18dp, Application.Context.Resources.NewTheme());

            }
            else if (batterylevel <= 40)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_20_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_20_white_18dp, Application.Context.Resources.NewTheme());

            }
            else if (batterylevel <= 60)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_40_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_40_white_18dp, Application.Context.Resources.NewTheme());

            }
            else if (batterylevel <= 80)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_60_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_60_white_18dp, Application.Context.Resources.NewTheme());

            }
            else if (batterylevel <= 90)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_80_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_80_white_18dp, Application.Context.Resources.NewTheme());
            }
            else if (batterylevel < 100)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_90_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_90_white_18dp, Application.Context.Resources.NewTheme());
            }
            else if (batterylevel == 100)
            {
                batteryIcon =
                    !isCharging ?
                    Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_white_18dp, Application.Context.Resources.NewTheme())
                    : Application.Context.Resources.GetDrawable(Resource.Drawable.ic_battery_charging_white_18dp, Application.Context.Resources.NewTheme());
            }
            return batteryIcon;
        }
    }
}