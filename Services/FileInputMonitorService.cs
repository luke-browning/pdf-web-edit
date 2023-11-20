using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace PDFWebEdit.Services
{
    public class FileInputMonitorService : BackgroundService
    {
    
        private PhysicalFileProvider fileProvider;
        private static IChangeToken? _fileChangeToken;
        private readonly ConcurrentDictionary<string, DateTime> _files = new ConcurrentDictionary<string, DateTime>();

        private IHubContext<EventHub> _hubContext;
        private ILogger _logger;

        private string folderToWatchFor;
    
        public FileInputMonitorService(IConfiguration configuration, IHubContext<EventHub> hubContext, ILogger<FileInputMonitorService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            folderToWatchFor = Path.GetFullPath(configuration["Directories:Inbox"]);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try {
                this._logger.LogInformation($"FileInputMonitorService is starting and watching '{folderToWatchFor}'");
                // Needed to make it work under linux
                fileProvider = new PhysicalFileProvider(folderToWatchFor + "/"){
                    UsePollingFileWatcher = true,
                    UseActivePolling = true
                };
                
                WatchForFileChanges();
                // _fileChangeToken = fileProvider.Watch("*.*");
                // _fileChangeToken.RegisterChangeCallback(FileChanged, default);
    
            } catch(Exception ex){
                this._logger.LogWarning($"FileInputMonitorService cant watch '{folderToWatchFor}': ", ex.Message);
            }
            return Task.CompletedTask;
        }

            private void WatchForFileChanges()
            {
                _fileChangeToken = fileProvider.Watch("**/*.*");
                _fileChangeToken.RegisterChangeCallback(Notify, default);
            }

            private void Notify(object state)
            {
                _logger.LogInformation("File activity detected.");
                this._hubContext.Clients.All.SendAsync("fileschangedevent");
                WatchForFileChanges();
            }
    }
}