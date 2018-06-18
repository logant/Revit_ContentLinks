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

namespace Elk.UpdateableLinks
{
    /// <summary>
    /// Interaction logic for LinkEditForm.xaml
    /// </summary>
    public partial class LinkEditForm : Window
    {
        public CommandInfo Command { get; set; }

        public LinkEditForm(CommandInfo cmd)
        {
            Command = cmd;
            InitializeComponent();

            linkNameTextBox.Text = Command.CommandName;
            addressTextBox.Text = Command.Address;
            iconTextBox.Text = Command.Icon;
            tooltipTextBox.Text = Command.Tooltip;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Command = null;
            Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Command.CommandName = linkNameTextBox.Text;
            Command.Address = addressTextBox.Text;
            Command.Icon = iconTextBox.Text;
            Command.Tooltip = tooltipTextBox.Text;
            Close();
        }

    }
}
