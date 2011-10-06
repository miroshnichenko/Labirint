using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Blasig.Labirint.GUI
{
    public enum MessageWindowImage
    { Error, Warning, Info, Exit, Printing }

    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public MessageWindow(string text, MessageWindowImage messageImage,
            bool hideCancel)
            : this()
        {
            this.MessageImage(messageImage);
            this.Text.Text = text;
            if (hideCancel)
                Canc.Visibility = Visibility.Collapsed;
        }

        private void MessageImage(MessageWindowImage messageImage)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            switch (messageImage)
            { 
                case MessageWindowImage.Error:
                    image.UriSource = new Uri("pack://application:,,,/Resources;component/Resources/Error.png");
                    break;
                case MessageWindowImage.Warning:
                    image.UriSource = new Uri("pack://application:,,,/Resources;component/Resources/Warning.png");
                    break;
                case MessageWindowImage.Info:
                    image.UriSource = new Uri("pack://application:,,,/Resources;component/Resources/Info.png");
                    break;
                case MessageWindowImage.Exit:
                    image.UriSource = new Uri("pack://application:,,,/Resources;component/Resources/Exit.png");
                    break;
                case MessageWindowImage.Printing:
                    image.UriSource = new Uri("pack://application:,,,/Resources;component/Resources/Print.png");
                    break;
            }
            image.EndInit();
            Img.Source = image;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
