using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
using LiveDisplay.Fragments.FragmentEventArgs;
using LiveDisplay.Services;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : HideableFragment, TextView.IOnEditorActionListener
    {
        public static event EventHandler<NotificationReadEventArgs> NotificationRead;

        OpenMessagingStyleNotification currentOpenNotification;

        List<OpenNotification> openNotifications = new List<OpenNotification>(); //The messaging style notifications.
        CardView maincontainer;
        TextView app_name, title, text;
        EditText response;
        Button action1, action2, action3;
        Button nextAppButton;

        AppCompatImageButton previous_message, next_message, send_response;

        const int RequestCodeMessagingStyleNotifications = 2;

        float initialX = 0;
        private float startPixelPoint;
        private long touchDownTime;
        float pixelToMoveTo = 0;
        private float totalPixelsMoved;
        bool isSetToDiscard = false;
        bool respondingToCurrentMessage = false;
        private bool isBeingAttached;

        #region Lifecycle events
        public override void OnAttach(Context context)
        {
            //if it's attaching then we can't use the hide or show methods, because they rely on Fragment transactions...
            isBeingAttached = true;
            base.OnAttach(context);
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            Console.WriteLine("On create called");
            CatcherHelper.RequestedOpenNotificationResultGenerated += CatcherHelper_RequestedOpenNotificationResultGenerated;
            base.OnCreate(savedInstanceState);
        }
        public override void OnHiddenChanged(bool hidden)
        {
            Console.WriteLine("OnHiddenChanged called");

            if (hidden) HideKeyboard();
        }
        public void HideKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)Application.Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(maincontainer.WindowToken, 0);
        }

        private void CatcherHelper_RequestedOpenNotificationResultGenerated(object sender, RequestedOpenNotificationGeneratedEventArgs e)
        {
            if (e.RequestCode == RequestCodeMessagingStyleNotifications)
            {
                if (e.OpenNotifications != null && e.OpenNotifications.Count > 0)
                {
                    //Now let's reorder them by certain criteria.
                    openNotifications =
                        e.OpenNotifications.
                        OrderByDescending(k => k.AppName).
                        OrderByDescending(k => k.When).
                        ToList();

                    //Finally, let's grab the first one from this list.
                    SetNotificationContent(openNotifications.First());
                }
                else
                {
                    Hide(); //No results so we hide the view.
                }
            }
        }
        void SetNotificationContent(OpenNotification notification)
        {
            bool isDirectReplyPresent = false;
            currentOpenNotification = new OpenMessagingStyleNotification(notification.UnderlyingStatusBarNotification);

            app_name.Text = currentOpenNotification.AppName;
            title.Text = currentOpenNotification.Title;
            text.Text = Build.VERSION.SdkInt <= BuildVersionCodes.P ? currentOpenNotification.Text :
                GetAppropiateMessageForMessagingStyleNotification(currentOpenNotification);

            int counter = 0;

            ClearActions();

            if (currentOpenNotification.HasActions)
                foreach (var action in notification.Actions)
                {
                    if (action.IsDirectReply)
                    {
                        isDirectReplyPresent = true;
                        HandleDirectReply(action);
                        continue;
                    }

                    DisplayAction(action, counter);
                    counter++;
                }
            CheckNotificationPosition();
            ToggleDirectReplyVisibility(isDirectReplyPresent);
        }
        void ClearActions()
        {
            action1.Click -= Action_Click;
            action2.Click -= Action_Click;
            action3.Click -= Action_Click;
            action1.Visibility = action2.Visibility = action3.Visibility = ViewStates.Invisible;
            action1.Text = action2.Text = action3.Text = string.Empty;
        }
        private void ToggleDirectReplyVisibility(bool isDirectReplyPresent)
        {
            response.Visibility = isDirectReplyPresent ? ViewStates.Visible : ViewStates.Gone;
            send_response.Visibility = isDirectReplyPresent ? ViewStates.Visible : ViewStates.Gone;
        }
        private string GetAppropiateMessageForMessagingStyleNotification(OpenMessagingStyleNotification notification)
        {
            string final_text = string.Empty;


            if (notification.Messages?.Count > 0)
                foreach (var message in notification.Messages)
                {
                    string message_text = string.Empty;

                    message_text = string.Concat(message.Sender, ": ", message.Text);

                    final_text += final_text == string.Empty ? message_text :
                            '\n' + message_text;
                }
            return final_text;
        }

        private void CheckNotificationPosition()
        {
            int index = GetOpenNotificationIndex(currentOpenNotification);
            next_message.Visibility = (index == openNotifications.Count - 1) ?
                ViewStates.Invisible : ViewStates.Visible;

            previous_message.Visibility = index == 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        void DisplayAction(OpenAction action, int index)
        {
            if (!action.IsDirectReply)
                switch (index)
                {
                    case 0:
                        action1.Visibility = ViewStates.Visible;
                        action1.Text = action.Title;
                        action1.Tag = action;
                        action1.Click += Action_Click;
                        break;
                    case 1:
                        action2.Visibility = ViewStates.Visible;
                        action2.Text = action.Title;
                        action2.Tag = action;
                        action2.Click += Action_Click;
                        break;
                    case 2:
                        action3.Visibility = ViewStates.Visible;
                        action3.Text = action.Title;
                        action3.Tag = action;
                        action3.Click += Action_Click;
                        break;
                }
        }

        private void Action_Click(object sender, EventArgs e)
        {
            var actionButton = (Button)sender;
            if (actionButton?.Tag is OpenAction openAction)
                NotificationSlave.GetInstance().ClickAction(openAction);
        }

        void HandleDirectReply(OpenAction action)
        {
            response.Hint = action.PlaceholderTextForInlineResponse;
            response.Tag = action;
        }
        public override void OnResume()
        {
            base.OnResume();
            Console.WriteLine("OnResume called");
            if (isBeingAttached)
            {
                isBeingAttached = false;
                //the fragment view is visible, so we don't need to call show, all we need to to is to request a notification.
                //using a secondary thread so the Attach process can be finished
                //and we can use Transactions
                RequestAllOpenMessagingNotifications();
            }
        }

        public override void OnPause()
        {
            Console.WriteLine("OnPause called");

            base.OnPause();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.Notification, container, false);

            maincontainer = v.FindViewById<CardView>(Resource.Id.container);
            app_name = v.FindViewById<TextView>(Resource.Id.app_name);
            nextAppButton = v.FindViewById<Button>(Resource.Id.next_app_button);
            title = v.FindViewById<TextView>(Resource.Id.title);
            text = v.FindViewById<TextView>(Resource.Id.text);
            response = v.FindViewById<EditText>(Resource.Id.response);
            response.SetOnEditorActionListener(this);

            action1 = v.FindViewById<Button>(Resource.Id.action_one);
            action2 = v.FindViewById<Button>(Resource.Id.action_two);
            action3 = v.FindViewById<Button>(Resource.Id.action_three);

            previous_message = v.FindViewById<AppCompatImageButton>(Resource.Id.previous_message);
            next_message = v.FindViewById<AppCompatImageButton>(Resource.Id.next_message);
            send_response = v.FindViewById<AppCompatImageButton>(Resource.Id.send_response);


            maincontainer.Touch += MainContainer_Touch;
            maincontainer.Click += MainContainer_Click; ;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;

            previous_message.Click += Previous_message_Click;
            next_message.Click += Next_message_Click;
            send_response.Click += Send_response_Click;

            Console.WriteLine("OnCreateView called");

            return v;
        }


        private void MainContainer_Click(object sender, EventArgs e)
        {
            NotificationSlave.GetInstance().ClickNotification(currentOpenNotification);
        }

        private void Send_response_Click(object sender, EventArgs e)
        {
            RespondToMessage();
        }

        void RespondToMessage()
        {
            var openAction = (OpenAction)response.Tag;

            NotificationSlave.GetInstance().ClickAction(openAction, response.Text);
            response.Text = string.Empty;
            response.ClearFocus();
            HideKeyboard();
        }

        private void Next_message_Click(object sender, EventArgs e)
        {
            int currentOpenNotificationIndex = GetOpenNotificationIndex(currentOpenNotification);
            int nextNotificationIndex = currentOpenNotificationIndex + 1;

            if (nextNotificationIndex > (openNotifications.Count - 1))
                return;

            SetNotificationContent(openNotifications[nextNotificationIndex]);
        }

        private void Previous_message_Click(object sender, EventArgs e)
        {
            int previousNotificationIndex = GetOpenNotificationIndex(currentOpenNotification) - 1;

            if (previousNotificationIndex < 0)
                return;

            SetNotificationContent(openNotifications[previousNotificationIndex]);
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {

            //Let's act when we are already showing a notification or a series of notifications.
            //only to be used when something is updated.
            if (e.OpenNotification.Style == OpenNotification.MessagingStyle)
            {
                if (e.UpdatesPreviousNotification)
                {
                    var openNotificationToUpdate = openNotifications.Where(o => o.Key == e.OpenNotification.Key)
                        .FirstOrDefault();
                    if (openNotificationToUpdate != null)
                    {
                        openNotifications.Remove(openNotificationToUpdate);
                        openNotifications.Add(e.OpenNotification);

                        if (currentOpenNotification.Key == e.OpenNotification.Key)
                        {
                            SetNotificationContent(e.OpenNotification);
                            Show();
                        }
                    }
                }
                else
                {
                    openNotifications.Add(e.OpenNotification);
                    SetNotificationContent(e.OpenNotification);
                    Show();
                }
            }
        }
        public override void OnDestroyView()
        {
            Console.WriteLine("OnDestroyView called");

            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            QuickGlanceFragment.ShowMessagesButtonClicked -= QuickGlanceFragment_ShowMessagesButtonClicked;
            base.OnDestroyView();
        }

        private void QuickGlanceFragment_ShowMessagesButtonClicked(object sender, EventArgs e)
        {
            if (!isBeingAttached)
            {
                if (IsHidden)
                {
                    if (CanShow())
                        Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public override void OnDestroy()
        {
            Console.WriteLine("OnDestroy called");

            CatcherHelper.RequestedOpenNotificationResultGenerated -= CatcherHelper_RequestedOpenNotificationResultGenerated;
            base.OnDestroy();
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            if (e.OpenNotification.Key == currentOpenNotification?.Key)
            {
                int notificationToBeRemovedIndex = GetOpenNotificationIndex(currentOpenNotification);
                int lastNotificationIndex = openNotifications.Count-1;
                openNotifications.Remove(currentOpenNotification);

                if (openNotifications.Count > 0)
                {
                    if (notificationToBeRemovedIndex == lastNotificationIndex)
                    {
                        Console.WriteLine("the notification removed was the last one, the next notification to be shown is the previous to the one that was removed.");
                        SetNotificationContent(openNotifications.LastOrDefault());
                    }
                    else
                    {
                        SetNotificationContent(openNotifications[notificationToBeRemovedIndex]); //Because after removal another one will take it's position.
                    }
                }

                Hide();
            }
        }

        private void MainContainer_Touch(object sender, View.TouchEventArgs e)
        {
            int discardThreshold = Resources.DisplayMetrics.WidthPixels / 5;
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    initialX = e.Event.GetX();
                    startPixelPoint = e.Event.RawX;
                    touchDownTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                    break;
                case MotionEventActions.Move:
                    {
                        pixelToMoveTo = e.Event.RawX - initialX;
                        totalPixelsMoved = startPixelPoint - e.Event.RawX;
                        totalPixelsMoved = totalPixelsMoved > 0 ? totalPixelsMoved : totalPixelsMoved * -1;

                        if (totalPixelsMoved >= discardThreshold)
                        {
                            Console.WriteLine("discard");
                            if (isSetToDiscard == false) //Prevents constant Vibration
                            {
                                maincontainer.PerformHapticFeedback(FeedbackConstants.Reject);
                            }
                            isSetToDiscard = true;
                        }
                        else
                        {
                            isSetToDiscard = false;
                            maincontainer.SetX(pixelToMoveTo);
                            float currentAlpha = 1 - ((totalPixelsMoved / 2) / discardThreshold);
                            currentAlpha = currentAlpha <= 0 ? 0 : currentAlpha >= 1 ? 1 : currentAlpha;
                            Console.WriteLine(currentAlpha);
                            maincontainer.Alpha = currentAlpha;
                        }
                    }
                    break;
                case MotionEventActions.Up:

                    int Xdiff = (int)(e.Event.GetX() - initialX);
                    if (Java.Lang.JavaSystem.CurrentTimeMillis() - touchDownTime < 100 && (Xdiff < 10))
                    {
                        maincontainer.PerformClick();
                    }
                    else
                    {
                        if (isSetToDiscard)
                        {
                            Console.WriteLine("Discarding");
                            NotificationSlave.GetInstance().CancelNotification(currentOpenNotification.Key);
                            isSetToDiscard = false;
                        }
                        ResetFragmentToOriginalPosition();
                    }

                    break;
            }
        }
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            Console.WriteLine("OnViewCreated NotificationFragment called");
            QuickGlanceFragment.ShowMessagesButtonClicked += QuickGlanceFragment_ShowMessagesButtonClicked;
            base.OnViewCreated(view, savedInstanceState);
        }
        void ResetFragmentToOriginalPosition()
        {
            Activity.RunOnUiThread(() =>
            {
                ValueAnimator animator = ValueAnimator.OfFloat(pixelToMoveTo, startPixelPoint - initialX);
                animator.SetInterpolator(new AccelerateInterpolator());
                animator.SetDuration(250);
                animator.Start();
                animator.Update += (sender, e) =>
                {
                    maincontainer.SetX((float)e.Animation.AnimatedValue);
                    maincontainer.Alpha = ((float)e.Animation.AnimatedValue - pixelToMoveTo) / ((startPixelPoint - initialX) - pixelToMoveTo);
                };
            });
        }
        void RequestAllOpenMessagingNotifications()
        {
            NotificationSlave.GetInstance().RequestOpenNotification(
               on => on.Style == OpenNotification.MessagingStyle, RequestCodeMessagingStyleNotifications);
        }

        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout(bool stop)
        {
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            
            //If the timeout has started, then cancel the action, and start again.

            //if (stop)
            //{
            //    maincontainer?.RemoveCallbacks(HideNotification); //Stop counting.
            //    return;
            //}
            //else
            //{
            //    if (timeoutStarted == true)
            //    {
            //        maincontainer?.RemoveCallbacks(HideNotification);
            //        maincontainer?.PostDelayed(HideNotification,7000);
            //    }
            //    //If not, simply wait 5 seconds then hide the notification, in that span of time, the timeout is
            //    //marked as Started(true)
            //    else
            //    {
            //        timeoutStarted = true;
            //        maincontainer?.PostDelayed(HideNotification, 7000);
            //    }
            //}
        }

        int GetOpenNotificationIndex(OpenNotification notification)
        {
            return openNotifications.FindIndex(n => n.Key == notification.Key);
        }

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Send)
            {
                RespondToMessage();
                return true;
            }
            return false;
        }

        public override bool CanShow()
        {
            return currentOpenNotification != null;
        }
    }
}