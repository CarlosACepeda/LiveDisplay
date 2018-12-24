using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Servicios.Battery.BatteryEventArgs
{
    public class BatteryChangedEventArgs : EventArgs
    {
        public int BatteryLevel { get; set; }
        public BatteryLevelFlags BatteryLevelFlags { get; set; }
    }
}