// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class LibraryCardDependencyException : Xeption
    {
        public LibraryCardDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
