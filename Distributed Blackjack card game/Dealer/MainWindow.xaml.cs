using System;
using System.Windows;
using Shared;

namespace Dealer
{
    public partial class MainWindow : Window
    {
        private readonly Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            Publisher = new Publisher();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Publisher.Publish(topic.Text, "HELLOW WORLD");
        }
    }
}
