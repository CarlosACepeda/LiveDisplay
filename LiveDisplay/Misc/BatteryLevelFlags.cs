using System;

namespace LiveDisplay.Misc
{
    [Flags]
    public enum BatteryLevelFlags
    {
        OverZero = 0,
        OverTwenty = 1,
        OverThirty = 2,
        OverFifty = 3,
        OverSixty = 4,
        OverEighty = 5,
        OverNinety = 6
    }
}