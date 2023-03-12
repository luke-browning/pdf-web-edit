namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A configuration.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the general configuration.
        /// </summary>
        /// <value>
        /// The general configuration.
        /// </value>
        public GeneralConfig GeneralConfig { get; set; } = new GeneralConfig();

        /// <summary>
        /// Gets or sets the preview configuration.
        /// </summary>
        /// <value>
        /// The preview configuration.
        /// </value>
        public PreviewConfig PreviewConfig { get; set; } = new PreviewConfig();

        /// <summary>
        /// Gets or sets the input configuration.
        /// </summary>
        /// <value>
        /// The input configuration.
        /// </value>
        public InputConfig InputConfig { get; set; } = new InputConfig();

        /// <summary>
        /// Gets or sets the output configuration.
        /// </summary>
        /// <value>
        /// The output configuration.
        /// </value>
        public OutputConfig OutputConfig { get; set; } = new OutputConfig();

        /// <summary>
        /// Gets or sets the trash configuration.
        /// </summary>
        /// <value>
        /// The trash configuration.
        /// </value>
        public TrashConfig TrashConfig { get; set; } = new TrashConfig();
    }
}
