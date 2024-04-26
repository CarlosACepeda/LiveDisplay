using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
namespace LiveDisplay.Models
{
    public class OpenMessage
    {
        public OpenPerson SenderPerson { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public long Time { get; set; }
    }
}