using Newtonsoft.Json.Linq;
using PostItNoteRacing.Common.Interfaces;
using PostItNoteRacing.Common.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class FooterViewModel(IDialogService dialogService) : NavigableViewModel(dialogService)
    {
        private const string VersionsUrl = "https://api.github.com/repos/derrickmoeller/PostItNoteRacing/releases/latest";

        private Version _gitHubVersion;
        private bool _oneShot = false;
        private string _releaseUrl;

        public Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        public Version GitHubVersion
        {
            get
            {
                if (_gitHubVersion == null && _oneShot == false)
                {
                    _oneShot = true;
                    SetCurrentVersionAsync();
                }

                return _gitHubVersion;
            }
            private set
            {
                if (_gitHubVersion != value)
                {
                    _gitHubVersion = value;
                    OnGitHubVersionChanged();
                }
            }
        }

        public bool IsCurrent => CurrentVersion.CompareTo(GitHubVersion) >= 0;

        public string ReleaseUrl
        {
            get => _releaseUrl;
            private set
            {
                if (_releaseUrl != value)
                {
                    _releaseUrl = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OnGitHubVersionChanged()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
            NotifyPropertyChanged(nameof(GitHubVersion));
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

            if (Version.TryParse(((string)jsonObject["tag_name"]).TrimStart('v'), out Version gitHubVersion) == true)
            {
                GitHubVersion = gitHubVersion;
            }

            ReleaseUrl = (string)jsonObject["html_url"];
        }
    }
}