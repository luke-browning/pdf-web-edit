using PDFEdit.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PDFEdit.Services
{
    /// <summary>
    /// A PDF service.
    /// </summary>
    public class PDFService
    {
        /// <summary>
        /// The document net.
        /// </summary>
        private readonly DocNetSingleton _docNet;

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFEdit.Services.PDFService"/> class.
        /// </summary>
        /// <param name="docNet">The document net.</param>
        public PDFService(DocNetSingleton docNet)
        {
            _docNet = docNet;
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
            byte[] file = LoadFile(path);

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
            byte[] file = LoadFile(path);

            // Open the document
            using (var doc = _docNet.Instance.GetDocReader(file, new Docnet.Core.Models.PageDimensions(width, height)))
            {
                var pageReader = doc.GetPageReader(pageNumber);

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

        /// <summary>
        /// Loads a file.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        private static byte[] LoadFile(string path)
        {
            byte[] file;

            using (var ms = new MemoryStream())
            using (FileStream fs = File.OpenRead(path))
            {
                fs.CopyTo(ms);
                file = ms.ToArray();
            }

            return file;
        }
    }
}
