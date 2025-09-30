namespace Application.CustomExceptions
{
    public class NotFoundException(string? errorCode, string? errorField, string? errorMessage) : Exception
    {

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public string? ErrorCode { get; set; } = errorCode;

        /// <summary>
        /// Gets or sets the error field.
        /// </summary>
        /// <value>
        /// The error field.
        /// </value>
        public string? ErrorField { get; set; } = errorField;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string? ErrorMessage { get; set; } = errorMessage;
    }
}
