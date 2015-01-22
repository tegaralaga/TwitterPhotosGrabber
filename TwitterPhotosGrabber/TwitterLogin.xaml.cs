using System;
using System.Windows.Controls;
using TwitterPhotosGrabber.Tools;
using TwitterPhotosGrabber.Model;
using Tweetinvi;
using System.Threading.Tasks;
using System.Windows;
using TwitterPhotosGrabber.Model;

namespace TwitterPhotosGrabber
{
    /// <summary>
    /// Interaction logic for TwitterLogin.xaml
    /// </summary>
    public partial class TwitterLogin : UserControl
    {

        private Tweetinvi.Core.Interfaces.Credentials.ITemporaryCredentials __credential;
        private StaticValue __sv = StaticValue.Instance;

        public TwitterLogin()
        {
            InitializeComponent();
            int count = __sv.TWITTER_CONSUMER_LIST.Count;
            for (int i = 0; i < count; i++)
            {
                TwitterConsumer tc = __sv.TWITTER_CONSUMER_LIST[i];
                ComboBox_Select_Application.Items.Add(tc.NAME);
            }
            ComboBox_Select_Application.SelectedIndex = __sv.SELECTED_APPLICATION;
        }

        private void ComboBox_Select_Application_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            __sv.SELECTED_APPLICATION = ComboBox_Select_Application.SelectedIndex;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs rea)
        {
            Button btn = (Button)sender;
            if (btn.Name.Equals("RUN"))
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            RUN.IsEnabled = false;
                            ComboBox_Select_Application.IsEnabled = false;
                            Status_Bar_Text.Text = "Building Credential...";
                        }));
                        __credential = CredentialsCreator.GenerateApplicationCredentials(__sv.TWITTER_CONSUMER.KEY, __sv.TWITTER_CONSUMER.SECRET);
                        var url = CredentialsCreator.GetAuthorizationURLForCallback(__credential, "oob");
                        Dispatcher.Invoke((Action)(() =>
                        {
                            Status_Bar_Text.Text = "Loading Page : " + url + " ...";
                            BROWSER.Navigate(url);
                        }));
                    }
                    catch (Exception e)
                    {
                        Utils.WriteLine("ERROR : " + e.Message);
                    }
                }).ContinueWith((task) =>
                { }, TaskScheduler.FromCurrentSynchronizationContext()
                );
            }
            else
            {
                if (TextBoxPin.Text.Trim().Length > 0)
                {
                    try
                    {
                        var result = CredentialsCreator.GetCredentialsFromVerifierCode(TextBoxPin.Text, __credential);
                        if (result.AccessToken == null || result.AccessTokenSecret == null)
                        {
                            MessageBox.Show("SOMETHING WENT WRONG, PLEASE TRY AGAIN...", "ERROR");
                        }
                        else
                        {
                            TwitterAccess ta = new TwitterAccess(result.AccessToken, result.AccessTokenSecret);
                            __sv.TWITTER_ACCESS = ta;
                            __sv.IS_LOGIN = true;
                            this.Content = new Dashboard();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "ERROR");
                    }
                }
                else
                {
                    MessageBox.Show("PIN HARUS DIISI", "YOU MORON!");
                    TextBoxPin.Focus();
                }
            }
        }

        private void BROWSER_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RUN.IsEnabled = true;
                ComboBox_Select_Application.IsEnabled = true;
                Status_Bar_Text.Text = "Page Loaded";
            }));
        }

    }

}
