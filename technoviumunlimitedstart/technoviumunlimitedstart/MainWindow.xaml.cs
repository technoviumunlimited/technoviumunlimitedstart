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
using System.Threading;

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
        private static string startMap                  = "start";
        private static string gameMap                   = "game";

        // game version variables
        private static string rootPathGame              = Environment.ExpandEnvironmentVariables("%AppData%\\TechnoviumUnlimited\\" + gameMap).ToString();
        private static string gameZipGame               = System.IO.Path.Combine(rootPathGame, "Version.txt").ToString();
        private static string gameExeGame               = System.IO.Path.Combine(rootPathGame, "Build", "Technovium unlimited.exe").ToString();
        private static string versionFileGame           = System.IO.Path.Combine(rootPathGame, "Version.txt").ToString();
        private static string onlineVersionFileGame     = "https://raw.githubusercontent.com/technoviumunlimited/technoviumunlimited_unity3d/main/version.txt";

        // technoviumunlimitedstart version variables
        private static string rootPathStart             = Environment.ExpandEnvironmentVariables("%AppData%\\TechnoviumUnlimited\\" + startMap).ToString();
        private static string gameZipStart              = System.IO.Path.Combine(rootPathStart, "Version.txt").ToString();
        private static string gameExeStart              = System.IO.Path.Combine(rootPathStart, "start", "technoviumunlimitedstart.exe").ToString();
        private static string versionFileStart          = System.IO.Path.Combine(rootPathStart, "Version.txt").ToString();
        private static string onlineVersionFileStart    = "https://api.technoviumunlimited.nl/start";

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
            if (!Directory.Exists(rootPathGame))
            {
                DirectoryInfo di = Directory.CreateDirectory(rootPathGame);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(rootPathGame));
            }

            if (!Directory.Exists(rootPathStart))
            {
                DirectoryInfo di = Directory.CreateDirectory(rootPathStart);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(rootPathStart));
            }

            startupEventArgs = e;
            getArguments();
            //check start (game launcher) update
            CheckForUpdatesStart();

            //check unity game update
            CheckForUpdates();
            
            //startGameAndCloseCurrent();
        }

        public void getArguments()
        {
            File.Delete(rootPathGame + "\\st.daliop");
            for (int i = 0; i != startupEventArgs.Args.Length; ++i)
            {
                Debug.WriteLine(startupEventArgs.Args[i]);
                IList<string> args = startupEventArgs.Args[i].Split('/').Reverse().ToList<string>();
               // Debug.WriteLine(s);
                TextWriter tw = new StreamWriter(rootPathGame + "\\st.daliop");
                foreach (String s in args)
                {
                    if(s != "technoviumunlimitedstart:")
                    {
                        tw.WriteLine(s);
                    }                    
                }
                tw.Close();
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //DeleteRegiter();
        }
        private void CheckForUpdatesStart()
        {
            Debug.WriteLine("------------------------> CheckForUpdatesStart <----------------------------------");
            Debug.WriteLine(versionFileStart);
            Debug.WriteLine(onlineVersionFileStart);

            if (File.Exists(versionFileStart))
            {
                Version localVersionStart = new Version(File.ReadAllText(versionFileStart));
                VersionText.Text = localVersionStart.ToString();
                Debug.WriteLine("------------------------> CheckForUpdates localVersionStart.ToString() <----------------------------------");
                Debug.WriteLine(localVersionStart.ToString());
               
                
                
                try
                {
                    WebClient webClientStart = new WebClient();
                    Version onlineVersionStart = new Version(webClientStart.DownloadString(onlineVersionFileStart));
                    Debug.WriteLine("------------------------> CheckForUpdates onlineVersionStart.ToString() <----------------------------------");
                    Debug.WriteLine("::::"+onlineVersionStart.ToString());
                    if (onlineVersionStart.IsDifferentThan(localVersionStart))
                    {
                        if (MessageBox.Show("Update Availible", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            //do no stuff
                        }
                        else
                        {
                            var target = "https://github.com/technoviumunlimited/TechnoviumUnlimitedInstaller/releases/download/0.0.1/TechnoviumUnlimitedInstaller.exe";
                            try
                            {
                                
                                System.Diagnostics.Process.Start("explorer", target);
                                //TODO download installer and install it without browser
                            }
                            catch (System.ComponentModel.Win32Exception noBrowser)
                            {
                                if (noBrowser.ErrorCode == -2147467259)
                                    MessageBox.Show(noBrowser.Message);
                            }
                            catch (System.Exception other)
                            {
                                MessageBox.Show(other.Message);
                            }
                            Close(); //TODO try to close without errors
                            //do yes stuff
                        }
                    }
                    else
                    {
                        //Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    //Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                //InstallGameFiles(false, Version.zero);
                // TODO messagebox to download installer
            }
        }
        private void CheckForUpdates()
        {
            Debug.WriteLine("------------------------> CheckForUpdates <----------------------------------");
            Debug.WriteLine(versionFileGame);
            Debug.WriteLine(onlineVersionFileGame);

            if (File.Exists(versionFileGame)) {
                Version localVersionGame = new Version(File.ReadAllText(versionFileGame));
                VersionText.Text = localVersionGame.ToString();
                try {
                    WebClient webClientGame = new WebClient();
                    Version onlineVersionGame = new Version(webClientGame.DownloadString(onlineVersionFileGame));

                    if (onlineVersionGame.IsDifferentThan(localVersionGame)) {
                        InstallGameFiles(true, onlineVersionGame);
                    } else {
                        Status = LauncherStatus.ready;

                    }
                }
                catch (Exception ex) {
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
                    _onlineVersion = new Version(webClient.DownloadString(onlineVersionFileGame));
                }
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://github.com/technoviumunlimited/technoviumunlimited_unity3d/releases/download/"+ _onlineVersion.ToString()  +"/Build.zip"), gameZipGame, _onlineVersion);
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
                string onlineVersionGame = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZipGame, rootPathGame, true);
                File.Delete(gameZipGame);
                File.WriteAllText(versionFileGame, onlineVersionGame);
                VersionText.Text = onlineVersionGame;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void startGameAndCloseCurrent()
        {
            if (File.Exists(gameExeGame) && Status == LauncherStatus.ready)
            {
                Debug.WriteLine("startGameAndCloseCurrent()");
                //start game and close this one
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExeGame);
                startInfo.WorkingDirectory = System.IO.Path.Combine(rootPathGame, "Build");
                Debug.WriteLine("startInfo.ToString():");
                Debug.WriteLine(startInfo.ToString());
                Process.Start(startInfo);
                Thread.Sleep(1000);
                Close();
            }
        }
        private void Play_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExeGame) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExeGame);
                startInfo.WorkingDirectory = System.IO.Path.Combine(rootPathGame, "Build");
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
