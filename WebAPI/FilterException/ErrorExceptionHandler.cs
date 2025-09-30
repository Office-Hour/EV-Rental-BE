using Application.CustomExceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using WebAPI.Responses;

namespace WebAPI.FilterException
{
    public class ErrorExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// The exception handling
        /// </summary>
        private readonly Dictionary<Type, Func<Exception, ErrorMessage>> ExceptionHandling;

        /// <summary>
        /// The message
        /// </summary>
        const string message = "One or more validation errors occured.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorExceptionHandler"/> class.
        /// </summary>
        public ErrorExceptionHandler()
        {
            this.ExceptionHandling = new Dictionary<Type, Func<Exception, ErrorMessage>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotImplementedException), HandleNotImplementedException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(Exception), HandleGenericException }
            };
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public (int, ErrorMessage) HandleException(Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var errorMessage = new ErrorMessage();
            if (this.ExceptionHandling.TryGetValue(exception.GetType(), out var handler))
            {
                errorMessage = handler(exception);
            }
            else
            {
                errorMessage = HandleGenericException(exception);
            }

            return (statusCode, errorMessage);
        }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                InvalidTokenException => StatusCodes.Status400BadRequest,
                LockoutException => StatusCodes.Status401Unauthorized,
                LoginFailException => StatusCodes.Status401Unauthorized,
                NotImplementedException => StatusCodes.Status400BadRequest,
                UserNotFoundException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        /// <summary>
        /// Handles the validation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private ErrorMessage HandleValidationException(Exception exception)
        {
            var validationException = exception as ValidationException;
            ArgumentNullException.ThrowIfNull(validationException);

            var listError = new List<ErrorDetail>();
            foreach (var error in validationException.Errors)
            {
                listError.Add(new ErrorDetail()
                {
                    ErrorMessage = error.ErrorMessage,
                    ErrorCode = error.ErrorCode,
                    ErrorField = error.PropertyName,
                });
            }

            return new ErrorMessage()
            {
                Message = message,
                ErrorDetails = listError
            };
        }

        /// <summary>
        /// Handles the not implemented exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private ErrorMessage HandleNotImplementedException(Exception exception)
        {
            return new ErrorMessage()
            {
                Message = message,
                ErrorDetails =
                [
                    new ()
                    {
                        ErrorMessage = "This features is not implemented.",
                        ErrorCode = "NotImplemented",
                        ErrorField = "",
                    }
                ]
            };
        }

        /// <summary>
        /// Handles the not found exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private ErrorMessage HandleNotFoundException(Exception exception)
        {
            var notFoundException = exception as NotFoundException;
            ArgumentNullException.ThrowIfNull(notFoundException);

            return new ErrorMessage()
            {
                Message = message,
                ErrorDetails =
                [
                    new ()
                    {
                        ErrorMessage = notFoundException.ErrorMessage,
                        ErrorCode = notFoundException.ErrorCode,
                        ErrorField = notFoundException.ErrorField,
                    }
                ]
            };
        }

        /// <summary>
        /// Handles the generic exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private ErrorMessage HandleGenericException(Exception exception)
        {
            return new ErrorMessage()
            {
                Message = message,
                ErrorDetails =
                [
                    new ()
                    {
                        ErrorMessage = exception.Message,
                        ErrorCode = "",
                        ErrorField = exception.Source,
                    }
                ]
            };
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, errorMessage) = HandleException(exception);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorMessage, cancellationToken);

            return true;
        }
    }
}
