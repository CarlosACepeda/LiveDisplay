using System;
using System.Collections.Generic;

namespace LiveDisplay.Services
{
    public class ActivityLifecycleHelper
    {
        private List<Tuple<Type, ActivityStates>> Activities;

        private static ActivityLifecycleHelper instance;

        //Activities should invoke this to let know other entities about its state.
        public event EventHandler<ActivityStateChangedEventArgs> ActivityStateChanged;

        public static ActivityLifecycleHelper GetInstance()
        {
            if (instance == null)
            {
                instance = new ActivityLifecycleHelper();
            }
            return instance;
        }

        private ActivityLifecycleHelper()
        {
            Activities = new List<Tuple<Type, ActivityStates>>();
        }

        public void NotifyActivityStateChange(Type activity, ActivityStates activityState)
        {
            //Perform a search, and remove.
            foreach (var item in Activities)
            {
                if (item.Item1 == activity)
                {
                    Activities.Remove(item);
                    break;
                }
            }

            Activities.Add(new Tuple<Type, ActivityStates>(activity, activityState));
            ActivityStateChanged?.Invoke(this, new ActivityStateChangedEventArgs
            {
                Activity = activity,
                State = activityState
            });
        }

        public ActivityStates GetActivityState(Type activity)
        {
            foreach (var item in Activities)
            {
                if (item.Item1 == activity)
                {
                    return item.Item2;
                }
            }
            return ActivityStates.Unknown;
        }
    }

    public class ActivityStateChangedEventArgs : EventArgs
    {
        public Type Activity { get; set; }
        public ActivityStates State { get; set; }
    }

    public enum ActivityStates
    {
        Unknown = -1,
        Paused = 0,
        Resumed = 1,
        Destroyed = 2,
    }
}