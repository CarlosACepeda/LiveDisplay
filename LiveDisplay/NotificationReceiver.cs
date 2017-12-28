using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LiveDisplay
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "notificationSender" }, DataMimeType = "text/plain")]
    public class NotificationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(Application.Context, "Received intent!", ToastLength.Short).Show();
            try
            {
                string titulo = intent.GetStringExtra("titulo");
                string texto = intent.GetStringExtra("texto");
                byte[] imagen = intent.GetByteArrayExtra("icon");
            }
            catch
            {
                Log.Info("Exception", "No se pudo obtener los datos de la notificación");
            }

        }

    }
}