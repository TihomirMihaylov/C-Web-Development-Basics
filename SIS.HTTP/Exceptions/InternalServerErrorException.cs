using System;

namespace SIS.HTTP.Exceptions
{
    public class InternalServerErrorException : Exception
    {
        private const string ErrorMessage = "The Server has encountered an error.";

        //public const HttpStatusCode StatusCode = HttpStatusCode.InternalServerError; //Гълов за какво го пише това?

        public InternalServerErrorException() : base(ErrorMessage)
        {
        }

        public InternalServerErrorException(string message) : base(message)
        {
        }
    }
}