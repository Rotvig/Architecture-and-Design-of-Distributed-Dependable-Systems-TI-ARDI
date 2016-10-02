using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Shared;

namespace Player
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly Socket _client;
        //private readonly EndPoint _remoteEndPoint;
        //private byte[] _data;
        //private int _recv;
        //private bool _isReceivingStarted;

        //private const int Port = 10001;

        private Subscriber Subscriber;

        public MainWindow()
        {
            InitializeComponent();


            Subscriber = new Subscriber();

            Subscriber.NewMessage += (sender, @event) => message.Text = @event.Message;

            //_client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //_remoteEndPoint = new IPEndPoint(Utils.GetLocalIp4Address(), Port);

        }

        //private void ReceiveDataFromServer()
        //{
        //    var publisherEndPoint = _client.LocalEndPoint;
        //    while (true)
        //    {
        //        _recv = _client.ReceiveFrom(_data, ref publisherEndPoint);
        //        var msg = Encoding.ASCII.GetString(_data, 0, _recv) + "," + publisherEndPoint;
        //        message.Text = msg;
        //    }
        //}

        /// <summary>
        ///     Subscribe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_sub_Click(object sender, RoutedEventArgs e)
        {
            Subscriber.Subscribe(topic.Text.Trim());
            //var topicName = topic.Text.Trim();
            //if (string.IsNullOrEmpty(topicName))
            //{
            //    MessageBox.Show("Please Enter a Topic Name");
            //    return;
            //}
            ((Button)sender).Visibility = Visibility.Collapsed;
            btn_unsub.Visibility = Visibility.Visible;

            //var Command = "Subscribe";

            //var message = Command + "," + topicName;
            //_client.SendTo(Encoding.ASCII.GetBytes(message), _remoteEndPoint);

            //if (_isReceivingStarted == false)
            //{
            //    _isReceivingStarted = true;
            //    _data = new byte[1024];
            //    var thread1 = new Thread(ReceiveDataFromServer);
            //    thread1.IsBackground = true;
            //    thread1.Start();
            //}
        }

        /// <summary>
        ///     UnSubscribe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_unsub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Subscriber.Unsubscribe();
                //var topicName = topic.Text.Trim();
                //if (string.IsNullOrEmpty(topicName))
                //{
                //    MessageBox.Show("Please Enter a Topic Name");
                //    return;
                //}
                //var command = "UnSubscribe";

                //var message = command + "," + topicName;
                //_client.SendTo(Encoding.ASCII.GetBytes(message), _remoteEndPoint);
                ((Button)sender).Visibility = Visibility.Collapsed;
                btn_sub.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }
    }
}
