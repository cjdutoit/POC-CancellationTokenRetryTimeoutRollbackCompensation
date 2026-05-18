// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Orchestrations.Students.Exceptions
{
    internal class StudentOrchestrationServiceException : Xeption
    {
        public StudentOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
