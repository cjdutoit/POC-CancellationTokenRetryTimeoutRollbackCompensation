// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

namespace StudentApp.Models.Foundations.LibraryCards
{
    internal class LibraryCard
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public LibraryCardStatus Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
