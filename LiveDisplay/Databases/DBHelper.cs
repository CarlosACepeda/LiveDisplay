using Android.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Databases
{
    [Obsolete("No se usa porque infringe la regla de repetición de datos, duplica las Notificaciones")]
    internal class DBHelper
    {
        //private string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        //public bool CreateDatabase()
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            connection.CreateTable<ClsNotification>();
        //            return true;
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error en la conexión a la base de datos", ex.Message);
        //        return false;
        //    }
        //}

        //public bool InsertIntoTableNotification(ClsNotification notificacion)
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            connection.Insert(notificacion);
        //            return true;
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error.", ex.Message);
        //        return false;
        //    }
        //}

        //public List<ClsNotification> SelectTableNotification()
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            return connection.Table<ClsNotification>().ToList();
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error.", ex.Message);
        //        return null;
        //    }
        //}

        //public bool UpdateTableNotification(ClsNotification notificacion)
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            connection.Query<ClsNotification>("UPDATE ClsNotification set Titulo=?, Texto=?, Icono=? where Id=?",
        //                notificacion.Titulo, notificacion.Texto, notificacion.Icono, notificacion.Id);
        //            return true;
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error.", ex.Message);
        //        return false;
        //    }
        //}

        //public bool DeleteTableNotification(ClsNotification notificacion)
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            connection.Delete(notificacion);
        //            return true;
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error.", ex.Message);
        //        return false;
        //    }
        //}

        //public bool SelectQueryTableNotification(int id)
        //{
        //    try
        //    {
        //        using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "notifications.db")))
        //        {
        //            connection.Query<ClsNotification>("SELECT * FROM ClsNotificacion Where Id=?", id);
        //            return true;
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Log.Warn("Error.", ex.Message);
        //        return false;
        //    }
        //}
    }
}