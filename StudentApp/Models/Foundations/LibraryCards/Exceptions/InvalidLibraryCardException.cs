// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class InvalidLibraryCardException : Xeption
    {
        public InvalidLibraryCardException(string message)
            : base(message)
        { }
    }
}
