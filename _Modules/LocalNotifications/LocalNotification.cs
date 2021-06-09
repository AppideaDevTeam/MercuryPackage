#if MERCURY_LOCALNOTIFICATIONS

namespace Mercury.LocalNotifications
{
    public class LocalNotification
    {
        public string Title;
        public string Text;

        #region ANDROID SPECIFIC
        public string IconSmall;
        public string IconLarge;
        
        public string ChannelID;
        #endregion

        public string Data;
        
        public System.TimeSpan FireTimeDelay;
    }
}
#endif