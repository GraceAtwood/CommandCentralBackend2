namespace CommandCentral.Enums
{
    /// <summary>
    /// Indicates the different ways a value can be searched.
    /// </summary>
    public enum SearchDataTypes
    {
        /// <summary>
        /// A string search uses strings to do the searching.
        /// </summary>
        String,

        /// <summary>
        /// A boolean search uses booleans.
        /// </summary>
        Boolean,

        /// <summary>
        /// A date time search uses date time ranges to do a search.
        /// </summary>
        DateTime
    }
}