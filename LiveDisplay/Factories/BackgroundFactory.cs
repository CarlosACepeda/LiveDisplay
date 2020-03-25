namespace LiveDisplay.Factories
{
    using Android.App;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;

    // TODO: Correct me, I can be optimized.
    internal class BackgroundFactory : Java.Lang.Object
    {
        public string SaveImagePath(Android.Net.Uri uri)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            string doc_id = "";
            using (var c1 = Application.Context.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                string document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = Application.Context.ContentResolver.Query(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
                configurationManager.SaveAValue(ConfigurationParameters.ImagePath, path);
            }
            return path;
        }
    }
}