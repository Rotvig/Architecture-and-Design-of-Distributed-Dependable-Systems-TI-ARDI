using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PubSubServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TextBlock.Text = "Pub/Sub Server Starting";

            var subscriberService = new SubscriberService();
            subscriberService.StartSubscriberService();

            var publisherService = new PublisherService();
            publisherService.StartPublisherService();

            TextBlock.Text = "Pub/Sub Server running";
        }
    }
}
