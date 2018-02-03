using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Objects
{
    public class ClsNotification
    {
        public string Titulo { get; set; }
        public string Texto { get; set; }
        public Drawable Icono { get; set; }

    }
}