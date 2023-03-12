using Octokit;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models.Config;
using System.Text;
using System.Text.Json;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// A configuration service.
    /// </summary>
    public class ConfigService
    {
        /// <summary>
        /// Filename of the configuration file.
        /// </summary>
        private const string CONFIG_FILENAME = "conf.json";

        /// <summary>
        /// Pathname of the configuration directory.
        /// </summary>
        private readonly string _configDirectory;

        /// <summary>
        /// Full pathname of the configuration file.
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        /// The release.
        /// </summary>
        public readonly string LatestRelease;

        /// <summary>
        /// The configuration.
        /// </summary>
        public Config Settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.ConfigService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ConfigService(IConfiguration configuration)
        {
            _configDirectory = configuration["Directories:Config"];
            _configFilePath = Path.Combine(_configDirectory, CONFIG_FILENAME);

            // Check the directory is accessible
            DirectoryHelpers.CheckDirectory(_configDirectory);

            // Check the config file exists
            if (!File.Exists(_configFilePath))
            {
                // And create it if it doesn't
                SaveConfig(new Config());
            }

            // Load config
            LoadConfiguration();
        }

        /// <summary>
        /// Updates the configuration described by updatedConfig.
        /// </summary>
        /// <param name="updatedConfig">The updated configuration.</param>
        public void UpdateConfiguration(Config updatedConfig)
        {
            // Persist changes to disk
            SaveConfig(updatedConfig);

            // Update settings object
            LoadConfiguration();
        }

        /// <summary>
        /// Reload configuration.
        /// </summary>
        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        #region Helpers

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private void LoadConfiguration()
        {
            // Load the config file
            using FileStream stream = File.OpenRead(_configFilePath);
            {
                Settings = JsonSerializer.Deserialize<Config>(stream);
            }

            // Load release info
            //GetLatestRelease(configuration["Github:Username"], configuration["Github:Repository"], configuration["Github:User-Agent"]);
        }

        /// <summary>
        /// Saves a configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private void SaveConfig(Config config)
        {
            using FileStream fs = File.Open(_configFilePath, System.IO.FileMode.Create);
            {
                var configuration = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true});
                var defaultConfigBytes = Encoding.UTF8.GetBytes(configuration);
                fs.Write(defaultConfigBytes, 0, defaultConfigBytes.Length);
            }
        }

        /// <summary>
        /// Gets the latest release info.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="userAgent">The user agent.</param>
        private void GetLatestRelease(string username, string repository, string userAgent)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue(userAgent));
            IReadOnlyList<Release> releases = client.Repository.Release.GetAll(username, repository).Result;

            // Setup the versions
            Version latestGitHubVersion = new Version(releases[0].TagName);
            Version localVersion = new Version("X.X.X"); //Replace this with your local version. 
                                                         //Only tested with numeric values.

            //Compare the Versions
            //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                //The version on GitHub is more up to date than this local release.
            }
            else if (versionComparison > 0)
            {
                //This local version is greater than the release version on GitHub.
            }
            else
            {
                //This local Version and the Version on GitHub are equal.
            }
        }

        #endregion
    }
}
