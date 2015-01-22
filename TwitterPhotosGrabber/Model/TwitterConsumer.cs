using System;

namespace TwitterPhotosGrabber.Model
{

    class TwitterConsumer
    {

        private string __key;
        private string __secret;
        private string __name;

        public TwitterConsumer(string name,string key, string secret)
        {
            __name = name;
            __key = key;
            __secret = secret;
        }

        public string KEY
        {
            get
            {
                return __key;
            }
            set
            {
                __key = value;
            }
        }

        public string SECRET
        {
            get
            {
                return __secret;
            }
            set
            {
                __secret = value;
            }
        }

        public string NAME
        {
            get
            {
                return __name;
            }
            set
            {
                __name = value;
            }
        }

    }

}
