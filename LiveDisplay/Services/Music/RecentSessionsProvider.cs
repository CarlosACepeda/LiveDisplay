using LiveDisplay.Misc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LiveDisplay.Services.Media
{
    public class RecentSessionsProvider
    {
        static RecentSessionsProvider instance;
        readonly ConfigurationManager configurationManager;
        const int maximumNumberOfSessions = 3;

        public static RecentSessionsProvider GetInstance()
        {
            instance??= new RecentSessionsProvider();

            
            return instance;
        }
        private RecentSessionsProvider()
        {
           configurationManager= new ConfigurationManager();
        }

        public void SaveSession(string sessionProviderPackageName)
        {
            var currentSessions = GetSavedSessions();

            if(currentSessions.Any(s=> s== sessionProviderPackageName))
            {
                currentSessions.Remove(sessionProviderPackageName);
            }

            
            if (currentSessions.Count >= maximumNumberOfSessions) currentSessions.Remove(currentSessions.Last());

            currentSessions.Add(sessionProviderPackageName);
            SaveSessions(currentSessions);
        }


        public void OpenApplication(string sessionProviderPackageName)
        {
            var intent= PackageUtils.GetAppIntent(sessionProviderPackageName);
            if (intent != null)
            {
                KeyguardPendingIntentMediator.GetInstance().SendIntent(intent);
            }
        }

       public ICollection<string> GetSavedSessions()
        {
            var sessions= configurationManager.RetrieveValues(ConfigurationParameters.RecentMediaSessions);

            if (sessions.Any(s => s == string.Empty))
            {
                sessions.Remove(string.Empty);
                SaveSessions(sessions);
            }

            return sessions;
        }
        void SaveSessions(ICollection<string> sessions)
        {
            configurationManager.SaveValues(ConfigurationParameters.RecentMediaSessions, sessions);
        }
        void BlockSessions(ICollection<string> sessions)
        {
            configurationManager.SaveValues(ConfigurationParameters.BlockedMediaSessions, sessions);
        }
        public ICollection<string> GetBlockedSessions()
        {
            ICollection<string> blockedSessions = configurationManager.RetrieveValues(ConfigurationParameters.BlockedMediaSessions);

            if (blockedSessions.Any(s => s == string.Empty))
            {
                blockedSessions.Remove(string.Empty);
                BlockSessions(blockedSessions);
            }
            return blockedSessions;
        }
    }
}