using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tweetinvi;
using System.Collections.Generic;
using System.Linq;
using TwitterPhotosGrabber.Tools;

namespace TwitterPhotosGrabber
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {

        private StaticValue __sv = StaticValue.Instance;
        private int TWITTER_TYPE_ORDER;
        private int TWITTER_GRAB_MODE;
        private Boolean TWITTER_INCLUDE_RTS = false;
        private Boolean TWITTER_INCLUDE_REPLIES = false;
        private string TWITTER_USERNAME;
        private string TWITTER_FOLDER_NAME;
        private string TWITTER_SINCE_ID;
        private string TWITTER_MAX_ID;
        private string TWITTER_ID;
        private bool FIRST_ATTEMPT_LOOP = true;
        private int PROCESSED_TWEET = 0;
        private int PROCESSED_IMAGES = 0;
        private int PROCESSED_IMAGES_LOCAL = 0;
        private int TRY_DOWNLOAD_ATTEMPT = 0;
        private int TWITTER_MAX_RESULT = 50;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken token = tokenSource.Token;
        private Boolean ALREADY_TERMINATE = false;

        public Dashboard()
        {
            InitializeComponent();
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            ComboBox_Order.Items.Add("Ascending");
            ComboBox_Order.Items.Add("Descending");
            ComboBox_Mode.Items.Add("Crawl (Slower, More Complete Result)");
            ComboBox_Mode.Items.Add("Search (Faster, Less Complete Result)");
            ComboBox_MaxResult.Items.Add("10");
            ComboBox_MaxResult.Items.Add("20");
            ComboBox_MaxResult.Items.Add("25");
            ComboBox_MaxResult.Items.Add("50");
            ComboBox_MaxResult.Items.Add("75");
            ComboBox_MaxResult.Items.Add("100");
            ComboBox_MaxResult.Items.Add("150");
            ComboBox_MaxResult.Items.Add("200");
            ComboBox_MaxResult.SelectedIndex = 3;
            ComboBox_Order.SelectedIndex = 0;
            ComboBox_Mode.SelectedIndex = 0;
            TWITTER_INCLUDE_RTS = CheckBox_RTS.IsChecked.Value;
            TWITTER_INCLUDE_REPLIES = CheckBox_Replies.IsChecked.Value;
            TWITTER_TYPE_ORDER = ComboBox_Order.SelectedIndex;
            TWITTER_GRAB_MODE = ComboBox_Mode.SelectedIndex;
            TwitterCredentials.SetCredentials(__sv.TWITTER_ACCESS.TOKEN, __sv.TWITTER_ACCESS.TOKEN_SECRET, __sv.TWITTER_CONSUMER.KEY, __sv.TWITTER_CONSUMER.SECRET);
        }

        private void Browse_Folder(object sender, RoutedEventArgs rea)
        {
            VistaFolderBrowserDialog vfbd = new VistaFolderBrowserDialog();
            vfbd.ShowNewFolderButton = true;
            vfbd.ShowDialog();
            if (vfbd.SelectedPath != null)
            {
                if (vfbd.SelectedPath.Trim().Length > 0)
                    TextBox_Browse_Folder.Text = vfbd.SelectedPath.Trim() + System.IO.Path.DirectorySeparatorChar.ToString();
            }
        }

        private void Grab_Content(object sender, RoutedEventArgs rea)
        {
            FIRST_ATTEMPT_LOOP = true;
            PROCESSED_TWEET = 0;
            PROCESSED_IMAGES = 0;
            PROCESSED_IMAGES_LOCAL = 0;
            TRY_DOWNLOAD_ATTEMPT = 0;
            ALREADY_TERMINATE = false;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            if (TextBox_Username.Text.Trim().Length > 0)
            {
                TWITTER_USERNAME = TextBox_Username.Text.Trim();
                if (TextBox_Browse_Folder.Text.Trim().Length > 0)
                {
                    String drive = System.IO.Path.GetPathRoot(TextBox_Browse_Folder.Text.Trim());
                    if (Directory.Exists(drive))
                    {
                        if (!Directory.Exists(TextBox_Browse_Folder.Text.Trim()))
                        {
                            Directory.CreateDirectory(TextBox_Browse_Folder.Text.Trim());
                        }
                        TWITTER_FOLDER_NAME = TextBox_Browse_Folder.Text.Trim();
                        if (TextBox_Since.Text.Trim().Length > 0)
                        {
                            TWITTER_SINCE_ID = TextBox_Since.Text.Trim();
                        }
                        else
                        {
                            TWITTER_SINCE_ID = "0";
                        }
                        if (TextBox_Max.Text.Trim().Length > 0)
                        {
                            TWITTER_MAX_ID = TextBox_Max.Text.Trim();
                        }
                        else
                        {
                            TWITTER_MAX_ID = "0";
                        }
                        Task.Factory.StartNew(() =>
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Command_Bar.Text = "";
                                Status_Bar_Text.Text = "";
                                //ProgressIndicator.IsBusy = true;
                                Enable_Control(false);
                                Button_Grab.Visibility = Visibility.Hidden;
                                Button_Terminate.Visibility = Visibility.Visible;
                            }));
                            TwitterImageGrabber();
                        }, token).ContinueWith((task) =>
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                //ProgressIndicator.IsBusy = false;
                                Enable_Control(true);
                                Button_Grab.Visibility = Visibility.Visible;
                                Button_Terminate.Visibility = Visibility.Hidden;
                            }));
                        }, TaskScheduler.FromCurrentSynchronizationContext()
                        );
                    }
                    else
                    {
                        MessageBox.Show("DRIVE TIDAK DITEMUKAN!", "ERROR");
                    }
                }
                else
                {
                    MessageBox.Show("HARAP TENTUKAN FOLDER TUJUAN!", "ERROR");
                }
            }
            else
            {
                MessageBox.Show("USERNAME HARUS DIISI!", "ERROR");
            }
        }

        private void Enable_Control(Boolean enable)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                TextBox_Username.IsEnabled = enable;
                TextBox_Browse_Folder.IsEnabled = enable;
                TextBox_Max.IsEnabled = enable;
                TextBox_Since.IsEnabled = enable;
                ComboBox_MaxResult.IsEnabled = enable;
                ComboBox_Mode.IsEnabled = enable;
                ComboBox_Order.IsEnabled = enable;
                Button_Browse_Folder.IsEnabled = enable;
                CheckBox_Replies.IsEnabled = enable;
                CheckBox_RTS.IsEnabled = enable;
            }));
        }

        private void Terminate_Grab(object sender, RoutedEventArgs rea)
        {
            try
            {
                if (token != null)
                    tokenSource.Cancel();
                //ProgressIndicator.IsBusy = false;
                Enable_Control(true);
                Button_Grab.Visibility = Visibility.Visible;
                Button_Terminate.Visibility = Visibility.Hidden;
                ALREADY_TERMINATE = true;
                Dispatcher.Invoke((Action)(() =>
                {
                    Status_Bar_Text.Text = "TASK TERMINATED";
                }));
            }
            catch (Exception e)
            {
                Utils.WriteLine("ERROR #2 : " + e.Message);
            }
        }

        //private void RecentSelectedChanged(object sender, SelectionChangedEventArgs scea)
        //{
        //    int index = ComboBox_Recent.SelectedIndex;
        //    if (index == 0)
        //    {
        //        TextBox_Username.Text = "";
        //        TextBox_Browse_Folder.Text = "";
        //        TextBox_Since.Text = "";
        //        TextBox_Max.Text = "";
        //    }
        //    else
        //    {
        //        try
        //        {
        //            MUser user = CommonCons.TWITTER_USERS.ElementAt(index - 1);
        //            if (user != null)
        //            {
        //                TextBox_Username.Text = user.USERNAME;
        //                TextBox_Browse_Folder.Text = user.PATH;
        //                TextBox_Since.Text = user.SINCE;
        //                TextBox_Max.Text = user.MAX;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Dispatcher.Invoke((Action)(() =>
        //            {
        //                Status_Bar_Text.Text = "ERROR #3 : " + e.Message;
        //            }));
        //        }
        //    }
        //}

        private void OrderSelectedChanged(object sender, SelectionChangedEventArgs scea)
        {
            TWITTER_TYPE_ORDER = ComboBox_Order.SelectedIndex;
        }

        private void MaxResultSelectedChanged(object sender, SelectionChangedEventArgs scea)
        {
            TWITTER_MAX_RESULT = Convert.ToInt32(ComboBox_MaxResult.SelectedValue.ToString());
        }

        private void ModeSelectedChanged(object sender, SelectionChangedEventArgs scea)
        {
            TWITTER_GRAB_MODE = ComboBox_Mode.SelectedIndex;
        }

        private void RTCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            TWITTER_INCLUDE_RTS = CheckBox_RTS.IsChecked.Value;
        }

        private void RepliesCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            TWITTER_INCLUDE_REPLIES = CheckBox_Replies.IsChecked.Value;
        }

        private void TwitterImageGrabber()
        {
            if (token != null)
            {
                if (token.IsCancellationRequested)
                {
                    if (!ALREADY_TERMINATE)
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "TASK TERMINATED";
                        }));
                    }
                }
                else
                {
                    PROCESSED_IMAGES_LOCAL = 0;
                    var tweets = new List<Tweetinvi.Core.Interfaces.ITweet>();
                    var next = true;
                    if (TWITTER_GRAB_MODE == 1)
                    {
                        var sp = Search.GenerateSearchTweetParameter("from:" + TWITTER_USERNAME + " filter:images");
                        sp.MaximumNumberOfResults = TWITTER_MAX_RESULT;
                        try
                        {
                            if (TWITTER_TYPE_ORDER == 0)
                            {
                                if (Convert.ToInt64(TWITTER_SINCE_ID) > 0)
                                {
                                    sp.SinceId = Convert.ToInt64(TWITTER_SINCE_ID);
                                }
                            }
                            else
                            {
                                if (Convert.ToInt64(TWITTER_MAX_ID) > 0)
                                {
                                    sp.MaxId = Convert.ToInt64(TWITTER_MAX_ID);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            next = false;
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Status_Bar_Text.Text = "ERROR #4 : " + e.Message;
                            }));
                        }
                        try
                        {
                            tweets = Search.SearchTweets(sp);
                        }
                        catch (Exception e)
                        {
                            next = false;
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Status_Bar_Text.Text = "ERROR #5: " + e.Message;
                            }));
                        }
                    }
                    else
                    {
                        try
                        {
                            var tp = Timeline.CreateUserTimelineRequestParameter(TWITTER_USERNAME);
                            tp.IncludeRTS = TWITTER_INCLUDE_RTS ? true : false;
                            tp.ExcludeReplies = !TWITTER_INCLUDE_REPLIES ? true : false;
                            tp.MaximumNumberOfTweetsToRetrieve = TWITTER_MAX_RESULT;
                            tp.TrimUser = true;
                            tp.IncludeContributorDetails = false;
                            try
                            {
                                if (TWITTER_TYPE_ORDER == 0)
                                {
                                    if (Convert.ToInt64(TWITTER_SINCE_ID) > 0)
                                    {
                                        tp.SinceId = Convert.ToInt64(TWITTER_SINCE_ID);
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt64(TWITTER_MAX_ID) > 0)
                                    {
                                        tp.MaxId = Convert.ToInt64(TWITTER_MAX_ID);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                next = false;
                                Dispatcher.Invoke((Action)(() =>
                                {
                                    Status_Bar_Text.Text = "ERROR #6 : " + e.Message;
                                }));
                            }
                            var tweetz = Timeline.GetUserTimeline(tp);
                            if (tweetz.Count() > 0)
                            {
                                foreach (Tweetinvi.Core.Interfaces.ITweet tweet in tweetz)
                                {
                                    tweets.Add(tweet);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            next = false;
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Status_Bar_Text.Text = "ERROR #7: " + e.Message;
                            }));
                        }
                    }
                    if (next)
                    {
                        TwitterGrabbingProcess(tweets);
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            //ProgressIndicator.IsBusy = false;
                            Enable_Control(true);
                            Button_Grab.Visibility = Visibility.Visible;
                            Button_Terminate.Visibility = Visibility.Hidden;
                            Status_Bar_Text.Text = "NO NEXT";
                        }));
                    }
                }
            }
        }

        private void ProcessTweet(Tweetinvi.Core.Interfaces.ITweet tweet)
        {
            PROCESSED_TWEET++;
            if (TWITTER_GRAB_MODE == 1)
            {
                if (TWITTER_TYPE_ORDER == 0)
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        TWITTER_SINCE_ID = tweet.IdStr;
                        Dispatcher.Invoke((Action)(() =>
                        {
                            TextBox_Since.Text = TWITTER_SINCE_ID;
                        }));
                        FIRST_ATTEMPT_LOOP = false;
                    }
                }
                else
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        FIRST_ATTEMPT_LOOP = false;
                    }
                }
                ProcessImagesTweet(tweet.Entities.Medias, tweet);
            }
            else
            {
                if (TWITTER_TYPE_ORDER == 0)
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        TWITTER_SINCE_ID = tweet.IdStr;
                        Dispatcher.Invoke((Action)(() =>
                        {
                            TextBox_Since.Text = TWITTER_SINCE_ID;
                        }));
                        FIRST_ATTEMPT_LOOP = false;
                    }
                }
                else
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        FIRST_ATTEMPT_LOOP = false;
                    }
                }
                if (!(tweet.Entities.Medias == null))
                {
                    if (token != null)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            ProcessImagesTweet(tweet.Entities.Medias, tweet);
                        }
                        else
                        {
                            if (!ALREADY_TERMINATE)
                            {
                                Dispatcher.Invoke((Action)(() =>
                                {
                                    Status_Bar_Text.Text = "TASK TERMINATED";
                                }));
                            }
                        }
                    }
                }
            }
        }

        private void ProcessImageTweet(Tweetinvi.Core.Interfaces.Models.Entities.IMediaEntity media, Tweetinvi.Core.Interfaces.ITweet tweet)
        {
            string url = media.MediaURLHttps;
            string filename = System.IO.Path.GetFileName(url);
            string filepath = TWITTER_FOLDER_NAME + tweet.IdStr + "_" + media.IdStr + "_" + filename;
            try
            {
                if (!File.Exists(filepath))
                {
                    String text = String.Format("#{3} | TID : {5} | IID : {4} | ▼ : {2} ... ", media.MediaType, url, filename, PROCESSED_TWEET.ToString("D4"), media.IdStr, tweet.IdStr);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        Command_Bar.Inlines.Add(Environment.NewLine + text);
                    }));
                    using (WebClient WC = new WebClient())
                    {
                        WC.DownloadFile(url, @filepath);
                    }
                    PROCESSED_IMAGES_LOCAL++;
                    PROCESSED_IMAGES++;
                    TRY_DOWNLOAD_ATTEMPT = 0;
                    text = String.Format("✔ #{0}", PROCESSED_IMAGES.ToString("D4"));
                    Dispatcher.Invoke((Action)(() =>
                    {
                        Command_Bar.Inlines.Add(text);
                        Scroll_Viewer.ScrollToBottom();
                        if (TWITTER_TYPE_ORDER == 1)
                            TextBox_Max.Text = tweet.IdStr;
                    }));
                    var jpeg = new JPEGMetaDataAdapter(filepath);
                    jpeg.Metadata.Comment = tweet.Text;
                    jpeg.Save();
                }
            }
            catch (Exception e)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Status_Bar_Text.Text = "ERROR #8 : " + e.Message;
                }));
                if (TRY_DOWNLOAD_ATTEMPT <= 3)
                {
                }
                else
                {
                    TRY_DOWNLOAD_ATTEMPT = 0;
                }
            }
        }

        private void ProcessImagesTweet(List<Tweetinvi.Core.Interfaces.Models.Entities.IMediaEntity> medias, Tweetinvi.Core.Interfaces.ITweet tweet)
        {
            try
            {
                if (token != null)
                {
                    if (token.IsCancellationRequested)
                    {
                        if (!ALREADY_TERMINATE)
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Status_Bar_Text.Text = "TASK TERMINATED";
                            }));
                        }
                    }
                    else
                    {
                        if (medias.Count() > 0)
                        {
                            medias.ForEach(m => ProcessImageTweet(m, tweet));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Status_Bar_Text.Text = "ERROR #9 : " + e.Message;
                }));
            }
        }

        private void TwitterGrabbingProcess(List<Tweetinvi.Core.Interfaces.ITweet> tweets)
        {
            if (tweets.Count() > 0)
            {
                if (token != null)
                {
                    if (!token.IsCancellationRequested)
                    {
                        tweets.ForEach(t => ProcessTweet(t));
                        TWITTER_MAX_ID = tweets.Last().IdStr;
                        Dispatcher.Invoke((Action)(() =>
                        {
                            TextBox_Max.Text = TWITTER_MAX_ID;
                        }));
                        TwitterImageGrabber();
                    }
                    else
                    {
                        if (!ALREADY_TERMINATE)
                        {
                            Dispatcher.Invoke((Action)(() =>
                            {
                                Status_Bar_Text.Text = "TASK TERMINATED";
                            }));
                        }
                    }
                }
            }
            else
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    //ProgressIndicator.IsBusy = false;
                    Enable_Control(true);
                    Button_Grab.Visibility = Visibility.Visible;
                    Button_Terminate.Visibility = Visibility.Hidden;
                }));
                if (TWITTER_TYPE_ORDER == 1)
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "NO NEW TWEETS!";
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "NO MORE TWEETS!";
                        }));
                    }
                }
                else
                {
                    if (FIRST_ATTEMPT_LOOP)
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "NO TWEETS!";
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "NO MORE TWEETS!";
                        }));
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (item.Header.ToString().Equals("_Keluar"))
            {
                Environment.Exit(0);
            }
            else if (item.Header.ToString().Equals("_Sign Out"))
            {
                Properties.Settings.Default.IS_LOGIN = false;
                Properties.Settings.Default.TWITTER_ACCESS_TOKEN = null;
                Properties.Settings.Default.TWITTER_ACCESS_TOKEN_SECRET = null;
                Properties.Settings.Default.Save();
                __sv.IS_LOGIN = false;
                __sv.TWITTER_ACCESS = null;
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            else if (item.Header.ToString().Equals("_Tentang"))
            {
                Tentang t = new Tentang();
                t.ShowDialog();
            }
            else if (item.Header.ToString().Equals("_Limit Status"))
            {
                LimitStatus ls = new LimitStatus();
                ls.ShowDialog();
            }
        }
    }
}
