using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }

    public class ValidationAppException : AppException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationAppException(string message) : base(message, 400)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationAppException(IDictionary<string, string[]> errors) : base("One or more validation errors occurred.", 400)
        {
            Errors = errors;
        }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409) { }
    }

    public class UnauthorizedAppException : AppException
    {
        public UnauthorizedAppException(string message) : base(message, 401) { }
    }
}
