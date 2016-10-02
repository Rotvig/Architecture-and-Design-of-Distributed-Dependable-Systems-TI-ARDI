using System.Windows;
using System.Windows.Controls;
using Shared;

namespace Player
{
    public partial class MainWindow : Window
    {
        private readonly Subscriber subscriber;

        public MainWindow()
        {
            InitializeComponent();


            subscriber = new Subscriber();

            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => message.Text = @event.Message);
        }

        private void btn_sub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Subscribe(topic.Text.Trim());

            ((Button) sender).Visibility = Visibility.Collapsed;
            btn_unsub.Visibility = Visibility.Visible;
        }

        private void btn_unsub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Unsubscribe();
            ((Button) sender).Visibility = Visibility.Collapsed;
            btn_sub.Visibility = Visibility.Visible;
        }
    }
}
