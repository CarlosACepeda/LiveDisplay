using Android.Media.Session;
using LiveDisplay.Services.Media.Enums;
using LiveDisplay.Services.Notifications;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    public class ControlsAvailabilityChangedEventArgs: EventArgs
    {
        public bool TakeCustomActionsFromNotification { get; set; } = false;
        public AvailableControls AvailableControls { get; set; }
        public List<PlaybackState.CustomAction> CustomActions { get; internal set; } = new List<PlaybackState.CustomAction>();
        public OpenNotification OpenNotification { get; set; }
    }
}