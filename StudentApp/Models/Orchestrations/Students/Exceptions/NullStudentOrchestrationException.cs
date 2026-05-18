// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Orchestrations.Students.Exceptions
{
    internal class NullStudentOrchestrationException : Xeption
    {
        public NullStudentOrchestrationException(string message)
            : base(message)
        { }
    }
}
