namespace CommandCentral.Enums
{
    /// <summary>
    /// Describes the levels of chain of command.
    /// </summary>
    public enum ChainOfCommandLevels
    {
        /// <summary>
        /// The default level indicating no presence in a chain of command.
        /// </summary>
        None = 0,
        /// <summary>
        /// The smallest grouping of personnel.
        /// </summary>
        Division = 1,
        /// <summary>
        /// Above a division and below a department.
        /// </summary>
        Department = 2,
        /// <summary>
        /// The largest grouping.
        /// </summary>
        Command = 3
    }
}