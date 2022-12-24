using PDFEdit.Models;

namespace PDFEdit.Services
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
        /// Pathname of the input directory.
        /// </summary>
        private readonly string _inputDirectory;

        /// <summary>
        /// Pathname of the original directory.
        /// </summary>
        private readonly string _originalDirectory;

        /// <summary>
        /// Pathname of the output directory.
        /// </summary>
        private readonly string _outputDirectory;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFEdit.Services.DirectoryService"/> class.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="configuration">The configuration.</param>
        public DirectoryService(IConfiguration configuration)
        {
            // Get directory paths
            _inputDirectory = configuration["Directories:Input"];
            _originalDirectory = configuration["Directories:Original"];
            _outputDirectory = configuration["Directories:Output"];

            if (!Directory.Exists(_inputDirectory))
            {
                throw new Exception("Input directory does not exist");
            }

            if (!Directory.Exists(_outputDirectory))
            {
                throw new Exception("Output directory does not exist");
            }
        }

        /// <summary>
        /// Gets the document lists in this collection.
        /// </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the document lists in this collection.
        /// </returns>
        public IEnumerable<Document> GetDocumentList()
        {
            var documents = Directory.EnumerateFiles(_inputDirectory, $"*{PDF_EXTENSION}")
                .Where(x => !x.EndsWith(EDITING_PDF_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .Select(x => new Document
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Created = File.GetCreationTime(x),
                    LastModified = File.GetLastWriteTime(x)
                });

            return documents;
        }

        /// <summary>
        /// Gets document path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The document path.
        /// </returns>
        public string? GetDocumentPath(string name)
        {
            var editedPdfPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

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
        /// <param name="name">The name.</param>
        /// <returns>
        /// The unmodified document path.
        /// </returns>
        public string GetUnmodifiedDocumentPath(string name)
        {
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

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
        /// <param name="name">The name.</param>
        /// <returns>
        /// The editing document path.
        /// </returns>
        public string GetEditingDocumentPath(string name)
        {
            var editedPdfPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var pdfPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

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
        /// Gets document bytes.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetDocumentBytes(string name)
        {
            byte[] result;

            var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
            var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

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
        /// Deletes the document and any modifications.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Delete(string name)
        {
            lock (__lock)
            {
                var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

                if (File.Exists(editedPDFPath))
                {
                    File.Delete(editedPDFPath);
                }

                if (File.Exists(originalPDFPath))
                {
                    File.Delete(originalPDFPath);
                }
            }
        }

        /// <summary>
        /// Saves the document to the output directory.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Save(string name)
        {
            lock (__lock)
            {
                var editedPDFPath = Path.Combine(_inputDirectory, name) + EDITING_PDF_EXTENSION;
                var originalPDFPath = Path.Combine(_inputDirectory, name) + PDF_EXTENSION;

                var outputPath = Path.Combine(_outputDirectory, name) + PDF_EXTENSION;
                var originalOutputPath = Path.Combine(_originalDirectory, name) + PDF_EXTENSION;

                if (File.Exists(editedPDFPath))
                {
                    File.Move(editedPDFPath, outputPath);
                    File.Move(originalPDFPath, originalOutputPath);
                }
                else if (File.Exists(originalPDFPath))
                {
                    File.Move(originalPDFPath, outputPath);
                }
            }
        }
    }
}
