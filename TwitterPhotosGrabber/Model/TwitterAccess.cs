using System;
namespace TwitterPhotosGrabber.Model
{

    class TwitterAccess
    {

        private static string __token;
        private static string __token_secret;

        public TwitterAccess(string token, string token_secret)
        {
            __token = token;
            __token_secret = token_secret;
            Properties.Settings.Default.TWITTER_ACCESS_TOKEN = __token;
            Properties.Settings.Default.TWITTER_ACCESS_TOKEN_SECRET = __token_secret;
            Properties.Settings.Default.Save();
        }

        public string TOKEN
        {
            get
            {
                return __token;
            }
            set
            {
                Properties.Settings.Default.TWITTER_ACCESS_TOKEN = value;
                Properties.Settings.Default.Save();
                __token = value;
            }
        }

        public string TOKEN_SECRET
        {
            get
            {
                return __token_secret;
            }
            set
            {
                Properties.Settings.Default.TWITTER_ACCESS_TOKEN_SECRET = value;
                Properties.Settings.Default.Save();
                __token_secret = value;
            }
        }

    }

}
