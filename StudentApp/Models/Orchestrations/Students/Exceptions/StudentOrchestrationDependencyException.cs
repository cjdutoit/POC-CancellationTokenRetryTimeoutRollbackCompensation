// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Orchestrations.Students.Exceptions
{
    internal class StudentOrchestrationDependencyException : Xeption
    {
        public StudentOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
