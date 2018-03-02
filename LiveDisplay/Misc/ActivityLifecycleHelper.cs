namespace LiveDisplay.Misc
{
    //Esta clase simplemente sirve para guardar el estado actual de la Activity.
    internal class ActivityLifecycleHelper
    {
        private static bool isActivityVisible;

        //Regresa el estado actual de la Activity.
        public bool IsActivityVisible()
        {
            return isActivityVisible;
        }

        //Actividad resumida, o iniciada.
        public void IsActivityResumed()
        {
            isActivityVisible = true;
        }

        //Actividad Pausada.
        public void IsActivityPaused()
        {
            isActivityVisible = false;
        }
    }
}