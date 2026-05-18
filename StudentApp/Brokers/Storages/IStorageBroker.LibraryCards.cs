// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudentApp.Models.Foundations.LibraryCards;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial interface IStorageBroker
    {
        ValueTask<LibraryCard> InsertLibraryCardAsync(LibraryCard libraryCard, CancellationToken cancellationToken = default);
        IQueryable<LibraryCard> SelectAllLibraryCards();
        ValueTask<LibraryCard> SelectLibraryCardByIdAsync(Guid libraryCardId, CancellationToken cancellationToken = default);
        ValueTask<LibraryCard> UpdateLibraryCardAsync(LibraryCard libraryCard, CancellationToken cancellationToken = default);
        ValueTask<LibraryCard> DeleteLibraryCardAsync(LibraryCard libraryCard, CancellationToken cancellationToken = default);
    }
}
