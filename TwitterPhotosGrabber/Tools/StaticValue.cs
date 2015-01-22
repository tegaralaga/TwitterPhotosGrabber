using System;
using System.Collections.Generic;
using TwitterPhotosGrabber.Model;

namespace TwitterPhotosGrabber.Tools
{
    class StaticValue
    {

        private static StaticValue __instance = null;
        private static List<TwitterConsumer> __twitter_consumer_list = new List<TwitterConsumer>();
        private static TwitterConsumer __current_twitter_consumer;
        private static int __selected_application = 0;
        private static TwitterAccess __twitter_access = null;
        private static Boolean __is_login;

        public StaticValue()
        {
            __twitter_consumer_list.Clear();
            TwitterConsumer tc = new TwitterConsumer("Grab Photos 3", "0TDdy4DyU3U4Tl3PWjaKeqhrz", "vJFaMhYi7dGBfFf4Kc36V6UrZpov5MggezVpNl1MjHLMFZbOeY");
            TwitterConsumer tc1 = new TwitterConsumer("Grab Photos 2", "qObG5ecnlzU5sRTTqtBHnwLMa", "TjZPXB7TQ3J7Gdt8ugebxP5fPKDDXr7DXjQ8L2CQtTsMANosos");
            TwitterConsumer tc2 = new TwitterConsumer("Grab Photos 1", "uZRF6nioDBwopfGawYCu0KtDD", "W6jenbyg9vP68YqjUSGOni43O1uCFdzrJr71iCwnUCFiSArrqA");
            TwitterConsumer tc3 = new TwitterConsumer("Grab Photos", "E7IAJO4MoKCZa2h0YzJ2anDSI", "VAtnJSWuRgV9sBXeX0o6c5QM8id6SMUBJ83vn9dlO4dcW8mRGb");
            __twitter_consumer_list.Add(tc3);
            __twitter_consumer_list.Add(tc2);
            __twitter_consumer_list.Add(tc1);
            __twitter_consumer_list.Add(tc);
            __selected_application = Properties.Settings.Default.TWITTER_CURRENT_APPLICATION;
            __current_twitter_consumer = __twitter_consumer_list[__selected_application];
            __is_login = Properties.Settings.Default.IS_LOGIN;
            __twitter_access = (__is_login) ? new TwitterAccess(Properties.Settings.Default.TWITTER_ACCESS_TOKEN, Properties.Settings.Default.TWITTER_ACCESS_TOKEN_SECRET) : null;
        }

        public static StaticValue Instance
        {
            get
            {
                if (__instance == null)
                {
                    __instance = new StaticValue();
                }
                return __instance;
            }
        }

        public Boolean IS_LOGIN
        {
            get
            {
                return __is_login;
            }
            set
            {
                Properties.Settings.Default.IS_LOGIN = value;
                Properties.Settings.Default.Save();
                __is_login = value;
            }
        }

        public TwitterConsumer TWITTER_CONSUMER
        {
            get
            {
                return __current_twitter_consumer;
            }
            set
            {
                __current_twitter_consumer = value;
            }
        }

        public List<TwitterConsumer> TWITTER_CONSUMER_LIST
        {
            get
            {
                return __twitter_consumer_list;
            }
            set
            {
                __twitter_consumer_list = value;
            }
        }

        public TwitterAccess TWITTER_ACCESS
        {
            get
            {
                return __twitter_access;
            }
            set
            {
                __twitter_access = value;
            }
        }

        public int SELECTED_APPLICATION
        {
            get
            {
                return __selected_application;
            }
            set
            {
                Properties.Settings.Default.TWITTER_CURRENT_APPLICATION = value;
                Properties.Settings.Default.Save();
                __current_twitter_consumer = __twitter_consumer_list[value];
                __selected_application = value;
            }
        }

    }
}
