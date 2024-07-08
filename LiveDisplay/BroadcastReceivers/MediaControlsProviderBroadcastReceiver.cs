using Android.App;
using Android.Content;
using LiveDisplay.Services;
using System;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Exported = true)]
    public class MediaControlsProviderBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var mediaControls = MediaControlsBase.Instance;

            if (intent!= null)
            {
                if (intent.Action == MediaControlsProviderService.ActionStopCommand)
                    mediaControls.Stop();
                else if (intent.Action == MediaControlsProviderService.ActionCycleRepeatOptionCommand)
                    mediaControls.CycleRepeatOption();
            }

            Console.WriteLine(intent);
        }
    }
}