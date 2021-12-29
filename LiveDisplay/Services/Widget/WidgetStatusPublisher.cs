using Android.Util;
using LiveDisplay.Enums;
using LiveDisplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Services.Widget
{
    //Widgets (fragments) should invoke the event! always, the reason is that the widgets had to know the state of all the widgets.
    //in order to show/hide them.
    public class WidgetStatusPublisher
    {
        //Convention: the last item in the currenActiveWidgets list is the one that should be showed by the lockscreen (or any other entity)

        static WidgetStatusPublisher instance;
        string widgetActiveForLimitedTime = string.Empty;
        const int ONE_SECOND_IN_MILLIS = 1000;

        readonly List<ShowParameters> currentActiveWidgets = new List<ShowParameters>();

        static System.Timers.Timer widgetActiveTimer = new System.Timers.Timer();

        public event EventHandler<WidgetStatusEventArgs> OnWidgetStatusChanged;


        private WidgetStatusPublisher()
        {
            widgetActiveTimer.Elapsed += WidgetActiveTimer_Elapsed;
            widgetActiveTimer.AutoReset = false;
        }

        private void WidgetActiveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WidgetActiveTimeHasConcluded();
        }
        void WidgetActiveTimeHasConcluded()
        {
            var widgetThatWillBeActive = currentActiveWidgets.ElementAt(currentActiveWidgets.Count - 2).WidgetName; 
            //Gets the active widget that's before the one that will be removed.

            NotifyWidgetAddition(widgetThatWillBeActive); //Putting this and the former line after the
            //assignment after the line 'widgetActiveForLimitedTime = string.Empty' will cause the previous method call
            //to never work, and the NotifyWidgetAddition event never fires, causing the widget that should be active to be never be notified
            //about it, why? I will never know. Wasted 2 hours here trying to fight with
            //an apparent Xamarin.Android bug, if you ever encounter this code, please don't move it. lol.

            //After notifying that the last active widget is not active, then we don't need it anymore
            NotifyWidgetRemoval(widgetActiveForLimitedTime);
            RemoveActiveWidget(widgetActiveForLimitedTime);
            widgetActiveForLimitedTime = string.Empty; //Reset value.
        }

        ShowParameters GetLastKnownOrDefaultWidget()
        {
            return new ShowParameters { WidgetName = WidgetTypes.CLOCK_FRAGMENT, Show = true, TimeToShow = ShowParameters.ACTIVE_PERMANENTLY };
            //The code below is made to return an active widget in case the Lockscreen is closed so we don't lose the reference about which one was 
            //active before closing the lockscreen.
            //however, this doesn't work correctly.
            //we are returning the clock. 
            //TODO: Find a way to keep track about the current active widget even when Lockscreen is closed.

            //switch (configurationManager.RetrieveAValue(ConfigurationParameters.LastActiveWidget, string.Empty))
            //{
            //    case WidgetTypes.CLOCK_FRAGMENT:
            //        return new ShowParameters { WidgetName = WidgetTypes.CLOCK_FRAGMENT, Show= true, TimeToShow= ShowParameters.ACTIVE_PERMANENTLY };
            //    case WidgetTypes.NOTIFICATION_FRAGMENT:
            //        return new ShowParameters { WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT, Show= true, TimeToShow= ShowParameters.WIDGET_MAY_DECIDE };
            //    case WidgetTypes.MUSIC_FRAGMENT:
            //        return new ShowParameters { WidgetName = WidgetTypes.MUSIC_FRAGMENT, TimeToShow = ShowParameters.WIDGET_MAY_DECIDE };
            //    default:
            //        return new ShowParameters { WidgetName = WidgetTypes.CLOCK_FRAGMENT, Show = true, TimeToShow = ShowParameters.ACTIVE_PERMANENTLY };
            //        //By design, the Clock is going to be the default value.
            //}
        }
        //This method is meant to be used by entities who want to get notified about the last Widget that was active.
        public void ShowActiveWidget()
        {
            var lastActiveWidget = GetCurrentActiveWidget();
            if (lastActiveWidget != null)
            {
                SetWidgetVisibility(lastActiveWidget);
            }
            else
            {
                currentActiveWidgets.Add(GetLastKnownOrDefaultWidget());
                SetWidgetVisibility(GetLastKnownOrDefaultWidget());
            }
        }

        public static WidgetStatusPublisher GetInstance()
        {
            if (instance == null)
                instance = new WidgetStatusPublisher();

            return instance;
        }

        public void SetWidgetVisibility(ShowParameters e)
        {
            if (string.IsNullOrEmpty(e.WidgetName))
                throw new InvalidOperationException(string.Format("Invalid {0}, please provide a correct value to {0}", nameof(e.WidgetName)));

            HandleActiveWidget(e);

            Log.Info("HandleActiveWidget: ",
                " WidgetName: " + GetCurrentActiveWidget().WidgetName.ToString()+
                " Show: " + GetCurrentActiveWidget().Show.ToString());
        }
        void HandleActiveWidget(ShowParameters e)
        {
            var activeWidget = GetCurrentActiveWidget();
            
            if (e.Show)
            {
                if (e.TimeToShow == ShowParameters.INVALID_TIME_TO_SHOW)
                    throw new InvalidOperationException(
                        string.Format(
                            "You must specify the amount of seconds this widget should be active, pass {0}.{1} to show it permanently",
                            nameof(ShowParameters), nameof(ShowParameters.ACTIVE_PERMANENTLY)));
            
                else if (e.TimeToShow > 0)
                {
                    widgetActiveTimer.Interval = e.TimeToShow * ONE_SECOND_IN_MILLIS;
                    widgetActiveTimer.Start();
                    widgetActiveForLimitedTime = e.WidgetName;
                }
            
                //now let's check that this widget is not already in the list of active Widgets
                if (activeWidget != null && activeWidget.WidgetName != e.WidgetName)
                {
                    NotifyWidgetRemoval(activeWidget.WidgetName);

                    ////If we have a time to show, we don't remove the widget from the list.
                    ////otherwise, we remove it, because the one with time to show must dissapear in certain amount of time.
                    ////then show the widget that was showing before the widget that has time to show.
                    //if (e.TimeToShow == ShowParameters.ACTIVE_PERMANENTLY) 
                    //{
                    //    RemoveCurrentActiveWidget();
                    //}
                    Console.WriteLine("Updating...");
                    Console.WriteLine(currentActiveWidgets.RemoveAll(caw=> caw.WidgetName == e.WidgetName));
                    currentActiveWidgets.Add(e); //This will be the new active widget
                    NotifyWidgetAddition(GetCurrentActiveWidget().WidgetName);
                }
                else
                {
                    //Just notify, it already exists within the list.
                    NotifyWidgetAddition(GetCurrentActiveWidget().WidgetName);
                }
            }
            else
            {
                if (activeWidget?.WidgetName == e.WidgetName)
                {
                    NotifyWidgetRemoval(e.WidgetName);
                    RemoveActiveWidget(e.WidgetName);
                    if (widgetActiveForLimitedTime == e.WidgetName)
                    {
                        //Cancel countdown.
                        widgetActiveTimer.Stop();
                        widgetActiveForLimitedTime = string.Empty;
                        NotifyWidgetAddition(GetCurrentActiveWidget().WidgetName);
                    }
                }
            }
        }
        void RemoveActiveWidget(string which)
        {
            currentActiveWidgets.RemoveAll(aw => aw.WidgetName == which);
        }
        ShowParameters GetCurrentActiveWidget()
        {
            var lastActiveWidget= currentActiveWidgets.LastOrDefault();
            return lastActiveWidget;
        }
        void NotifyWidgetRemoval(string which)
        {
            var widgetToBeRemoved = GetWidget(which);
            lock (widgetToBeRemoved)
            {
                OnWidgetStatusChanged?.Invoke(this, new WidgetStatusEventArgs
                {
                    AdditionalInfo = widgetToBeRemoved.AdditionalInfo,
                    Show = false,
                    WidgetName = widgetToBeRemoved.WidgetName
                });
            }
        }
        void NotifyWidgetAddition(string which)
        {
            var widgetToBeAdded = GetWidget(which);
            lock (widgetToBeAdded)
            {
                OnWidgetStatusChanged?.Invoke(this, new WidgetStatusEventArgs
                {
                    AdditionalInfo = widgetToBeAdded.AdditionalInfo,
                    Show = true,
                    WidgetName = widgetToBeAdded.WidgetName
                });
            }
        }
        ShowParameters GetWidget(string which)
        {
            return currentActiveWidgets.FirstOrDefault(w => w.WidgetName == which);
        }
    }
    public class ShowParameters
    {
        public const int ACTIVE_PERMANENTLY = 0;
        public const int INVALID_TIME_TO_SHOW = -1;
        public const int WIDGET_MAY_DECIDE = 2; //If we start the widget from somewhere external instead of the widget doing so by itself, then we pass this value.
        public bool Show { get; set; }
        public bool Active 
        {
            get
            { 
                if(TimeToShow> INVALID_TIME_TO_SHOW)
                {
                    return true;
                }
                return false;
            }
        }
        public int TimeToShow { get; set; } = INVALID_TIME_TO_SHOW;
        public object AdditionalInfo { get; set; }
        public string WidgetName { get; set; }
    }
    public class WidgetStatusEventArgs : EventArgs
    {
        public object AdditionalInfo { get; set; } //An aditional object if the widget  being shown needs it to show or hide correctly.
        public string WidgetName { get; set; }
        public bool Show { get; set; }
    }
}