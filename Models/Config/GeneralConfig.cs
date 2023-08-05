using PDFWebEdit.Enumerations;

namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A general configuration.
    /// </summary>
    public class GeneralConfig
    {
        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        /// <value>
        /// The default language.
        /// </value>
        public string DefaultLanguage { get; set; } = "en";

        /// <summary>
        /// Gets or sets a value indicating whether the tour is enabled.
        /// </summary>
        /// <value>
        /// True if enable tour, false if not.
        /// </value>
        public bool EnableTour { get; set; } = true;

        /// <summary>
        /// Gets or sets the default folder.
        /// </summary>
        /// <value>
        /// The default folder.
        /// </value>
        public string DefaultFolder { get; set; } = "Inbox";

        /// <summary>
        /// Gets or sets the default sort column.
        /// </summary>
        /// <value>
        /// The default sort column.
        /// </value>
        public string DefaultSortColumn { get; set; } = "Name";

        /// <summary>
        /// Gets or sets the default sort direction.
        /// </summary>
        /// <value>
        /// The default sort direction.
        /// </value>
        public string DefaultSortDirection { get; set; } = "Asc";

        /// <summary>
        /// Gets or sets a value indicating whether the icons is shown.
        /// </summary>
        /// <value>
        /// True if show icons, false if not.
        /// </value>
        public bool ShowIcons { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the labels is shown.
        /// </summary>
        /// <value>
        /// True if show labels, false if not.
        /// </value>
        public bool ShowLabels { get; set; } = false;

        /// <summary>
        /// Gets or sets the default colour mode.
        /// </summary>
        /// <value>
        /// The colour mode.
        /// </value>
        public string DefaultColourMode { get; set; } = "auto";

        /// <summary>
        /// Gets or sets a value indicating whether the default show files on save as.
        /// </summary>
        /// <value>
        /// True if default show files on save as, false if not.
        /// </value>
        public bool DefaultShowFilesOnSaveAs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the debug mode.
        /// </summary>
        /// <value>
        /// True if debug mode, false if not.
        /// </value>
        public bool DebugMode { get; set; } = false;
    }
}
