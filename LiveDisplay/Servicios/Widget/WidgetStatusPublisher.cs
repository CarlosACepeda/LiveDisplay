using LiveDisplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios.Widget
{
    //Widgets (fragments) should invoke the event! always, the reason is that the widgets had to know the state of all the widgets.
    //in order to show/hide them.
    public class WidgetStatusPublisher
    {
        static WidgetStatusPublisher instance;
        public string CurrentActiveWidget = string.Empty;
        const int ONE_SECOND_IN_MILLIS = 1000;
        ConfigurationManager configurationManager;

        List<ShowParameters> currentActiveWidgets = new List<ShowParameters>();

        static System.Timers.Timer widgetActiveTimer = new System.Timers.Timer();

        public event EventHandler<WidgetStatusEventArgs> OnWidgetStatusChanged;


        private WidgetStatusPublisher()
        {
            widgetActiveTimer.Elapsed += WidgetActiveTimer_Elapsed;
            widgetActiveTimer.AutoReset = false;
            configurationManager = new ConfigurationManager(AppPreferences.Default);
        }

        private void WidgetActiveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WidgetActiveTimeHasConcluded();
        }
        void WidgetActiveTimeHasConcluded()
        {
            SetWidgetVisibility(new ShowParameters
            {
                AdditionalInfo = GetLastActiveWidget().AdditionalInfo,
                Show = false, //If the widget was active it means that it is showing to the user currently, order it to stop.
                WidgetName = GetLastActiveWidget().WidgetName
            });

            //After notifying that the last active widget is not active, then we don't need it anymore
            RemoveLastActiveWidget();
        }

        ShowParameters GetLastKnownOrDefaultWidget()
        {
            switch (configurationManager.RetrieveAValue(ConfigurationParameters.LastActiveWidget, string.Empty))
            {
                case Constants.CLOCK_FRAGMENT:
                    return new ShowParameters { WidgetName = Constants.CLOCK_FRAGMENT, Show= true, Active= true, TimeToShow= ShowParameters.ACTIVE_PERMANENTLY };
                case Constants.NOTIFICATION_FRAGMENT:
                    return new ShowParameters { WidgetName = Constants.NOTIFICATION_FRAGMENT };
                case Constants.MUSIC_FRAGMENT:
                    return new ShowParameters { WidgetName = Constants.MUSIC_FRAGMENT };
                default:
                    return new ShowParameters { WidgetName = Constants.CLOCK_FRAGMENT, Show = true, Active = true, TimeToShow = ShowParameters.ACTIVE_PERMANENTLY };
                    //By design, the Clock is going to be the default value.
            }
        }
        //This method is meant to be used by entities who want to get notified about the last Widget that was active.
        //Currently meant to be used 
        public void ShowActiveWidget()
        {
            var lastActiveWidget = GetLastActiveWidget();
            if (lastActiveWidget != null) SetWidgetVisibility(lastActiveWidget);
            else SetWidgetVisibility(GetLastKnownOrDefaultWidget());
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

            OnWidgetStatusChanged?.Invoke(null, new WidgetStatusEventArgs
            {
                AdditionalInfo = e.AdditionalInfo,
                Show = e.Show,
                WidgetName = e.WidgetName
            });
            
        }
        void HandleActiveWidget(ShowParameters e)
        {
            lock (this)
            {
                if (e.Active && !e.Show) throw new InvalidOperationException(
                    string.Format(
                        "You can't hide a Widget and have it as Active at the same time, check that {0} is false or that's not set it at all and that {1} is false"
                    , nameof(e.Active), nameof(e.Show)));

                if (e.TimeToShow > ShowParameters.INVALID_TIME_TO_SHOW && !e.Active)
                    throw new InvalidOperationException("When setting time to show, you have to set it as active as well");


                var activeWidget = GetLastActiveWidget();

                if (e.Show)
                {
                    if (e.Active)
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
                        }

                        //now let's check if there's an already Active Widget.
                        if (activeWidget != null)
                        {
                            if (activeWidget.WidgetName == e.WidgetName)
                            {
                                //Removing Existent active widget
                                currentActiveWidgets.Remove(activeWidget);
                            }
                        }
                        currentActiveWidgets.Add(e);

                    }
                }
                else
                {
                    if (activeWidget?.WidgetName == e.WidgetName)
                        RemoveLastActiveWidget();

                }
                configurationManager.SaveAValue(ConfigurationParameters.LastActiveWidget, GetLastActiveWidget() == null ? string.Empty : GetLastActiveWidget().WidgetName); //Allows #GetLastKnownWidgetOrDefault() to work as expected.
            }
        }
        ShowParameters GetLastActiveWidget()
        {
            var lastActiveWidget= currentActiveWidgets.LastOrDefault();
            //if (lastActiveWidget == null)
            //    return GetLastKnownOrDefaultWidget();
            return lastActiveWidget;
        }
        void RemoveLastActiveWidget()
        {
            if(currentActiveWidgets.Count()>0)
                currentActiveWidgets.RemoveAt(currentActiveWidgets.Count()-1);
        }
    }
    public class ShowParameters
    {
        public const int ACTIVE_PERMANENTLY = 0;
        public const int INVALID_TIME_TO_SHOW = -1;
        public bool Show { get; set; }
        public bool Active { get; set; }
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