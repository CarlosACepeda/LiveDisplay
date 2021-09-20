using Android.Media.Session;
using LiveDisplay.Misc;
using LiveDisplay.Services.Widget;
using System.Collections.Generic;

namespace LiveDisplay.Services.Music
{
    /// <summary>
    /// This class acts a a Listener for MediaSessions being created
    /// So, when a Session is created, I catch that Session and Use it to Control Media of tha session
    /// through the Jukebox class.
    /// </summary>
    internal class ActiveMediaSessionsListener : Java.Lang.Object, MediaSessionManager.IOnActiveSessionsChangedListener
    {
        public void OnActiveSessionsChanged(IList<MediaController> controllers)
        {
            if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "0") == "0")
            {//0 equals 'Media Session'
                //Pick the best mediacontroller.
                if (controllers.Count > 0)
                    foreach (var mediacontroller in controllers)
                    {
                        if (mediacontroller?.GetTransportControls() != null)//Ensure that this session has transport controls we can control
                        {
                            try
                            {
                                MusicController.StartPlayback(mediacontroller.SessionToken, null);
                                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                                    new ShowParameters { Show = true, WidgetName = Constants.MUSIC_FRAGMENT });
                            }
                            catch
                            {
                                MusicController.StopPlayback(mediacontroller?.SessionToken);
                            }
                            break;
                        }
                    }
            }
        }
    }
}