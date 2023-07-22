using PDFWebEdit.Models.Requests.Batch;

namespace PDFWebEdit.Models.Api
{
    /// <summary>
    /// A batch response.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public class BatchResponse<T> : List<T> where T : IBatchResult
    {
        /// <summary>
        /// Gets status code based on the response values.
        /// </summary>
        /// <returns>
        /// The status code.
        /// </returns>
        public int GetStatusCode()
        {
            var statusCodes = this.DistinctBy(x => x.StatusCode);
            var differntStatusCodes = statusCodes.Count();

            switch (differntStatusCodes)
            {
                case int x when differntStatusCodes == 0:
                default:
                    return StatusCodes.Status204NoContent;
                    break;

                case int x when differntStatusCodes == 1:
                    return statusCodes.First().StatusCode;
                    break;

                case int x when differntStatusCodes > 1:
                    return StatusCodes.Status207MultiStatus;
                    break;
            }
        }
    }
}
