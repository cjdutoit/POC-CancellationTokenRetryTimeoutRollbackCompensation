// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Models.Foundations.Students.Exceptions
{
    internal class TimeoutStudentException : Xeption
    {
        public TimeoutStudentException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TimeoutStudentException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
