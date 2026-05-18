// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class InvalidStudentException : Xeption
    {
        public InvalidStudentException(string message)
            : base(message)
        { }
    }
}
