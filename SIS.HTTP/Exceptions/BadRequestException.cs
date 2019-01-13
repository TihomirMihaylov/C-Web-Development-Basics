using System;

namespace SIS.HTTP.Exceptions
{
    public class BadRequestException : Exception
    {
        private const string ErrorMessage = "The Request was malformed or contains unsupported elements.";

        //public const HttpStatusCode StatusCode = HttpStatusCode.BadRequest; //Гълов за какво го пише това?

        public BadRequestException() : base(ErrorMessage)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}