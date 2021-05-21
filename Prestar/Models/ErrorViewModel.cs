using System;

namespace Prestar.Models
{
    /// <summary>
    /// Error View Model Class
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Error request identification
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Checks if request identification is not null and is not empty
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
