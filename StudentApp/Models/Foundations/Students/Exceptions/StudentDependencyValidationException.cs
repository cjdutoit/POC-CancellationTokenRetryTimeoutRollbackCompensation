// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class StudentDependencyValidationException : Xeption
    {
        public StudentDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
