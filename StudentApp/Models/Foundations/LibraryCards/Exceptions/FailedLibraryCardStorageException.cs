// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class FailedLibraryCardStorageException : Xeption
    {
        public FailedLibraryCardStorageException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
