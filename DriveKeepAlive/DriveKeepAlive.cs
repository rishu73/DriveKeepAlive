using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.IO;

using System.Timers;

namespace DriveKeepAlive
{
    public partial class DriveKeepAlive : ServiceBase
    {
        private Timer timer1 = new Timer();

        Properties.Settings settings = Properties.Settings.Default;
        
        public DriveKeepAlive()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("DriveKeepAlive"))
            {
                System.Diagnostics.EventLog.CreateEventSource("DriveKeepAlive", "Application");
            }
            eventLog1.Source = "DriveKeepAlive";
            eventLog1.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry(String.Format("Started - Writing to {0}:\\ every {1} seconds",
                settings.DriveLetter, settings.TimeIntervalSeconds));

            settings.Reload();

            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            timer1.Interval = settings.TimeIntervalSeconds * 1000;
            timer1.Start();
        }

        void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            string dirPath = String.Format("{0}:\\{1}",
                settings.DriveLetter, Guid.NewGuid().ToString());

            try
            {
                if (settings.VerboseLevel > 0)
                {
                    eventLog1.WriteEntry(String.Format("Writing to {0}", dirPath));
                }

                Directory.CreateDirectory(dirPath);
                Directory.Delete(dirPath);
            }
            catch (Exception ex)
            {
                logException(dirPath, ex);
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            eventLog1.WriteEntry("Stopped");
        }

        private void logException(string dirPath, Exception ex)
        {
            string msg =
                String.Format(
                "Exception: {0} with dirPath = '{1}'\r\n" +
                "Message: {2}\r\n" +
                "Stack Trace:\r\n" +
                "{3}",
                ex.GetType().Name, dirPath, ex.Message, ex.StackTrace).Replace("\r", "").Replace("\n", "\r\n").Replace("\t", "     ");

            eventLog1.WriteEntry(msg, System.Diagnostics.EventLogEntryType.Error);
        }
    }
}
