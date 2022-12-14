using Docnet.Core;

namespace PDFEdit.Models
{
    /// <summary>
    /// A document net singleton.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class DocNetSingleton : IDisposable
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public IDocLib Instance { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="PDFEdit.Models.DocNetSingleton"/> class.
        /// </summary>
        public DocNetSingleton()
        {
            Instance = DocLib.Instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <seealso cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            Instance.Dispose();
        }
    }
}
