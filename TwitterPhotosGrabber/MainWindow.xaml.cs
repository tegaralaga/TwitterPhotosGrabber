using System;
using System.Windows;
using TwitterPhotosGrabber.Tools;

namespace TwitterPhotosGrabber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StaticValue __sv = StaticValue.Instance;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            if (__sv.IS_LOGIN)
            {
                this.Content = new Dashboard();
            }
            else
            {
                this.Content = new TwitterLogin();
            }
        }
    }
}
