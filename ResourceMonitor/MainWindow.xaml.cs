using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ResourceMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        private string _instanceName = null;

        public MainWindow()
        {
            InitializeComponent();
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick +=Timer_Tick;
            timer.Start();
            TextBlockUser.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick +=_timer_Tick;
            _timer.Start();
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();

            DataGridProcesses.ItemsSource = processes.Select(x => new SelectedProcess()
            {
                ProcessName = x.ProcessName,
                Id = x.Id,
            });
        }


        private void _timer_Tick(object? sender, EventArgs e)
        {
            if(_instanceName == null) return;

            var cpu = new PerformanceCounter("Process", "% Processor Time", _instanceName, true);
            var ram = new PerformanceCounter("Process", "Private Bytes", _instanceName, true);

            // If system has multiple cores, that should be taken into account
            TextBlockCPU.Text = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2).ToString();
            // Returns number of MB consumed by application
            TextBlockMemory.Text = Math.Round(ram.NextValue() / 1024 / 1024, 2).ToString();

        }

        private void DataGridProcesses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridProcesses.SelectedItem == null) return;

            var _process = DataGridProcesses.SelectedItem as SelectedProcess;

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");

            string[] instances = cat.GetInstanceNames();
            foreach (string instance in instances)
            {

                using (PerformanceCounter cnt = new PerformanceCounter("Process",
                           "ID Process", instance, true))
                {
                    int val = (int)cnt.RawValue;
                    if (val == _process.Id)
                    {
                        _instanceName = instance;
                        return;
                    }
                }
            }
            throw new Exception("Could not find performance counter " +
                                "instance name for current process. This is truly strange ...");
        }
    }
}
