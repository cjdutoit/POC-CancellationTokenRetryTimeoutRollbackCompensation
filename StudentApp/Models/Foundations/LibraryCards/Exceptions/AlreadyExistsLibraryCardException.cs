// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class AlreadyExistsLibraryCardException : Xeption
    {
        public AlreadyExistsLibraryCardException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
