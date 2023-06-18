using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// A directory service.
    /// </summary>
    public class DirectoryService
    {
        /// <summary>
        /// The lock.
        /// </summary>
        private object __lock = new object();

        /// <summary>
        /// The PDF extension.
        /// </summary>
        private const string PDF_EXTENSION = ".pdf";

        /// <summary>
        /// The editing PDF extension.
        /// </summary>
        private const string EDITING_PDF_EXTENSION = ".edit.pdf";

        /// <summary>
        /// Pathname of the inbox directory.
        /// </summary>
        private readonly string _inboxDirectory;

        /// <summary>
        /// Pathname of the archive directory.
        /// </summary>
        private readonly string _archiveDirectory;

        /// <summary>
        /// Pathname of the outbox directory.
        /// </summary>
        private readonly string _outboxDirectory;

        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly ConfigService _configService;

        /// <summary>
        /// The PDF service.
        /// </summary>
        private readonly IPDFService _pdfService;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.DirectoryService"/> class.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="configuration">The configuration.</param>
        /// <param name="configService">The configuration service.</param>
        /// <param name="pdfService">The PDF service.</param>
        public DirectoryService(IConfiguration configuration, ConfigService configService, IPDFService pdfService)
        {
            // Get directory paths
            _inboxDirectory = configuration["Directories:Inbox"];
            _archiveDirectory = configuration["Directories:Archive"];
            _outboxDirectory = configuration["Directories:Outbox"];

            // Check the directories are writable
            DirectoryHelpers.CheckDirectory(_inboxDirectory);
            DirectoryHelpers.CheckDirectory(_archiveDirectory);
            DirectoryHelpers.CheckDirectory(_outboxDirectory);

            // The config service
            _configService = configService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Gets the folders in the save directory.
        /// </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the folders in this collection.
        /// </returns>
        public IEnumerable<Folder> GetFolders()
        {
            string directory = GetTargetDirectoryPath(TargetDirectory.Outbox);

            // Recursively get folders
            var tree = GetFolders(directory);

            return tree;
        }

        /// <summary>
        /// Gets the document lists in this collection.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="includeSubdirectories">Include documents in subdirectories.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the document lists in this collection.
        /// </returns>
        public IEnumerable<Document> GetDocumentList(TargetDirectory targetDirectory, bool includeSubdirectories = false)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);

            // Search options
            var searchOption = SearchOption.TopDirectoryOnly;

            if (includeSubdirectories)
            {
                searchOption = SearchOption.AllDirectories;
            }

            // Load docs
            var documents = Directory.EnumerateFiles(directory, $"*{PDF_EXTENSION}", searchOption)
                .Where(x => !x.EndsWith(EDITING_PDF_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .Select(path => {

                    var editedPdfPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + EDITING_PDF_EXTENSION);

                    var doc = new Document
                    {
                        Name = Path.GetFileNameWithoutExtension(path),
                        Directory = Path.GetDirectoryName(path).Replace(directory, string.Empty),
                        Created = File.GetCreationTime(path),
                        LastModified = File.GetLastWriteTime(path),
                        HasChanges = File.Exists(editedPdfPath),
                        Status = File.Exists(editedPdfPath) ? _pdfService.GetDocumentStatus(editedPdfPath) : _pdfService.GetDocumentStatus(path),
                    };

                    return doc;
                });

            return documents;
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="targetDirectory">Target directory.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public Document GetDocument(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            Document doc = null;

            string directory = GetTargetDirectoryPath(targetDirectory);

            string pdfPath = GetUnmodifiedDocumentPath(targetDirectory, subDirectory, name);

            if (pdfPath != null)
            {
                var editedPdfPath = Path.Combine(Path.GetDirectoryName(pdfPath), Path.GetFileNameWithoutExtension(pdfPath) + EDITING_PDF_EXTENSION);

                // Load doc
                doc = new Document
                {
                    Name = Path.GetFileNameWithoutExtension(pdfPath),
                    Directory = Path.GetDirectoryName(pdfPath).Replace(directory, string.Empty),
                    Created = File.GetCreationTime(pdfPath),
                    LastModified = File.GetLastWriteTime(pdfPath),
                    HasChanges = File.Exists(editedPdfPath),
                    Status = File.Exists(editedPdfPath) ? _pdfService.GetDocumentStatus(editedPdfPath) : _pdfService.GetDocumentStatus(pdfPath),
                };
            }

            return doc;
        }

        /// <summary>
        /// Gets document path.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The document path.
        /// </returns>
        public string? GetDocumentPath(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            var editedPdfPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(directory, name) + PDF_EXTENSION;

            if (File.Exists(editedPdfPath))
            {
                return editedPdfPath;
            }
            else if (File.Exists(pdfPath))
            {
                return pdfPath;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets unmodified document path.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The unmodified document path.
        /// </returns>
        public string GetUnmodifiedDocumentPath(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            var pdfPath = Path.Combine(directory, name) + PDF_EXTENSION;

            if (File.Exists(pdfPath))
            {
                return pdfPath;
            }
            else
            {
                throw new Exception($"Source file does not exist: {pdfPath}");
            }
        }

        /// <summary>
        /// Gets editing document path.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The editing document path.
        /// </returns>
        public string GetEditingDocumentPath(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            var editedPdfPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(directory, name) + PDF_EXTENSION;

            // Check the original file exists before returning an editing path
            if (File.Exists(pdfPath))
            {
                return editedPdfPath;
            }
            else
            {
                throw new Exception($"Source file does not exist: {pdfPath}");
            }
        }

        /// <summary>
        /// Gets the next document version number path (E.g. document.pdf returns document_1.pdf).
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The next document version number path.
        /// </returns>
        public string GetNextDocumentVersionNumberPath(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            var newName = name;
            var index = 1;

            // Find all the files that match the name of the document
            var dir = new DirectoryInfo(directory);
            var files = dir.GetFiles($"{name}*", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var filename = Path.GetFileNameWithoutExtension(file.FullName);
                var fileEnding = filename.Remove(0, name.Length);

                if (fileEnding.Contains("_"))
                {
                    fileEnding = fileEnding.Remove(0, fileEnding.LastIndexOf('_') + 1);
                }

                int value;

                int.TryParse(fileEnding, out value);

                if (value > index)
                {
                    index = value + 1;
                }
                else if (value == index)
                {
                    index = index + 1;
                }
            }

            if (index > 0)
            {
                newName = $"{name}_{index}" + PDF_EXTENSION;
            }

            return Path.Combine(directory, newName);
        }

        /// <summary>
        /// Renames a file.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        /// <param name="newName">Name of the new.</param>
        public void Rename(TargetDirectory targetDirectory, string? subDirectory, string name, string newName)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            lock (__lock)
            {
                var editedPDFPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(directory, name) + PDF_EXTENSION;

                if (newName.Any(x => Path.GetInvalidFileNameChars().Contains(x)))
                {
                    throw new Exception($"New file name contains invalid characters");
                }

                if (File.Exists(editedPDFPath))
                {
                    var newEditedPDFPath = Path.Combine(directory, newName) + EDITING_PDF_EXTENSION;

                    if (File.Exists(newEditedPDFPath))
                    {
                        throw new Exception($"New file name already exists: {newEditedPDFPath}");
                    }

                    File.Move(editedPDFPath, newEditedPDFPath);
                }

                if (File.Exists(originalPDFPath))
                {
                    var newOriginalPDFPath = Path.Combine(directory, newName) + PDF_EXTENSION;

                    if (File.Exists(newOriginalPDFPath))
                    {
                        throw new Exception($"New file name already exists: {newOriginalPDFPath}");
                    }

                    File.Move(originalPDFPath, newOriginalPDFPath);
                }
            }
        }

        /// <summary>
        /// Gets document bytes.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetDocumentBytes(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            byte[] result;

            var editedPDFPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
            var originalPDFPath = Path.Combine(directory, name) + PDF_EXTENSION;

            if (File.Exists(editedPDFPath))
            {
                result = File.ReadAllBytes(editedPDFPath);
            }
            else  if (File.Exists(originalPDFPath))
            {
                result = File.ReadAllBytes(originalPDFPath);
            }
            else
            {
                throw new Exception($"Source file does not exist: {originalPDFPath}");
            }

            return result;
        }

        /// <summary>
        /// Archive the document and any modifications.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        public void Archive(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            lock (__lock)
            {
                var editedPDFPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(directory, name) + PDF_EXTENSION;

                var editedPDFArchivePath = Path.Combine(_archiveDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFArchivePath = Path.Combine(_archiveDirectory, name) + PDF_EXTENSION;

                // Make sure a file doesn't already exist in the inbox location
                if (File.Exists(editedPDFArchivePath))
                {
                    throw new Exception("A file with the same name already exists in the archive directory.");
                }

                if (File.Exists(originalPDFArchivePath))
                {
                    throw new Exception("A file with the same name already exists in the archive directory.");
                }

                // Move the files
                if (File.Exists(editedPDFPath))
                {
                    if (_configService.Settings.InboxConfig.DeleteDocumentOnArchive)
                    {
                        File.Delete(editedPDFPath);
                    }
                    else
                    {
                        File.Move(editedPDFPath, editedPDFArchivePath);
                    }
                }

                if (File.Exists(originalPDFPath))
                {
                    if (_configService.Settings.InboxConfig.DeleteDocumentOnArchive)
                    {
                        File.Delete(originalPDFPath);
                    }
                    else
                    {
                        File.Move(originalPDFPath, originalPDFArchivePath);
                    }
                }
            }
        }

        /// <summary>
        /// Permenently delete from archive.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="name">The name.</param>
        public void PermenentlyDeleteFromArchive(string name)
        {
            lock (__lock)
            {
                var editedPDFArchivePath = Path.Combine(_archiveDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFArchivePath = Path.Combine(_archiveDirectory, name) + PDF_EXTENSION;

                // Move the files
                if (File.Exists(editedPDFArchivePath))
                {
                    File.Delete(editedPDFArchivePath);
                }

                if (File.Exists(originalPDFArchivePath))
                {
                    File.Delete(originalPDFArchivePath);
                }
            }
        }

        /// <summary>
        /// Restores a deleted document to the inbox directory.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="subDirectory">Subdirectory storing document.</param>
        /// <param name="name">The name.</param>
        public void Restore(TargetDirectory targetDirectory, string? subDirectory, string name)
        {
            string directory = GetTargetDirectoryPath(targetDirectory);
            directory = Path.Join(directory, subDirectory ?? string.Empty);

            lock (__lock)
            {
                var editedPDFPath = Path.Combine(directory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(directory, name) + PDF_EXTENSION;

                var editedPDFInboxDirectoryPath = Path.Combine(_inboxDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFInboxDirectoryPath = Path.Combine(_inboxDirectory, name) + PDF_EXTENSION;

                // Make sure a file doesn't already exist in the inbox location
                if (File.Exists(editedPDFInboxDirectoryPath))
                {
                    throw new Exception("A file with the same name already exists in the inbox directory.");
                }

                if (File.Exists(originalPDFInboxDirectoryPath))
                {
                    throw new Exception("A file with the same name already exists in the inbox directory.");
                }

                // Move the files
                if (File.Exists(editedPDFPath))
                {
                    File.Move(editedPDFPath, editedPDFInboxDirectoryPath);
                }

                if (File.Exists(originalPDFPath))
                {
                    File.Move(originalPDFPath, originalPDFInboxDirectoryPath);
                }
            }
        }

        /// <summary>
        /// Saves the document to the outbox directory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sourceSubDirectory">The source sub directory.</param>
        /// <param name="targetSubDirectory">The target sub directory.</param>
        public void Save(string name, string? sourceSubDirectory, string? targetSubDirectory)
        {
            string sourceDirectory = GetTargetDirectoryPath(TargetDirectory.Inbox);
            sourceDirectory = Path.Join(sourceDirectory, sourceSubDirectory ?? string.Empty);

            string outboxDirectory = GetTargetDirectoryPath(TargetDirectory.Outbox);
            outboxDirectory = Path.Join(outboxDirectory, targetSubDirectory ?? string.Empty);

            lock (__lock)
            {
                var editedPDFPath = Path.Combine(sourceDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(sourceDirectory, name) + PDF_EXTENSION;

                string outboxPath = Path.Combine(outboxDirectory, name);

                outboxPath += PDF_EXTENSION;
                var archiveOutboxPath = Path.Combine(_archiveDirectory, name) + PDF_EXTENSION;

                // Make sure a file doesn't already exist in the outbox location
                if (File.Exists(outboxPath))
                {
                    throw new Exception("A file with the same name already exists in the outbox directory.");
                }

                // Move the file
                if (File.Exists(editedPDFPath))
                {
                    // If we're going to be deleting the original file on save, make sure that 
                    // we don't continue if it already exists in the archive folder
                    if ((_configService.Settings.InboxConfig.ArchiveOriginalFileOnSave) && (File.Exists(archiveOutboxPath)))
                    {
                        throw new Exception("A file with the same name already exists in the archive directory. Save aborted.");
                    }

                    File.Move(editedPDFPath, outboxPath);

                    if (_configService.Settings.InboxConfig.ArchiveOriginalFileOnSave)
                    {
                        File.Move(originalPDFPath, archiveOutboxPath);
                    }
                    else
                    {
                        File.Delete(originalPDFPath);
                    }
                }
                else if (File.Exists(originalPDFPath))
                {
                    File.Move(originalPDFPath, outboxPath);
                }
            }
        }

        #region Helpers

        /// <summary>
        /// Gets target directory path.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <returns>
        /// The target directory path.
        /// </returns>
        private string GetTargetDirectoryPath(TargetDirectory targetDirectory)
        {
            string directory;

            switch (targetDirectory)
            {
                default:
                case TargetDirectory.Inbox:
                    directory = _inboxDirectory;
                    break;

                case TargetDirectory.Outbox:
                    directory = _outboxDirectory;
                    break;

                case TargetDirectory.Archive:
                    directory = _archiveDirectory;
                    break;
            }

            return directory;
        }

        /// <summary>
        /// Gets the folders in the defined directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the folders in this collection.
        /// </returns>
        private IEnumerable<Folder> GetFolders(string directory)
        {
            var result = Directory.GetDirectories(directory)
                .Select(x => new Folder
                {
                    Name = Path.GetFileName(x),
                    SubFolders = GetFolders(Path.Combine(directory, x))
                });

            return result;
        }

        #endregion
    }
}
