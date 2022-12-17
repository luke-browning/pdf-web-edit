using PDFEdit.Models;
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
            //PdfDocument pdfDocument = new PdfDocument(new PdfReader(path));
            //var pages = pdfDocument.GetNumberOfPages();
            //pdfDocument.Close();

            var reader = _docNet.Instance.GetDocReader(path, new Docnet.Core.Models.PageDimensions());
            var pages = reader.GetPageCount();

            return pages;
        }

        /// <summary>
        /// Gets page preview.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public byte[] GetPagePreview(string path, int pageNumber, ImageFormat imageFormat)
        {
            //PdfDocument pdfDocument = new PdfDocument(new PdfReader(path));
            //var page = pdfDocument.GetPage(pageNumber);
            //var thumb = page.GetThumbnailImage();
            //pdfDocument.Close();
            //Image image = new Image(thumb);

            byte[] result = null;

            // Open the document
            var doc = _docNet.Instance.GetDocReader(path, new Docnet.Core.Models.PageDimensions(565, 800));
            var pageReader = doc.GetPageReader(pageNumber);

            // Render the image
            var rawBytes = pageReader.GetImage();

            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                // Create the bitmap
                AddBytes(bmp, rawBytes);

                // Convert it to a byte array
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, imageFormat);
                    result = stream.ToArray();
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the bytes to 'rawBytes'.
        /// </summary>
        /// <param name="bmp">The bitmap.</param>
        /// <param name="rawBytes">The raw in bytes.</param>
        private static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}
