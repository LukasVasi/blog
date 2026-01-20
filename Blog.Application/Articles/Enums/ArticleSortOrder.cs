namespace Blog.Application.Articles.Enums
{
    /// <summary>
    /// The sorting order of article lists.
    /// </summary>
    public enum ArticleSortOrder
    {
        /// <summary>
        /// Sorts by creation date latest to oldest.
        /// </summary>
        Latest,

        /// <summary>
        /// Sorts by creation date oldest to latest.
        /// </summary>
        Oldest,

        /// <summary>
        /// Sorts by article rating highest to lowest
        /// and then by creation date latest to oldest (as tiebreaker).
        /// </summary>
        Ranking,

        /// <summary>
        /// This sorting order works in combination with search queries.
        /// 
        /// <para>
        /// When no search query is provided - works the same as ranking
        /// sort: sorts by article rating highest to lowest
        /// and then by creation date latest to oldest (as tiebreaker).
        /// </para>
        /// 
        /// <para>
        /// When a search query is provided sorts by relevence: 
        /// first orders by exact title match to search query, 
        /// then by title starts with search query, 
        /// then orders by title contains the search query, 
        /// then by text contains, 
        /// then by author username match to search query,
        /// then by rating and finally by date.
        /// </para>
        /// </summary>
        Relevance
    }
}
