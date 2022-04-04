using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using LiveDisplay.Enums;
using LiveDisplay.Misc;
using LiveDisplay.Models;
using LiveDisplay.Services.Widget;
using System;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    //Provides a basic way to show a notification on the lockscreen.
    public abstract class NotificationStyle: Java.Lang.Object, View.IOnClickListener
    {
        public string CUSTOM_VIEW_TAG = "TAG";

        public static event EventHandler<bool> SendInlineResponseAvailabityChanged;

        protected const int DefaultActionIdentificator = Resource.String.defaulttag;
        protected LinearLayout NotificationView { get; private set; }
        protected LinearLayout ActualNotification { get; private set; }

        public AndroidX.Fragment.App.Fragment NotificationFragment { get; private set; }
        protected LinearLayout NotificationActions { get; private set; }
        protected TextView Title { get; private set; }
        protected TextView Text { get; private set; }
        protected TextView ApplicationName { get; private set; }
        protected TextView When { get; private set; }
        protected TextView Subtext { get; private set; }
        protected ProgressBar NotificationProgress { get; private set; }
        protected ImageButton CloseNotification { get; private set; }
        protected LinearLayout InlineResponseNotificationContainer { get; private set; }
        //protected TextView PreviousMessages { get; private set; }
        protected ImageButton Collapse { get; private set; }
        protected EditText InlineResponse { get; private set; }
        protected ImageButton SendInlineResponse { get; private set; }
        protected ImageButton PreviousNotificationButton { get; private set; }
        protected ImageButton NextNotificationButton { get; private set; }
        protected OpenNotification OpenNotification { get; private set; }
        protected Resources Resources { get; private set; }

        public NotificationStyle(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
        {
            OpenNotification = openNotification;
            NotificationView = notificationView;
            
            NotificationFragment = notificationFragment;
            Resources = NotificationFragment.Resources;
            NotificationActions = FindView<LinearLayout>(Resource.Id.actions);
            ActualNotification = FindView<LinearLayout>(Resource.Id.actual_notification);
            Title = FindView<TextView>(Resource.Id.notification_title);
            Text = FindView<TextView>(Resource.Id.notification_text);
            ApplicationName = FindView<TextView>(Resource.Id.app_name);
            When = FindView<TextView>(Resource.Id.when);
            Subtext = FindView<TextView>(Resource.Id.subtext);
            NotificationProgress = FindView<ProgressBar>(Resource.Id.progress);
            CloseNotification = FindView<ImageButton>(Resource.Id.close_notification);
            InlineResponseNotificationContainer = FindView<LinearLayout>(Resource.Id.inline_notification_container);
            //PreviousMessages = FindView<TextView>(Resource.Id.previousMessages);
            Collapse = FindView<ImageButton>(Resource.Id.toggle_collapse);
            PreviousNotificationButton = FindView<ImageButton>(Resource.Id.previous_notification);
            NextNotificationButton = FindView<ImageButton>(Resource.Id.next_notification);
            
            InlineResponse = FindView<EditText>(Resource.Id.inline_text);
            SendInlineResponse = FindView<ImageButton>(Resource.Id.send_response);
            CloseNotification.Click += CloseNotification_Click;
            Collapse.Click += Collapse_Click;
        }

        protected virtual void Collapse_Click(object sender, EventArgs e)
        {
            //Do nothing, it depends on actual implementations.
        }

        protected virtual void CloseNotification_Click(object sender, EventArgs e)
        {
            ImageButton closenotificationbutton = sender as ImageButton;
            OpenNotification openNotification = closenotificationbutton.GetTag(DefaultActionIdentificator) as OpenNotification;
            NotificationHijackerWorker.RemoveNotification(openNotification);
            NotificationView.SetTag(Resource.String.defaulttag, openNotification?.GetCustomId);
            WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT });
            NotificationView.Visibility = ViewStates.Invisible;
        }

        public virtual void ApplyStyle()
        {
            SetWhen();
            SetTitle();
            SetText();
            SetSubtext();
            SetTextMaxLines();
            SetApplicationName();
            SetProgress();
            SetCloseButtonVisibility();
            SetCloseButtonTag();
            SetNotificationActionsVisibility();
            SetNotificationInlineRespVisibility();
            SetNotificationPreviousMessageVisibility();
            SetMoveBetweenSiblingNotificationsVisibility();
            ApplyActionsStyle();
            if (HasCustomView()) //Left by DecoratedCustomViewStyle.
            {
                RemoveCustomView();
            }
        }
        protected virtual void SetMoveBetweenSiblingNotificationsVisibility() 
        {
            if(NotificationHijackerWorker.NotificationAdapter.NotificationHasSiblings(OpenNotification))
            {
                PreviousNotificationButton.Visibility = ViewStates.Visible;
                NextNotificationButton.Visibility = ViewStates.Visible;
            }
            else
            {
                PreviousNotificationButton.Visibility = ViewStates.Invisible;
                NextNotificationButton.Visibility = ViewStates.Invisible;
            }
        }

        protected virtual void SetTitle()
        {
            Title.Text = OpenNotification.Title;
        }

        protected virtual void SetText()
        {
            Text.Text = OpenNotification.Text;
        }

        protected virtual void SetTextMaxLines()
        {
            Text.SetMaxLines(2);
        }

        protected virtual void SetApplicationName()
        {
            ApplicationName.Text = OpenNotification.ApplicationOwnerName;
        }

        protected virtual void SetWhen()
        {
            When.Text = OpenNotification.When;
        }

        protected virtual void SetProgress()
        {
            if (OpenNotification.MaximumProgress > 0)
            {
                NotificationProgress.Visibility = ViewStates.Visible;
                NotificationProgress.Indeterminate = OpenNotification.ProgressIndeterminate;
                NotificationProgress.Max = OpenNotification.MaximumProgress;
                NotificationProgress.Progress = OpenNotification.Progress;
            }
            else
            {
                NotificationProgress.Visibility = ViewStates.Gone;
            }
        }

        protected virtual void SetSubtext()
        {
            Subtext.Text = OpenNotification.SubText;
        }

        protected virtual void SetCloseButtonVisibility()
        {
            CloseNotification.Visibility = OpenNotification.IsRemovable ? ViewStates.Visible : ViewStates.Invisible;
        }

        protected virtual void SetCloseButtonTag()
        {
            CloseNotification.SetTag(DefaultActionIdentificator, OpenNotification);
        }

        protected virtual void SetNotificationActionsVisibility()
        {
            NotificationActions.Visibility = OpenNotification.HasActions() ? ViewStates.Visible : ViewStates.Invisible;
        }

        protected virtual void SetNotificationInlineRespVisibility()
        {
            InlineResponseNotificationContainer.Visibility = ViewStates.Invisible;
        }

        protected virtual void SetNotificationPreviousMessageVisibility()
        {
            //PreviousMessages.Text = string.Empty; //If a Messaging Style notification sets text here, then we clear it here when another instance of NotificationStyle is created.
            //PreviousMessages.Visibility = ViewStates.Gone;
        }

        public virtual void ApplyActionsStyle()
        {
            CleanActionViews();
            if (OpenNotification.HasActions())
            {
                NotificationActions.Visibility = ViewStates.Visible;
                var actions = OpenNotification.Actions;
                for (int i = 0; i <= actions.Count - 1; i++)
                {
                    Button actionButton = NotificationActions.GetChildAt(i) as Button;
                    actionButton.SetOnClickListener(this);

                    var action = actions[i];
                    OpenAction openAction = new OpenAction(action);
                    
                    if (i > 2)
                    {
                        actionButton.Visibility = ViewStates.Visible;
                    }
                    SetActionTag(actionButton, openAction);
                    
                    SetActionText(actionButton, openAction.Title());
                    SetActionIcon(actionButton, openAction);
                }
            }
            else
            {
                NotificationActions.Visibility = ViewStates.Gone;
            }
        }
        void CleanActionViews()
        {
            for (int i = 0; i <= NotificationActions.ChildCount-1; i++)
            {
                Button actionView= NotificationActions.GetChildAt(i) as Button;
                actionView.SetOnClickListener(null);

                if (i>2)
                {
                    actionView.Visibility = ViewStates.Gone; //Hide additional two action buttons.
                }
                actionView.SetTag(DefaultActionIdentificator, null);
                SetActionText(actionView, null);
                SetActionIcon(actionView, null);
            }
        }

        protected virtual void SetActionTextColor(Button action)
        {
            action.SetTextColor(Android.Graphics.Color.White);
        }

        protected virtual void SetActionTag(Button action, OpenAction openAction)
        {
            action.SetTag(DefaultActionIdentificator, openAction);
        }

        protected virtual void SetActionButtonGravity(Button action)
        {
            action.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
        }

        protected virtual void SetActionButtonTextMaxLines(Button action)
        {
            action.SetMaxLines(1);
        }

        protected virtual void SetActionButtonTextTypeface(Button action)
        {
            string typeface = "sans-serif-condensed";
            action.SetTypeface(Typeface.Create(typeface, TypefaceStyle.Normal), TypefaceStyle.Normal);
        }

        protected virtual void SetActionButtonBackground(Button action)
        {
            TypedValue outValue = new TypedValue();
            Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackground, outValue, true);
            action.SetBackgroundResource(outValue.ResourceId);
        }

        protected virtual void SetActionText(Button action, string text)
        {
            action.Text = text;
        }

        protected virtual void SetActionTextLowercase(Button action)
        {
            action.TransformationMethod = null;
        }

        protected virtual void SetActionIcon(Button action, OpenAction openAction)
        {
            if(openAction==null)
                action.SetCompoundDrawablesRelativeWithIntrinsicBounds(null, null, null, null);

            //In versions beyond Nougat the only notification that has icons is the MediaStyle one.
            if (Build.VERSION.SdkInt<= BuildVersionCodes.N  || OpenNotification.Style == NotificationStyles.MEDIA_STYLE)
                action.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction?.GetActionIcon(Color.White), null, null, null);
        }

        private void SendInlineResponse_Click(object sender, EventArgs e)
        {
            ImageButton actionButton = sender as ImageButton;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;
            openAction.SendInlineResponse(InlineResponse.Text);
            InlineResponse.Text = string.Empty;
            NotificationActions.Visibility = ViewStates.Visible;
            InlineResponseNotificationContainer.Visibility = ViewStates.Invisible;
            // Check if no view has focus:
            View view = NotificationFragment?.Activity?.CurrentFocus;
            if (view != null)
            {
                InputMethodManager imm = (InputMethodManager)NotificationFragment.Activity.GetSystemService(Context.InputMethodService);                
                imm.HideSoftInputFromInputMethod(view.WindowToken, 0);
            }
            SendInlineResponseAvailabityChanged?.Invoke(null, false);
        }

        protected T FindView<T>(int id) where T : View
        {
            return (T)NotificationView.FindViewById(id);
        }

        public void OnClick(View v)
        {
            Button actionButton = v as Button;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;
            if (openAction.ActionRepresentsDirectReply())
            {
                if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.EnableQuickReply))
                {
                    NotificationActions.Visibility = ViewStates.Invisible;
                    InlineResponseNotificationContainer.Visibility = ViewStates.Visible;
                    InlineResponse.Hint = openAction.GetPlaceholderTextForInlineResponse();
                    SendInlineResponse.SetTag(DefaultActionIdentificator, openAction);
                    SendInlineResponse.Click += SendInlineResponse_Click;
                    SendInlineResponseAvailabityChanged?.Invoke(null, true);
                }
            }
            else
            {
                openAction.ClickAction();
            }
        }
        bool HasCustomView()
        {
           return ActualNotification.FindViewWithTag(CUSTOM_VIEW_TAG) !=null;
        }

        void RemoveCustomView()
        {
            ActualNotification.RemoveView(NotificationView.FindViewWithTag(CUSTOM_VIEW_TAG));
        }
    }
}