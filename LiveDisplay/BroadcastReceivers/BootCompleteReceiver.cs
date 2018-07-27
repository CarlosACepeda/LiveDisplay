﻿using Android.App;
using Android.Content;
using Android.Preferences;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

namespace LiveDisplay.BroadcastReceivers
{
    [BroadcastReceiver(Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        public override void OnReceive(Context context, Intent intent)
        {
            //Sólo iniciar el LockScreen si el usuario activa el lockScreen sin notificaciones
            if (sharedPreferences.GetBoolean(ConfigurationParameters.enabledlockscreennonotifications, false) == true)
            {
                //funciona, no se hace nada más aquí

                Intent lanzarLockScreen = new Intent(context, typeof(LockScreenActivity));
                lanzarLockScreen.AddFlags(ActivityFlags.NewTask);
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, lanzarLockScreen, 0);
                pendingIntent.Send();
            }
        }
    }
}