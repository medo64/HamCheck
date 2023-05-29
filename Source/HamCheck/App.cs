using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace HamCheck {
    internal static class App {

        [STAThread]
        static void Main() {
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
            using (var setupMutex = new Mutex(false, @"Global\JosipMedved_HamCheck", out var createdNew, mutexSecurity)) {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Medo.Configuration.Settings.Read("CultureName", "en-US"));

                Medo.Application.UnhandledCatch.ThreadException += new EventHandler<ThreadExceptionEventArgs>(UnhandledCatch_ThreadException);
                Medo.Application.UnhandledCatch.Attach();

                Medo.Configuration.Settings.NoRegistryWrites = Settings.NoRegistryWrites;
                Medo.Windows.Forms.State.NoRegistryWrites = Settings.NoRegistryWrites;
                Medo.Diagnostics.ErrorReport.DisableAutomaticSaveToTemp = Settings.NoRegistryWrites;

                if (!((Environment.OSVersion.Version.Build < 7000) || (App.IsRunningOnMono))) {
                    var appId = Assembly.GetExecutingAssembly().Location;
                    if (appId.Length > 127) { appId = @"JosipMedved_HamCheck\" + appId.Substring(appId.Length - 127 - 20); }
                    NativeMethods.SetCurrentProcessExplicitAppUserModelID(appId);
                }

                var mainForm = new MainForm();

                //single instance
                Medo.Application.SingleInstance.NewInstanceDetected += delegate (object sender, Medo.Application.NewInstanceEventArgs e) {
                    mainForm.Show();
                    mainForm.Activate();
                };
                if (Medo.Application.SingleInstance.IsOtherInstanceRunning) {
                    var currProcess = Process.GetCurrentProcess();
                    foreach (var iProcess in Process.GetProcessesByName(currProcess.ProcessName)) {
                        try {
                            if (iProcess.Id != currProcess.Id) {
                                NativeMethods.AllowSetForegroundWindow(iProcess.Id);
                                break;
                            }
                        } catch (Win32Exception) { }
                    }
                }
                Medo.Application.SingleInstance.Attach();

                Application.Run(mainForm);
            }
        }



        private static void UnhandledCatch_ThreadException(object sender, ThreadExceptionEventArgs e) {
#if !DEBUG
            Medo.Diagnostics.ErrorReport.ShowDialog(null, e.Exception, new Uri("https://medo64.com/feedback/"));
#else
            throw e.Exception;
#endif
        }


        private static class NativeMethods {

            [DllImportAttribute("user32.dll", EntryPoint = "AllowSetForegroundWindow")]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            public static extern bool AllowSetForegroundWindow(int dwProcessId);

            [DllImport("Shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern UInt32 SetCurrentProcessExplicitAppUserModelID(String AppID);

        }

        private static bool IsRunningOnMono {
            get {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }

    }
}
