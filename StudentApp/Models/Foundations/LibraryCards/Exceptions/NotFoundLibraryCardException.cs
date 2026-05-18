// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class NotFoundLibraryCardException : Xeption
    {
        public NotFoundLibraryCardException(string message)
            : base(message)
        { }
    }
}
