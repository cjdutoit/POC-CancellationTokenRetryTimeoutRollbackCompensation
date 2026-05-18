// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class FailedStudentServiceException : Xeption
    {
        public FailedStudentServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
