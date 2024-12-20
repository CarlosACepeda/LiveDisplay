using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using System;
using System.Threading;
using Fragment = AndroidX.Fragment.App.Fragment;

public abstract class HideableFragment : Fragment
{
    public override void OnAttach(Context context)
    {
        Console.WriteLine("OnAttach HideableFragment!!!!");

        base.OnAttach(context);
    }
    public override void OnViewCreated(View view, Bundle savedInstanceState)
    {
        Console.WriteLine("OnViewCreated HideableFragment!!!!");
        base.OnViewCreated(view, savedInstanceState);
    }
    public virtual void Hide()
    {
        try
        {
            ThreadPool.QueueUserWorkItem(m =>
            CommitFragmentOperation(true)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    public virtual void Show()
    {
        try
        {
            ThreadPool.QueueUserWorkItem(m =>
            CommitFragmentOperation(false)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    void CommitFragmentOperation(bool hide)
    {
        Activity.RunOnUiThread(() =>
        {
            var tran = Activity.SupportFragmentManager.BeginTransaction();
            if(hide)
            {
                tran.SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentClose);
                tran.Hide(this);
            }
            else
            {
                tran.SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentOpen);
                tran.Show(this);
            }
            tran.CommitNowAllowingStateLoss();
        }
        );
    }
    public virtual bool CanShow() => true; //Subclasses can specify whether Show method works or not, specifying a custom condition, like availability of data.
}