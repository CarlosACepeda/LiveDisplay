using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Models
{
    public class OpenPerson
    {
        public Icon Icon { get; set; }
        public bool IsBot { get; set; }
        public bool IsImportant { get; set; }
        public string Key { get; set; }
        public ICharSequence NameFormatted { get; set; }
        public string Name { get; set; }
    }
}