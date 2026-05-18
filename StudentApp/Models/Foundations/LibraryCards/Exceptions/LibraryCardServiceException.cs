// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class LibraryCardServiceException : Xeption
    {
        public LibraryCardServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
