using Android.Media.Session;
using LiveDisplay.Misc;
using LiveDisplay.Services.Widget;
using System.Collections.Generic;

namespace LiveDisplay.Services.Media
{
    /// <summary>
    /// This class acts a a Listener for MediaSessions being created
    /// So, when a Session is created, I catch that Session and Use it to Control Media of tha session
    /// </summary>
    internal class ActiveMediaSessionsListener : Java.Lang.Object, MediaSessionManager.IOnActiveSessionsChangedListener
    {
        public void OnActiveSessionsChanged(IList<MediaController> controllers)
        {

        }
    }
}