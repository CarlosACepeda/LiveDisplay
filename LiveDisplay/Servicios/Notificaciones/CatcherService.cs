using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.Adapters;
using LiveDisplay.BroadcastReceivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcher", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class Catcher : NotificationListenerService
    {
        public static NotificationAdapter adapter;
        public static List<StatusBarNotification> listaNotificaciones;
        public static Catcher catcherInstance;
        private bool isListenerConnected = false;

        public override IBinder OnBind(Intent intent)
        {
            //KitKat Workaround: Enviar una notificación para poder iniciar la lista de notificaciones y obtener las notificaciones que hayan sido posteadas desde antes.
            //Porque parece imposible hacerlo sin otros métodos
            
            NotificationSlave slave = new NotificationSlave();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(700);
                //Fix me.
                slave.PostNotification();
                slave = null;
            });
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop && isListenerConnected == false)
            {
                catcherInstance = this;
                listaNotificaciones = GetActiveNotifications().ToList();
                adapter = new NotificationAdapter(listaNotificaciones);
                RegisterReceiver(new ScreenOnOffReceiver(), new IntentFilter(Intent.ActionScreenOn));
                isListenerConnected = true;
                Log.Info("KitkatListener connected, list: ", listaNotificaciones.Count.ToString());
            }

            return base.OnBind(intent);
        }

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
            catcherInstance = this;
            listaNotificaciones = GetActiveNotifications().ToList();
            adapter = new NotificationAdapter(listaNotificaciones);
            RegisterReceiver(new ScreenOnOffReceiver(), new IntentFilter(Intent.ActionScreenOn));
            RegisterReceiver(new ScreenOnOffReceiver(), new IntentFilter(Intent.ActionScreenOff));

            isListenerConnected = true;
            Log.Info("Listener connected, list: ", listaNotificaciones.Count.ToString());
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            //Si encuentra el indice significa que el id ya existe y por lo tanto la notificación debe ser actualizada
            int indice = listaNotificaciones.IndexOf(listaNotificaciones.FirstOrDefault(o => o.Id == sbn.Id && o.PackageName == sbn.PackageName));
            if (indice >= 0)
            {
                UpdateNotification(indice, sbn);
            }
            else
            {
                InsertNotification(sbn);
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
        private void UpdateNotification(int indice, StatusBarNotification sbn)
        {
            //Condicional debido a que Play Store causa que algun item se pierda #wontfix ?
            if (indice >= 0)
            {
                listaNotificaciones.RemoveAt(indice);
                listaNotificaciones.Add(sbn);
                try
                {
                    adapter.NotifyItemChanged(indice);
                }
                catch (Exception ex)
                {
                    Log.Wtf("Adapter exception!? Update Notification:", ex.ToString());
                }
                
                if (LockScreenActivity.lockScreenInstance!=null)
                { 
                    LockScreenActivity.lockScreenInstance.RunOnUiThread(() => LockScreenActivity.lockScreenInstance.OnNotificationUpdated());
                }
                
            }
            
            Log.Info("Elemento actualizado", "Tamaño lista: " + listaNotificaciones.Count);
        }
        private void InsertNotification(StatusBarNotification sbn)
        {
            listaNotificaciones.Add(sbn);

            try {
                adapter.NotifyItemInserted(listaNotificaciones.Count);
            }
            catch (Exception ex)
            {
                Log.Wtf("Adapter exception!? Insert Notification:", ex.ToString());
            }

            if (ScreenOnOffReceiver.isScreenOn == false)
            {
                //AwakeService awake = new AwakeService();
                //awake.UnlockScreen(this);
                Intent intent = new Intent(Application.Context, typeof(LockScreenActivity));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);

            }
            Log.Info("Elemento insertado", "Tamaño lista: " + listaNotificaciones.Count);
        }
        public override void OnListenerDisconnected()
        {
            base.OnListenerDisconnected();
            //Implementame
        }
        
    }
}