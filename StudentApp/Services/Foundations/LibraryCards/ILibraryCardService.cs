// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Services.Foundations.LibraryCards
{
    internal interface ILibraryCardService
    {
        ValueTask<LibraryCard> AddLibraryCardApplicationAsync(Student student, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<LibraryCard>> RetrieveAllLibraryCards();
        ValueTask<LibraryCard> RetrieveLibraryCardByIdAsync(Guid libraryCardId, CancellationToken cancellationToken = default);
        ValueTask<LibraryCard> ModifyLibraryCardAsync(LibraryCard libraryCard, CancellationToken cancellationToken = default);
        ValueTask<LibraryCard> RemoveLibraryCardByIdAsync(Guid libraryCardId, CancellationToken cancellationToken = default);
    }
}
