using Android.Graphics;
using SQLite;

namespace LiveDisplay.Objects
{
    public class ClsNotification
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Titulo { get; set; }
        public string Texto { get; set; }
        public byte[] Icono { get; set; }
    }
}