// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentApp.Models.Foundations.LibraryCards;

namespace StudentApp.Brokers.Storages
{
    internal partial class StorageBroker
    {
        public DbSet<LibraryCard> LibraryCards { get; set; }

        public async ValueTask<LibraryCard> InsertLibraryCardAsync(
            LibraryCard libraryCard,
            CancellationToken cancellationToken = default) =>
                await this.InsertAsync(libraryCard, cancellationToken);

        public IQueryable<LibraryCard> SelectAllLibraryCards() =>
            this.Set<LibraryCard>();

        public async ValueTask<LibraryCard> SelectLibraryCardByIdAsync(
            Guid libraryCardId,
            CancellationToken cancellationToken = default) =>
                await this.SelectAsync<LibraryCard>(new object[] { libraryCardId }, cancellationToken);

        public async ValueTask<LibraryCard> UpdateLibraryCardAsync(
            LibraryCard libraryCard,
            CancellationToken cancellationToken = default) =>
                await this.UpdateAsync(libraryCard, cancellationToken);

        public async ValueTask<LibraryCard> DeleteLibraryCardAsync(
            LibraryCard libraryCard,
            CancellationToken cancellationToken = default) =>
                await this.DeleteAsync(libraryCard, cancellationToken);
    }
}
