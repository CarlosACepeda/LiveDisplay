using LiveDisplay.Models;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Services.Notifications.NotificationEventArgs
{
    public class NotificationItemClickedEventArgs : EventArgs
    {
        public int Position { get; set; }
        public OpenNotification StatusBarNotification { get; set; }

        public int SiblingCount { get; set; } 
        public int ChildCount { get; set; } 
        public bool IsParent { get; set; }

        public bool IsFakeParent { get; set; }

        public bool IsStandalone { get; set; }
        public bool IsSibling { get; set; }

        public List<OpenNotification> Children { get; set; }
        public List<OpenNotification> Siblings { get; set; }

        public float NotificationViewX { get; set; }
        public int NotificationViewWidth { get; set; }
        
    }
}