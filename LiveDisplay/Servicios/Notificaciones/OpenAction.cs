using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using LiveDisplay.Factories;
using System.Linq;

public class OpenAction : Java.Lang.Object
{
    Notification.Action _action;
    const string EncodedIconFieldMember = "icon.I";

    public OpenAction(Notification.Action action)
    {
        this._action = action;
    }

    public string Title => _action.Title.ToString();

    public RemoteInput[] RemoteInputs => _action.GetRemoteInputs();

    public RemoteInput FirstRemoteInput => (RemoteInputs != null && RemoteInputs.Length > 0) ? RemoteInputs.FirstOrDefault(ri => ri.ResultKey != null) : null;


    public bool IsDirectReply
    {
        get
        {
            //Direct reply action is a new feature in Nougat, so when called on Marshmallow and backwards, so in those cases an Action will never represent a Direct Reply.
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M) return false;

            return FirstRemoteInput != null;
        }
    }

    public Drawable Icon
    {
        get
        {
            Drawable actionIcon;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                actionIcon = IconFactory.ReturnActionIconDrawable(_action.Icon, _action.ActionIntent.CreatorPackage);
            }
            else
            {
                actionIcon = IconFactory.ReturnActionIconDrawable(_action.JniPeerMembers.InstanceFields.GetInt32Value(EncodedIconFieldMember, _action), _action.ActionIntent.CreatorPackage);
            }

            return actionIcon;
        }
    }


    public string PlaceholderTextForInlineResponse
    {
        get
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M) return string.Empty;

            return FirstRemoteInput?.Label;

        }
    }
    public PendingIntent ActionIntent => _action.ActionIntent;
}