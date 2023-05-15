namespace PDFWebEdit.Models.Config
{
    /// <summary>
    /// A header configuration.
    /// </summary>
    public class HeaderConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to make the header sticky.
        /// </summary>
        /// <value>
        /// True if sticky header, false if not.
        /// </value>
        public bool StickyHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the directory picker is shown.
        /// </summary>
        /// <value>
        /// True if show directory picker, false if not.
        /// </value>
        public bool ShowDirectoryPicker { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the search is shown.
        /// </summary>
        /// <value>
        /// True if show search, false if not.
        /// </value>
        public bool ShowSearch { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the preview size picker is shown.
        /// </summary>
        /// <value>
        /// True if show preview size picker, false if not.
        /// </value>
        public bool ShowPreviewSizePicker { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the sort picker is shown.
        /// </summary>
        /// <value>
        /// True if show sort picker, false if not.
        /// </value>
        public bool ShowSortPicker { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the colour mode picker is shown.
        /// </summary>
        /// <value>
        /// True if show colour mode picker, false if not.
        /// </value>
        public bool ShowColourModePicker { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the settings button is shown.
        /// </summary>
        /// <value>
        /// True if show settings button, false if not.
        /// </value>
        public bool ShowSettingsButton { get; set; } = true;

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
    }
}
