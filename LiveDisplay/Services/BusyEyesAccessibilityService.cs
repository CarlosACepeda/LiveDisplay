using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.Accessibility;
using System;
using System.Timers;

[Service(Enabled =true, Label = "@string/app_name", Permission = Manifest.Permission.BindAccessibilityService, Exported =true)]
[IntentFilter(new[] {ServiceInterface})]
[MetaData(ServiceMetaData, Resource = "@xml/accessibility_settings")]
class BusyEyesAccessibilityService : AccessibilityService
{
    Timer longPressTimer;
    bool elapsed = false;
    public static event EventHandler<EventArgs> VolumeUpDownLongPress;
    protected override void OnServiceConnected()
    {
        Console.WriteLine("SErvice connected");
        longPressTimer = new Timer
        {
            Interval = 500,
            AutoReset = false
        };
        longPressTimer.Elapsed += LongPressTimer_Elapsed;
        base.OnServiceConnected();
    }

    private void LongPressTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        elapsed = true;
    }

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        return; //We don't use this one.
    }

    public override void OnInterrupt()
    {
        Console.WriteLine("BusyEyes Interrupted");
    }
    public override bool OnUnbind(Intent intent)
    {
        Console.WriteLine("BusyEYES UNBIND");
        longPressTimer.Elapsed -= LongPressTimer_Elapsed;

        return base.OnUnbind(intent);
    }
    protected override bool OnKeyEvent(KeyEvent e)
    {
        Console.WriteLine("BusyEYES ONKEY" + e.Action);
        if (e.KeyCode == Keycode.VolumeDown || e.KeyCode == Keycode.VolumeUp)
        {
            if (e.Action == KeyEventActions.Down)
            {
                longPressTimer.Start();
            }
            if (e.Action == KeyEventActions.Up)
            {
                if (elapsed)
                {
                    Console.WriteLine("VOLUME LONG PRESS");
                    VolumeUpDownLongPress?.Invoke(null, null);
                }
                else
                {
                    longPressTimer.Stop();
                }
            }
        }

        return base.OnKeyEvent(e);
    }
}