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
using Blasig.Labirint.DataModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Blasig.Labirint.GUI
{
    /// <summary>
    /// Логика взаимодействия для AddVisitorWindow.xaml
    /// </summary>
    public partial class AddVisitorWindow : Window
    {
        public AddVisitorWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            IDataErrorInfo errors = DataContext as IDataErrorInfo;
            if(errors != null)
            {
                if (errors.Error != null)
                {
                    MessageWindow wnd = new MessageWindow(errors.Error,
                        MessageWindowImage.Warning, true);
                    wnd.Owner = this;
                    wnd.ShowDialog();
                }
                else
                    this.DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(ClientNameTextBox);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = TelephoneTextBox;
            BindingExpression expr = BindingOperations.GetBindingExpression(obj, TextBox.TextProperty);
            if (expr != null)
                expr.UpdateSource();
        }
    }
}
