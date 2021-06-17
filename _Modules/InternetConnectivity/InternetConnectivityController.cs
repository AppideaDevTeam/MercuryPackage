#if MERCURY_INTERNETCONNECTIVITY
using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    public abstract class InternetConnectivityController : MercuryMonoBehaviorInstanceReferencer<InternetConnectivityController>
    {
        #region VARIABLES

        private static Coroutine _InternetConnectionCoroutine;
        private static Coroutine _TimePeriodicRenewalCoroutine;

        // MANAGER REFERENCES
        [ShowInInspector] public static  TimeInfo                       TimeInfo       => InternetConnectivityManager.TimeInfo;
        [ShowInInspector] public static  ConnectionInfo                 ConnectionInfo => InternetConnectivityManager.ConnectionInfo;
        private static InternetConnectivityDatabaseSO Database       => MercuryLibrarySO.InternetConnectivityDatabase;

        #endregion

        #region MAIN FUNCTIONS

        public static void Initialize() => InternetConnectivityManager.Initialize();

        public static void RunInternetCheckLoop() => Instance.RunInternetConnectionCheckingCoroutine();

        public static void StopInternetCheckLoop() => Instance.StopAllCoroutines();

        #endregion

        private void RunInternetConnectionCheckingCoroutine()
        {
            if(_InternetConnectionCoroutine != null) StopCoroutine(_InternetConnectionCoroutine);
            _InternetConnectionCoroutine = StartCoroutine(InternetConnectionCoroutine());
        }

        private IEnumerator InternetConnectionCoroutine()
        {
            WaitForSeconds waiter =  new WaitForSeconds(Database.InternetConnectionCheckingLoopInterval);
            
            bool lastAttemptFailed = false;
            int autoAttempted = 0;
                
            InternetConnectivityManager.LogMessage("Internet Check Started...");
            
            RetryInitialConnection:
            InternetConnectivityManager.CheckInternetConnection();
            autoAttempted++;

            if (ConnectionInfo.ConnectionEstablished)
            {
                autoAttempted = 0;
                InternetConnectionEstablished();
                InternetConnectivityManager.LogMessage("Internet Connection Established...");
                
                if (!TimeInfo.WasFetched)
                {
                    if(_TimePeriodicRenewalCoroutine != null) StopCoroutine(_TimePeriodicRenewalCoroutine);
                    _TimePeriodicRenewalCoroutine = StartCoroutine(TimePeriodicRenewalCoroutine());
                }
            }
            else
            {
                if (Database.InternetConnectionAutoRetryOnFail && autoAttempted < Database.InternetConnectionMaxAutoRetry)
                {
                    yield return waiter;
                    InternetConnectionNotEstablished(true);
                    InternetConnectivityManager.LogMessage("Internet Connection NOT Established... RETRY");
                    goto RetryInitialConnection;
                }
                InternetConnectionNotEstablished(false);
                InternetConnectivityManager.LogMessage("Internet Connection NOT Established... STOP");
                _InternetConnectionCoroutine = null;
                yield break;
            }
            
            InternetConnectivityManager.LogMessage("Internet Connection LOOP Started...");
            
            while (Database.InternetConnectionCheckingInLoop)
            {
                RetryLoopConnection:
                InternetConnectivityManager.LogMessage("Internet Connection LOOP Iteration...");
                yield return waiter;
                
                InternetConnectivityManager.CheckInternetConnection();

                if (ConnectionInfo.CurrentlyConnected)
                {
                    if (lastAttemptFailed)
                    {
                        lastAttemptFailed = false;
                        autoAttempted = 0;
                        InternetConnectionRestored();
                        InternetConnectivityManager.LogMessage("Internet Connection RESTORED");
                    }
                }
                else
                {
                    autoAttempted++;
                    if (Database.InternetConnectionAutoRetryOnLost && autoAttempted < Database.InternetConnectionMaxAutoRetry)
                    {
                        InternetConnectionLost(true);
                        InternetConnectivityManager.LogMessage("Internet Connection LOST... Retry");
                        lastAttemptFailed = true;
                        goto RetryLoopConnection;
                    }

                    InternetConnectionLost(false);
                    InternetConnectivityManager.LogMessage("Internet Connection LOST... STOP");
                    _InternetConnectionCoroutine = null;
                    yield break;
                }
            }
            _InternetConnectionCoroutine = null;
        }

        private IEnumerator TimePeriodicRenewalCoroutine()
        {
            WaitForSeconds waiter = new WaitForSeconds(Database.TimePeriodicRenewalInterval);
            InternetConnectivityManager.FetchInternetTime();
            InternetConnectivityManager.LogMessage($"Time Periodic Renewal Started... {TimeInfo.DateTime.ToString_HHMMSS()}");

            if(TimeInfo.WasFetched)InternetTimeFetched(InternetConnectivityManager.CalculateCurrentTime());
            else InternetTimeNotFetched();
            
            while (Database.TimePeriodicRenewal)
            {
                yield return waiter;
                InternetConnectivityManager.FetchInternetTime();
                
                if(TimeInfo.WasRenewed) InternetTimeRenewed(InternetConnectivityManager.CalculateCurrentTime());
                else InternetTimeNotRenewed();
                
                InternetConnectivityManager.LogMessage($"Time Periodic Renewal LOOP... {TimeInfo.DateTime.ToString_HHMMSS()}");
            }

            _TimePeriodicRenewalCoroutine = null;
        }

        #region ABSTRACT
        protected abstract void InternetConnectionEstablished();
        protected abstract void InternetConnectionNotEstablished(bool _retrying);
        protected abstract void InternetConnectionRestored();
        protected abstract void InternetConnectionLost(bool _reconnecting);
        protected abstract void InternetTimeFetched(DateTime _localDateTime);
        protected abstract void InternetTimeNotFetched();
        protected abstract void InternetTimeRenewed(DateTime _localDateTime);
        protected abstract void InternetTimeNotRenewed();
        #endregion

        private void OnDestroy() => InternetConnectivityManager.Reset();
    }
}
#endif
