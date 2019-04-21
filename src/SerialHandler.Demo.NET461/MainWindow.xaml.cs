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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialHandler.Demo.NET461
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyCommandService _commandService;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void _commandService_HeartReport()
        {
            txtReporter.Dispatcher.Invoke(new Action<string>(txtReporter.AppendText), $"Terminal Report State:{DateTime.Now}");
        }

        private void OpenPort(object sender, RoutedEventArgs e)
        {
            if (_commandService == null)
            {
                _commandService = new MyCommandService(txtPortName.Text, Convert.ToInt32(txtBaudRate.Text));
                _commandService.HeartReport += _commandService_HeartReport;
            }
            if (!_commandService.IsOpen)
                _commandService.Open();
            else
                MessageBox.Show("The port is already opened.");
        }

        private void ClosePort(object sender, RoutedEventArgs e)
        {
            if (_commandService != null && _commandService.IsOpen)
                _commandService.Close();
        }

        private void CmdQueryState(object sender, RoutedEventArgs e)
        {
            if (_commandService == null || !_commandService.IsOpen)
            {
                MessageBox.Show("The port is not opened.");
                return;
            }

            bool isError;
            var reault = _commandService.QueryState(out isError);
            if (reault == SendResult.Success)
            {
                MessageBox.Show($"The terminal state is {isError}.");
                return;
            }
            else
            {
                MessageBox.Show($"Command failed:{reault}.");
            }
        }
    }
}
