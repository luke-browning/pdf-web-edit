namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A configuration.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the header configuration.
        /// </summary>
        /// <value>
        /// The header configuration.
        /// </value>
        public HeaderConfig HeaderConfig { get; set; } = new HeaderConfig();

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
        /// Gets or sets the inbox configuration.
        /// </summary>
        /// <value>
        /// The inbox configuration.
        /// </value>
        public InboxConfig InboxConfig { get; set; } = new InboxConfig();

        /// <summary>
        /// Gets or sets the outbox configuration.
        /// </summary>
        /// <value>
        /// The outbox configuration.
        /// </value>
        public OutboxConfig OutboxConfig { get; set; } = new OutboxConfig();

        /// <summary>
        /// Gets or sets the archive configuration.
        /// </summary>
        /// <value>
        /// The archive configuration.
        /// </value>
        public ArchiveConfig ArchiveConfig { get; set; } = new ArchiveConfig();
    }
}
