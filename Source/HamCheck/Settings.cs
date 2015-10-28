using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

namespace HamCheck {

    internal static class Settings {

        public static bool NoRegistryWrites {
            get {
                try {
                    using (var key = Registry.CurrentUser.OpenSubKey(Medo.Configuration.Settings.SubkeyPath)) {
                        return (key == null);
                    }
                } catch (SecurityException) {
                    return true;
                }
            }
            set {
                try {
                    if (value) { //remove subkey
                        try {
                            Registry.CurrentUser.DeleteSubKeyTree(Medo.Configuration.Settings.SubkeyPath);
                        } catch (ArgumentException) { }
                    } else {
                        Registry.CurrentUser.CreateSubKey(Medo.Configuration.Settings.SubkeyPath);
                    }
                    Medo.Configuration.Settings.NoRegistryWrites = value;
                    Medo.Windows.Forms.State.NoRegistryWrites = value;
                    Medo.Diagnostics.ErrorReport.DisableAutomaticSaveToTemp = value;
                } catch (IOException) {
                } catch (SecurityException) {
                } catch (UnauthorizedAccessException) { }
            }
        }

        public static bool DebugShowHitBoxes {
            get { return Medo.Configuration.Settings.Read("DebugShowHitBoxes", false); }
        }

    }
}
