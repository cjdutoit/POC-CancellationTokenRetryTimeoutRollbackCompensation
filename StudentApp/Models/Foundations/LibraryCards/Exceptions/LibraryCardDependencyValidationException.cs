// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class LibraryCardDependencyValidationException : Xeption
    {
        public LibraryCardDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
