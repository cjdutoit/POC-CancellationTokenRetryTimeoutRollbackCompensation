// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class StudentValidationException : Xeption
    {
        public StudentValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
