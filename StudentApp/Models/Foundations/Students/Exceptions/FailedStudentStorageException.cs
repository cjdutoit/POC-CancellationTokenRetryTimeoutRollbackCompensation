// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class FailedStudentStorageException : Xeption
    {
        public FailedStudentStorageException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
