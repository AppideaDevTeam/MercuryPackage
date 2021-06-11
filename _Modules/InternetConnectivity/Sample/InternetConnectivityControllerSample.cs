using System;

namespace Mercury.InternetConnectivity
{
    public class InternetConnectivityControllerSample : InternetConnectivityController
    {
        private void Start()
        {
            Initialize();
            RunInternetCheckLoop();    
        }
        
        protected override void InternetConnectionEstablished() { }
        protected override void InternetConnectionNotEstablished() { }
        protected override void InternetConnectionRestored() {  }
        protected override void InternetConnectionLost() { }
    }
}
