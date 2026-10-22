namespace eShop.OrderProcessor
{
    /// <summary>
    /// Configuration for the grace-period background task.
    /// </summary>
    public class BackgroundTaskOptions
    {
        /// <summary>
        /// Gets or sets the grace period duration, in minutes.
        /// </summary>
        public int GracePeriodTime { get; set; }

        /// <summary>
        /// Gets or sets the interval between grace-period checks, in seconds.
        /// </summary>
        public int CheckUpdateTime { get; set; }
    }
}
