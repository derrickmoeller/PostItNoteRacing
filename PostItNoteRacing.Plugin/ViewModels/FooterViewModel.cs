using Newtonsoft.Json.Linq;
using PostItNoteRacing.Common;
using PostItNoteRacing.Common.Interfaces;
using PostItNoteRacing.Common.ViewModels;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Windows.Input;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class FooterViewModel(IDialogService dialogService) : InteractiveViewModel(dialogService)
    {
        private const string VersionsUrl = "https://api.github.com/repos/derrickmoeller/PostItNoteRacing/releases/latest";

        private string _currentVersion;
        private ICommand _gotoReleaseCommand;
        private string _releaseUrl;
        private bool _oneShot = false;

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

        public ICommand GotoReleaseCommand
        {
            get
            {
                _gotoReleaseCommand ??= new RelayCommand(x => GotoRelease(), CanGotoRelease);
                return _gotoReleaseCommand;
            }
        }

        public string InstalledVersion => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public bool IsCurrent => string.Compare(InstalledVersion, CurrentVersion) >= 0;

        private bool CanGotoRelease(object obj)
        {
            return _releaseUrl != null;
        }

        private void GotoRelease()
        {
            try
            {
                Process.Start(_releaseUrl);
            }
            catch (Exception ex)
            {
                DialogService.Show(ex.Message);
            }
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
            _releaseUrl = (string)jsonObject["html_url"];
        }
    }
}