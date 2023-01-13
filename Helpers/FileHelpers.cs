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
    }
}
