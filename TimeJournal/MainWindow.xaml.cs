using System;
using System.Configuration;
using System.Windows.Automation;
using System.Management;
using System.IO;
using System.Globalization;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace TimeJournal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer showTimer = new DispatcherTimer();
        DispatcherTimer dismissTimer = new DispatcherTimer();
        Random generator = new Random();

        private ObservableCollection<string> activities = new ObservableCollection<string>();
        private List<string> allActivities = new List<string>();
        bool initialized = false;

        string lastWindowTitle = String.Empty;

        public MainWindow()
        {
            InitializeComponent();

            PopulateActivities();

            showTimer.Interval = TimeSpan.FromMinutes(GetShowInterval());
            showTimer.Tick += new EventHandler(showTimer_Tick);

            showTimer.Start();

            dismissTimer.Interval = TimeSpan.FromMinutes(GetDismissInterval());
            dismissTimer.Tick += new EventHandler(dismissTimer_Tick);

            Activity.KeyUp += new KeyEventHandler(Activity_KeyUp);
            Activity.KeyDown += new KeyEventHandler(Activity_KeyDown);
            SelectedItems.KeyUp += new KeyEventHandler(SelectedItems_KeyDown);
            SelectedItems.SelectionChanged += new SelectionChangedEventHandler(SelectedItems_SelectionChanged);
            SelectedItems.MouseDoubleClick += new MouseButtonEventHandler(SelectedItems_DoubleTapped);

            this.StateChanged += new EventHandler(MainWindow_StateChanged);
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Activated +=new EventHandler(MainWindow_Activated);
        }

        private double GetShowInterval()
        {
            string showIntervalMinSetting = ConfigurationManager.AppSettings["showIntervalMin"];
            if (String.IsNullOrEmpty(showIntervalMinSetting)) { showIntervalMinSetting = "5"; }

            string showIntervalMaxSetting = ConfigurationManager.AppSettings["showIntervalMax"];
            if (String.IsNullOrEmpty(showIntervalMaxSetting)) { showIntervalMaxSetting = "25"; }

            double showIntervalMin = Convert.ToDouble(showIntervalMinSetting);
            double showIntervalMax = Convert.ToDouble(showIntervalMaxSetting);

            return showIntervalMin + (generator.NextDouble() * (showIntervalMax - showIntervalMin));
        }

        private double GetDismissInterval()
        {
            string dismissIntervalSetting = ConfigurationManager.AppSettings["dismissInterval"];
            if (String.IsNullOrEmpty(dismissIntervalSetting)) { dismissIntervalSetting = "4"; }

            return Convert.ToDouble(dismissIntervalSetting);
        }

        private void PopulateActivities()
        {
            string[] outputFiles = new string[] { GetLogFileName(0), GetLogFileName(-1) };
            foreach (string outputFile in outputFiles)
            {
                if (File.Exists(outputFile))
                {
                    foreach (PSObject result in PowerShell.Create().AddCommand("Import-Csv").AddParameter("Path", outputFile).Invoke())
                    {
                        string activityValue = result.Properties["Activity"].Value.ToString();

                        if (!allActivities.Contains(activityValue))
                        {
                            activities.Add(activityValue);
                            allActivities.Add(activityValue);
                        }
                    }
                }
            }
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (!initialized)
            {
                this.WindowState = System.Windows.WindowState.Minimized;
                this.Hide();

                initialized = true;
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedItems.DataContext = activities;
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Activity.Focus();
        }

        void SelectedItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveItem(SelectedItems.SelectedItem.ToString());
            }
        }

        void SaveItem(string text)
        {
            dismissTimer.Stop();

            string date = EscapeForCsv(DateTime.Now.ToString());
            string windowTitle = EscapeForCsv(lastWindowTitle);
            string activity = EscapeForCsv(text);
            string csvLine = String.Format("{0},{1},{2}\r\n", date, windowTitle, activity);

            string outputFile = GetLogFileName(0);
            if (!File.Exists(outputFile))
            {
                string directoryName = Path.GetDirectoryName(outputFile);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllLines(outputFile, new string[] { "Date,WindowTitle,Activity" });
            }

            File.AppendAllText(outputFile, csvLine);

            if (allActivities.Contains(text))
            {
                allActivities.Remove(text);
            }
            allActivities.Insert(0, text);
            Activity.Text = String.Empty;
            RefreshItems(Activity.Text);
            this.WindowState = System.Windows.WindowState.Minimized;
            this.Hide();
        }

        string EscapeForCsv(string input)
        {
            bool wrappedInQuotes = false;

            if (input.IndexOf('"') >= 0)
            {
                input = '"' + input.Replace("\"", "\"\"") + '"';
                wrappedInQuotes = true;
            }

            if ((input.IndexOf(',') >= 0) && (! wrappedInQuotes))
            {
                input = '"' + input + '"';
            }

            return input;
        }

        void SelectedItems_DoubleTapped(object sender, MouseButtonEventArgs e)
        {
            SaveItem(SelectedItems.SelectedItem.ToString());
        }

        void SelectedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems.SelectedItem != null)
            {
                Activity.Text = SelectedItems.SelectedItem.ToString();
            }
        }

        void Activity_KeyDown(object sender, KeyEventArgs e)
        {
            string compareText = Activity.Text.Trim();

            if (e.Key == Key.Enter)
            {
                if (!String.IsNullOrEmpty(compareText))
                {
                    if (SelectedItems.Items.Count != 0)
                    {
                        compareText = SelectedItems.Items[0].ToString();
                    }

                    SaveItem(compareText);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                SelectedItems.Focus();
                SelectedItems.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                SelectedItems.Focus();
                SelectedItems.SelectedIndex = SelectedItems.Items.Count - 1;
                e.Handled = true;
            }
        }

        void Activity_KeyUp(object sender, KeyEventArgs e)
        {
            dismissTimer.Stop();

            string compareText = Activity.Text.Trim();
            RefreshItems(compareText);

            Activity.Focus();
        }

        private void RefreshItems(string compareText)
        {
            activities.Clear();
            foreach (string activity in allActivities)
            {
                if (activity.IndexOf(compareText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    activities.Add(activity);
                }
            }

            SelectedItems.DataContext = null;
            SelectedItems.DataContext = activities;
        }

        void showTimer_Tick(object sender, EventArgs e)
        {
            int currentProcess = AutomationElement.FocusedElement.Current.ProcessId;
            lastWindowTitle = String.Empty;

            try
            {
                do
                {
                    lastWindowTitle = System.Diagnostics.Process.GetProcessById(currentProcess).MainWindowTitle;
                    if (!String.IsNullOrEmpty(lastWindowTitle)) { break; }

                    currentProcess = GetParentProcess(currentProcess);
                } while (currentProcess >= 0);
            }
            catch (ArgumentException) { }

            this.Show();
            NativeMethods.Win32Utils.Flash(this);

            if(SelectedItems.Items.Count > 0)
            {
                SelectedItems.SelectedIndex = 0;
            }

            Activity.Focus();
            Activity.SelectAll();
            dismissTimer.Start();
        }

        void dismissTimer_Tick(object sender, EventArgs e)
        {
            string currentActivity = GetCurrentAppointment();
            if (!String.IsNullOrEmpty(currentActivity))
            {
                Activity.Text = currentActivity;
                SaveItem(Activity.Text);
            }
            else
            {
                currentActivity = "N/A";
            }

            Activity.Text = currentActivity;
        }

        string GetCurrentAppointment()
        {
            string scriptText = @"
            ## Look at Outlook to determine your current meeting, if you're in one.
            ## This is used when you're away from your desk and the dismiss timer
            ## auto-dismisses the dialog.
            try
            {
                $Date = Get-Date
        
                ## Access Outlook via COM, and open the Calendar folder
                $olApp = New-Object -com Outlook.Application
                $namespace = $olApp.GetNamespace(""MAPI"")
                $fldCalendar = $namespace.GetDefaultFolder(9)

                $dateString = '{0:g}' -f $Date
                $items = $fldCalendar.Items.Restrict(""([Start] < '$dateString') AND ([End] > '$dateString')"")
                $items.IncludeRecurrences = $true

                ## Sort by start time, descending, so that we can access
                ## the most recent items first.
                $items.Sort(""Start"")
                $items = @($items)

                ## Look through all the items
                for($index = $items.Count - 1; $index -ge 0; $index--)
                {
                    $item = $items[$index]

                    ## Skip items that ended in the past
                    if($item.End -lt $Date)
                    {
                        return
                    }

                    ## If the current appointment spans the current time,
                    ## AND is marked as ""Busy"" (as opposed to ""Free,"")
                    ## then return that as a result.
                    if(($item.Start -le $Date) -and
                        ($item.End -ge $Date) -and
                        ($item.BusyStatus -eq 2))
                    {
                        ""Meeting: "" + $item.Subject
                        break
                    } 
                }
            }
            catch
            {
                ## Catch the error when the user doesn't have Outlook
                ## installed.
                $error.RemoveAt(0)
            }
            finally
            {
                $olApp = $null
            }
            ";

            StringBuilder results = new StringBuilder();
            foreach (string output in PowerShell.Create().AddScript(scriptText).Invoke<string>())
            {
                results.Append(output);
            }

            return results.ToString();
        }

        string GetLogFileName(int weekOffset)
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            DateTime weekStart = DateTime.Now.AddDays(7 * weekOffset);

            while (weekStart.DayOfWeek != dateTimeFormat.FirstDayOfWeek)
            {
                weekStart = weekStart.AddDays(-1);
            }

            string filename = weekStart.ToString("yyyyMMdd") + ".csv";
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string result = Path.Combine(myDocs, Path.Combine("TimeJournal", filename));

            return result;
        }

        int GetParentProcess(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(String.Format("SELECT * FROM Win32_Process WHERE ProcessId={0}", pid));
            ManagementObjectCollection resultCollection = searcher.Get();
            foreach(ManagementObject result in resultCollection)
            {
                return Convert.ToInt32(result["ParentProcessId"]);
            }

            return -1;
        }
    }
}
