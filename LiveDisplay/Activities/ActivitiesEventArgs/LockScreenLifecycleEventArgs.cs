using System;

namespace LiveDisplay.Activities.ActivitiesEventArgs
{
    public class LockScreenLifecycleEventArgs : EventArgs
    {
        public ActivityStates State { get; set; }
    }
}