using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.Adapters;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcherrrr", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class Catcher : NotificationListenerService
    {

        public static NotificationAdapter adapter;
        public static List<StatusBarNotification> listaNotificaciones;
        public static Catcher catcherInstance;
        //Kitkat ListenerConnected variable.
        bool isConnected = false;
        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            catcherInstance = this;
            listaNotificaciones = GetActiveNotifications().ToList();
            adapter = new NotificationAdapter(listaNotificaciones);
            base.OnListenerConnected();
            Log.Info("Listener connected, list: ", listaNotificaciones.Count.ToString());
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            //Kitkat Dirty ListenerConnected.
            //No funciona si no hay una notificación nueva
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop && isConnected == false)
            {
                catcherInstance = this;
                listaNotificaciones = GetActiveNotifications().ToList();
                adapter = new NotificationAdapter(listaNotificaciones);
                isConnected = true;
                Log.Info("Kitkat Listener connected, list: ", listaNotificaciones.Count.ToString());

            }

            int id = sbn.Id;
            int indice = listaNotificaciones.IndexOf(listaNotificaciones.FirstOrDefault(o => o.Id == sbn.Id));
            //Condicional debido a que Play Store causa que algun item se pierda #wontfix ?
            if (indice >= 0)
            {
                listaNotificaciones.RemoveAt(indice);
                listaNotificaciones.Add(sbn);
                if (LockScreenActivity.lockScreenInstance != null)
                {
                    LockScreenActivity.lockScreenInstance.RunOnUiThread(() => adapter.NotifyItemChanged(indice));
                }

                Log.Info("Elemento actualizado", "Tamaño lista: " + listaNotificaciones.Count);
            }
            else
            {
                listaNotificaciones.Add(sbn);
                if (LockScreenActivity.lockScreenInstance != null)
                {
                    LockScreenActivity.lockScreenInstance.RunOnUiThread(() => adapter.NotifyItemInserted(listaNotificaciones.Count));
                }
                Log.Info("Elemento insertado", "Tamaño lista: " + listaNotificaciones.Count);
            }
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            int indice = listaNotificaciones.IndexOf(listaNotificaciones.FirstOrDefault(o => o.Id == sbn.Id));
            if (indice >= 0)
            {
                listaNotificaciones.RemoveAt(indice);
            }
            if (adapter != null && LockScreenActivity.lockScreenInstance != null)
            {
                LockScreenActivity.lockScreenInstance.RunOnUiThread(() => adapter.NotifyItemRemoved(indice));
                Log.Info("Remoción, tamaño lista:  ", listaNotificaciones.Count.ToString());
            }
        }
    }
}