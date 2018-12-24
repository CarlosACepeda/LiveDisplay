using Android.App;
using Android.Content;
using Android.OS;
using LiveDisplay.DataRepository;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Battery.BatteryEventArgs;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Label = "Battery Receiver")]
    [IntentFilter(new[] { Intent.ActionBatteryChanged })]
    public class BatteryReceiver : BroadcastReceiver
    {
        private Battery battery = Battery.BatteryInstance();
        private BatteryLevelFlags batteryLevelFlags;

        public static event EventHandler<BatteryChangedEventArgs> BatteryInfoChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            int batterylevel = intent.GetIntExtra(BatteryManager.ExtraLevel, 0);
            Battery.BatteryLevel = batterylevel;
            Battery.BatteryLevelFlags = ReturnBatteryLevelFlag(batterylevel);
            OnBatteryInfoChanged(batterylevel);
        }

        private void OnBatteryInfoChanged(int batterylevel)
        {
            BatteryInfoChanged?.Invoke(this, new BatteryChangedEventArgs
            {
                BatteryLevel = batterylevel,
                BatteryLevelFlags = ReturnBatteryLevelFlag(batterylevel)
            });
        }

        private BatteryLevelFlags ReturnBatteryLevelFlag(int batterylevel)
        {
            if (batterylevel > 0)
            {
                batteryLevelFlags = BatteryLevelFlags.OverZero;
            }
            if (batterylevel > 20)
            {
                batteryLevelFlags = BatteryLevelFlags.OverTwenty;
            }
            if (batterylevel > 30)
            {
                batteryLevelFlags = BatteryLevelFlags.OverThirty;
            }
            if (batterylevel > 50)
            {
                batteryLevelFlags = BatteryLevelFlags.OverFifty;
            }
            if (batterylevel > 60)
            {
                batteryLevelFlags = BatteryLevelFlags.OverSixty;
            }
            if (batterylevel > 80)
            {
                batteryLevelFlags = BatteryLevelFlags.OverEighty;
            }
            if (batterylevel > 90)
            {
                batteryLevelFlags = BatteryLevelFlags.OverNinety;
            }
            return batteryLevelFlags;
        }
    }
}