// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using GitFyle.Core.Api.Brokers.Storages;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Services.Foundations.LibraryCards
{
    internal partial class LibraryCardService : ILibraryCardService
    {
        private readonly IStorageBroker storageBroker;

        public LibraryCardService(IStorageBroker storageBroker) =>
            this.storageBroker = storageBroker;

        public ValueTask<LibraryCard> AddLibraryCardApplicationAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
                TryCatch(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateStudent(student);

                    var libraryCard = new LibraryCard
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        Status = LibraryCardStatus.Pending,
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    return await this.storageBroker.InsertLibraryCardAsync(libraryCard, cancellationToken);
                });

        public ValueTask<IQueryable<LibraryCard>> RetrieveAllLibraryCards() =>
            TryCatch(async () => this.storageBroker.SelectAllLibraryCards());

        public ValueTask<LibraryCard> RetrieveLibraryCardByIdAsync(
            Guid libraryCardId,
            CancellationToken cancellationToken = default) =>
                TryCatch(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateLibraryCardId(libraryCardId);

                    LibraryCard maybeLibraryCard =
                        await this.storageBroker.SelectLibraryCardByIdAsync(libraryCardId, cancellationToken);

                    ValidateStorageLibraryCard(maybeLibraryCard, libraryCardId);

                    return maybeLibraryCard;
                });

        public ValueTask<LibraryCard> ModifyLibraryCardAsync(
            LibraryCard libraryCard,
            CancellationToken cancellationToken = default) =>
                TryCatch(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateLibraryCardOnModify(libraryCard);

                    LibraryCard maybeLibraryCard =
                        await this.storageBroker.SelectLibraryCardByIdAsync(libraryCard.Id, cancellationToken);

                    ValidateStorageLibraryCard(maybeLibraryCard, libraryCard.Id);

                    return await this.storageBroker.UpdateLibraryCardAsync(libraryCard, cancellationToken);
                });

        public ValueTask<LibraryCard> RemoveLibraryCardByIdAsync(
            Guid libraryCardId,
            CancellationToken cancellationToken = default) =>
                TryCatch(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateLibraryCardId(libraryCardId);

                    LibraryCard maybeLibraryCard =
                        await this.storageBroker.SelectLibraryCardByIdAsync(libraryCardId, cancellationToken);

                    ValidateStorageLibraryCard(maybeLibraryCard, libraryCardId);

                    return await this.storageBroker.DeleteLibraryCardAsync(maybeLibraryCard, cancellationToken);
                });
    }
}
