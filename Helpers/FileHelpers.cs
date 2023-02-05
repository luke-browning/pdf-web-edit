namespace PDFWebEdit.Helpers
{
    public static class FileHelpers
    {
        /// <summary>
        /// Loads a file.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// An array of byte.
        /// </returns>
        public static byte[] LoadFile(string path)
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

        /// <summary>
        /// Loads file to memory stream.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// The file to stream.
        /// </returns>
        public static Stream LoadFileToMemoryStream(string path)
        {
            var ms = new MemoryStream();

            using (FileStream fs = File.OpenRead(path))
            {
                fs.CopyTo(ms);
            }

            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// Writes a file.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="content">The content.</param>
        public static void WriteFile(string path, byte[] content)
        {
            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(content, 0, content.Length);
            }
        }

        /// <summary>
        /// Writes a file.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="content">The content.</param>
        /// <returns>
        /// A path to the temporary file.
        /// </returns>
        public static string WriteTempFile(string path, Stream content)
        {
            var tmpFilePath = $"{path}.tmp";

            using (FileStream file = new FileStream(tmpFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                content.Position = 0;
                content.CopyTo(file);
            }

            return tmpFilePath;
        }

        /// <summary>
        /// Finalise temporary file.
        /// </summary>
        /// <param name="tempFilePath">Full pathname of the temporary file.</param>
        /// <param name="destinationFilePath">Full pathname of the destination file.</param>
        public static void FinaliseTempFile(string tempFilePath, string destinationFilePath)
        {
            File.Move(tempFilePath, destinationFilePath, true);
        }
    }
}
