namespace PDFEdit.Helpers
{
    /// <summary>
    /// A directory helpers.
    /// </summary>
    public static class DirectoryHelpers
    {
        /// <summary>
        /// Check if a directory is writeable.
        /// </summary>
        /// <param name="path">Full pathname of the file.</param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        public static bool IsDirectoryWriteable(string path)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(path, Path.GetRandomFileName()), 
                    1, 
                    FileOptions.DeleteOnClose))
                { 
                    // Do nothing
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check directory is usable.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="path">Full pathname of the file.</param>
        public static void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception($"Directory does not exist: {path}");
            }

            if (!IsDirectoryWriteable(path))
            {
                throw new Exception($"Unable to write to path: {path}");
            }
        }
    }
}
