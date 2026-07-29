namespace TurtlePath.Application.Models.Responses
{
    /// <summary>
    /// Represents the result of a paged query, including pagination details and the result set.
    /// </summary>
    /// <typeparam name="TResponse">The type of the items in the result set.</typeparam>
    public class PagedResponse<TResponse>
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of rows in the result set.
        /// </summary>
        public long RowCount { get; set; }

        /// <summary>
        /// Gets or sets the results for the current page.
        /// </summary>
        public IEnumerable<TResponse> Results { get; set; } = new List<TResponse>();
    }
}
