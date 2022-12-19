using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;

namespace technoviumunlimitedstart
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StartupEventArgs startupEventArgs;

        private string rootPath;
        private string gameZip;
        private string gameExe;
        private string versionFile;

        private string UriScheme = "TechnoviumUnlimited";
        private string FriendlyName = "Technovim Unlimited";

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Content = "Play";
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = "Downloading Game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = "Downloading Update";
                        break;
                    default:
                        break;
                }
            }
        }
        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();
            SetRegister();
            startupEventArgs = e;
            getArguments();

            CheckForUpdates();
        }

        public void getArguments()
        {
            for (int i = 0; i != startupEventArgs.Args.Length; ++i)
            {
                ArgsButton.Content = startupEventArgs.Args[i];
                Debug.WriteLine(startupEventArgs.Args[i]);
                IList<string> args = startupEventArgs.Args[i].Split('/').Reverse().ToList<string>();
                foreach (String s in args)
                {
                    //write txt file to 
                    Debug.WriteLine(s);
                }

            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //DeleteRegiter();
        }

        private void DeleteRegiter()
        {
            //Registry.ClassesRoot.DeleteSubKey(UriScheme);
            //SetRegister();
        }

        private void SetRegister()
        {
            rootPath = Environment.ExpandEnvironmentVariables("%AppData%\\TechnoviumUnlimited\\game");//Directory.GetCurrentDirectory();
            //var key = Registry.ClassesRoot.CreateSubKey(UriScheme);
            //Registry.ClassesRoot

            // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
            // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
            //string applicationLocation = rootPath + "\\Starter\\TechnoviumUnlimitedLauncher.exe";
            //key.SetValue("URL Protocol", "/f");
            //var command = key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
            //command.SetValue("", applicationLocation);

            //key.SetValue("", "URL:" + FriendlyName);
            //key.SetValue("URL Protocol", "");
            /*
            using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
            {
                defaultIcon.SetValue("", applicationLocation + ",1");
            }*/

            //var command = scheme.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
            //command.SetValue("", $"\"{Assembly.GetExecutingAssembly().Location}\" \"%1\"");

            /*using (var commandKey = key.CreateSubKey(@"shell\open\command"))
            {
                commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
            }*/


            if (!Directory.Exists(rootPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(rootPath);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(rootPath));
            }

            versionFile = System.IO.Path.Combine(rootPath, "Version.txt");
            gameZip = System.IO.Path.Combine(rootPath, "Build.zip");
            gameExe = System.IO.Path.Combine(rootPath, "Build", "Technovium unlimited.exe");
            Debug.WriteLine("gameExe: ");
            Debug.WriteLine(gameExe);
            Debug.WriteLine("gameZip: ");
            Debug.WriteLine(gameZip);
            Debug.WriteLine("versionFile: ");
            Debug.WriteLine(versionFile);
            Debug.WriteLine("rootpath: ");
            Debug.WriteLine(rootPath);

        }
        private void CheckForUpdates()
        {
            Debug.WriteLine("------------------------> CheckForUpdates <----------------------------------");
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();
                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/technoviumunlimited/technoviumunlimited_unity3d/main/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/technoviumunlimited/technoviumunlimited_unity3d/main/version.txt"));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://github.com/technoviumunlimited/technoviumunlimited_unity3d/releases/download/0.0.1/Build.zip"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }
        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }
        private void Play_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = System.IO.Path.Combine(rootPath, "Build");
                Debug.WriteLine("startInfo.ToString():");
                Debug.WriteLine(startInfo.ToString());
                Process.Start(startInfo);
                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }

        }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);
            private short major;
            private short minor;
            private short subMinor;
            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }
            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}
