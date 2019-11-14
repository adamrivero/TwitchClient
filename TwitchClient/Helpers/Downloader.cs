using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace TwitchClient.Helpers
{
    class Downloader
    {
        private Uri _videoURL;
        private Process _proc;
        private StorageFolder localFolder;
        private string login;
        private string time;


        public Downloader(Uri url, string login, string time)
        {
            this.login = login;
            this.time = time;
            localFolder = ApplicationData.Current.LocalFolder;
            _videoURL = url;
            var thread = new Thread(StartProc);
            thread.Start();
        }

        private void StartProc()
        {
            _proc = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-i {_videoURL} -acodec copy -vcodec copy -absf aac_adtstoasc \"" +
                                localFolder.Path + $"\\{login}-{time}.mp4",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true,
            };

            _proc.Start();
            using (var reader = _proc.StandardError)
            {
                var thisline = reader.ReadLine();
                while (true)
                {
                    Debug.WriteLine(thisline);
                    thisline = reader.ReadLine();
                }
            }
        }
        public async void Stop()
        {
            var buffer = _proc.StandardInput;
            buffer.WriteLine("q");
            await Launcher.LaunchFolderPathAsync(localFolder.Path);
        }
    }
}

