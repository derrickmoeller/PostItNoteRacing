using Newtonsoft.Json.Linq;
using PostItNoteRacing.Common;
using PostItNoteRacing.Common.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Windows.Input;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class FooterViewModel : ViewModelBase
    {
        private const string Filename = "PostItNoteRacing.Plugin.dll";
        private const string VersionsUrl = "https://api.github.com/repos/derrickmoeller/PostItNoteRacing/releases/latest";

        private string _currentVersion;
        private ICommand _downloadCommand;
        private string _downloadUrl;
        private bool _oneShot = false;

        public FooterViewModel()
        {
        }

        public string CurrentVersion
        {
            get
            {
                if (_currentVersion == null && _oneShot == false)
                {
                    _oneShot = true;
                    SetCurrentVersionAsync();
                }

                return _currentVersion;
            }
            set
            {
                if (_currentVersion != value)
                {
                    _currentVersion = value;
                    OnCurrentVersionChanged();
                }
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                _downloadCommand ??= new RelayCommand(x => Download(), CanDownload);
                return _downloadCommand;
            }
        }

        public string InstalledVersion => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public bool IsCurrent => string.Compare(InstalledVersion, CurrentVersion) >= 0;

        private bool CanDownload(object obj)
        {
            return _downloadUrl != null;
        }

        private void Download()
        {
            Process.Start(_downloadUrl);
        }

        private void OnCurrentVersionChanged()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
            NotifyPropertyChanged(nameof(CurrentVersion));
        }

        private async void SetCurrentVersionAsync()
        {
            string json;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                json = await httpClient.GetStringAsync(VersionsUrl);
            }

            var jsonObject = JObject.Parse(json);

            CurrentVersion = (string)jsonObject["tag_name"];
            _downloadUrl = (string)jsonObject["assets"].SingleOrDefault(x => (string)x["name"] == Filename)?["browser_download_url"];
        }
    }
}