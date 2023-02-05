using Docnet.Core.Exceptions;
using PDFWebEdit.Enumerations;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PDFWebEdit.Services
{
    /// <summary>
    /// A DocNet PDF service.
    /// </summary>
    public class DocNetPDFService : IPDFService
    {
        /// <summary>
        /// The document net.
        /// </summary>
        private readonly DocNetSingleton _docNet;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFWebEdit.Services.DocNetPDFService"/> class.
        /// </summary>
        /// <param name="docNet">The DocNet service.</param>
        public DocNetPDFService(DocNetSingleton docNet)
        {
            _docNet = docNet;
        }

        /// <summary>
        /// Gets document status.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// The document status.
        /// </returns>
        public DocumentStatus GetDocumentStatus(string path)
        {
            var status = DocumentStatus.Ok;

            // Get the file 
            byte[] file = FileHelpers.LoadFile(path);

            try
            {
                using (var reader = _docNet.Instance.GetDocReader(file, new Docnet.Core.Models.PageDimensions()))
                {
                    // We're just testing we can read the document
                }
            }
            catch (DocnetLoadDocumentException ex)
            {
                switch (ex.ErrorCode)
                {
                    default:
                    case 3:
                        status = DocumentStatus.Corrupted;
                        break;

                    case 4:
                        status = DocumentStatus.PasswordProtected;
                        break;
                }
            }
            catch (Exception x)
            {
                status = DocumentStatus.Corrupted;
            }

            return status;
        }

        /// <summary>
        /// Gets page count.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// The page count.
        /// </returns>
        public int GetPageCount(string path)
        {
            int pages = 0;

            // Get the file 
            byte[] file = FileHelpers.LoadFile(path);

            using (var reader = _docNet.Instance.GetDocReader(file, new Docnet.Core.Models.PageDimensions()))
            {
                pages = reader.GetPageCount();
            }

            return pages;
        }

        /// <summary>
        /// Gets page preview.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetPagePreview(string path, int pageNumber, int width, int height)
        {
            byte[] result;

            byte[] rawBytes;
            int pageWidth;
            int pageHeight;

            // Get the file 
            byte[] file = FileHelpers.LoadFile(path);

            // Open the document
            using (var doc = _docNet.Instance.GetDocReader(file, new Docnet.Core.Models.PageDimensions(width, height)))
            using (var pageReader = doc.GetPageReader(pageNumber))
            {
                // Render the image
                rawBytes = pageReader.GetImage();

                pageWidth = pageReader.GetPageWidth();
                pageHeight = pageReader.GetPageHeight();
            }

            // Convert it to a byte array
            using (var img = Image.LoadPixelData<Bgra32>(rawBytes, pageWidth, pageHeight))
            using (var stream = new MemoryStream())
            {
                img.SaveAsPng(stream);
                result = stream.ToArray();
            }

            return result;
        }
    }
}
