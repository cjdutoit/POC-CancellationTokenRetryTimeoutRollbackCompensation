// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Models.Foundations.LibraryCards.Exceptions
{
    internal class TimeoutLibraryCardException : Xeption
    {
        public TimeoutLibraryCardException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public TimeoutLibraryCardException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
