#if MERCURY_LOCALNOTIFICATIONS
using System;
using System.Collections.Generic;

namespace Mercury.LocalNotifications
{
    public class LocalNotificationsControllerSample : LocalNotificationsController
    {
        protected override List<NotificationInfo_Periodic> GetPeriodicNotifications()
        {
            return new List<NotificationInfo_Periodic>
            {
                new NotificationInfo_Periodic
                {
                    Period = "Daily",
                    Data = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("Gift", "Coins"),
                        new DataTagValuePair("Amount",  "150")
                    },
                    FireTime   = DateTime.Today.AddSeconds(5).Subtract(DateTime.Now)
                },
                
                new NotificationInfo_Periodic
                {
                    Period = "Weekly",
                    Data = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("Gift",   "Gems"),
                        new DataTagValuePair("Amount", "30")
                    },
                    FireTime   = DateTime.Today.AddDays(1).Subtract(DateTime.Now)
                }
            };
        }

        protected override List<NotificationInfo_FreeResources> GetFreeResourcesNotifications()
        {
            return new List<NotificationInfo_FreeResources>
            {
                new NotificationInfo_FreeResources
                {
                    Identifier = "Coin",
                    Data = new DataTagValuePair("Amount", "150"),
                    FireTime = DateTime.Today.AddDays(1).Subtract(DateTime.Now)
                },
                
                new NotificationInfo_FreeResources
                {
                    Identifier = "Gems",
                    Data       = new DataTagValuePair("Amount", "30"),
                    FireTime   = DateTime.Today.AddDays(1).Subtract(DateTime.Now)
                },
            };
        }

        protected override List<NotificationInfo_Processes> GetProcessesNotifications()
        {
            return new List<NotificationInfo_Processes>
            {
                new NotificationInfo_Processes
                {
                    Identifier = "UpgradeFinished",
                    Data       = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("Building", "Barracks"), 
                        new DataTagValuePair("Level", "3")
                    },
                    FireTime   = new DateTime(637583616000000000).Subtract(DateTime.Now)
                },
                
                new NotificationInfo_Processes
                {
                    Identifier = "UpgradeFinished",
                    Data       = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("Building", "Market"),
                        new DataTagValuePair("LevelFrom", "7"),
                        new DataTagValuePair("LevelTo", "8")
                    },
                    FireTime   = new DateTime(637583616000000000).Subtract(DateTime.Now)
                },
                
                new NotificationInfo_Processes
                {
                    Identifier = "ChestOpen",
                    Data       = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("Type", "Epic")
                    },
                    FireTime   = new DateTime(637583796600000000).Subtract(DateTime.Now)
                },
                
                new NotificationInfo_Processes
                {
                    Identifier = "ExpeditionFinished",
                    Data       = new List<DataTagValuePair>
                    {
                        new DataTagValuePair("UnitName", "Shrek"),
                        new DataTagValuePair("ReturnStatus", "Successfully")
                    },
                    FireTime   = new DateTime(637583796600000000).Subtract(DateTime.Now)
                },
            };
        }

        protected override List<string> GetEnemyConnectedNotifications()
        {
            return new List<string> {"Mixo", "Aleko", "Bidzina", "Sonya", "Sandro"};
        }

        protected override List<LocalNotification> RescheduleCustomNotifications()
        {
            return new List<LocalNotification>
            {
                new LocalNotification()
                {
                    Title = "Example Title",
                    Text = "Example Title",
                    ChannelID = "ISwearICreatedChannel",
                    IconSmall = "icon_small_is_added",
                    IconLarge = "icon_large_is_surely_added",
                    Data = "010011100100111110100001110_LOL",
                    FireTimeDelay = TimeSpan.FromSeconds(5)
                }
            };
        }
    }
}
#endif