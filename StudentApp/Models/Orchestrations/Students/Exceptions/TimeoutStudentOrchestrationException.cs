// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace StudentApp.Models.Orchestrations.Students.Exceptions
{
    internal class TimeoutStudentOrchestrationException : Xeption
    {
        public TimeoutStudentOrchestrationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TimeoutStudentOrchestrationException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
