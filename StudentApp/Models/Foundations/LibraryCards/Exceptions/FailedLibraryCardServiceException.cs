// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class FailedLibraryCardServiceException : Xeption
    {
        public FailedLibraryCardServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
