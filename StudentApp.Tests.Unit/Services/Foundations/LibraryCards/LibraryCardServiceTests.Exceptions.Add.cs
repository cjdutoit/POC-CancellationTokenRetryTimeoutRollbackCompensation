// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.LibraryCards.Exceptions;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Foundations.LibraryCards
{
    public partial class LibraryCardServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDuplicateKeyErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            string someMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(someMessage);

            var alreadyExistsLibraryCardException =
                new AlreadyExistsLibraryCardException(
                    message: "Library card already exists, please try again.",
                    innerException: duplicateKeyException);

            var expectedLibraryCardDependencyValidationException =
                new LibraryCardDependencyValidationException(
                    message: "Library card dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsLibraryCardException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            LibraryCardDependencyValidationException actualException =
                await Assert.ThrowsAsync<LibraryCardDependencyValidationException>(
                    testCode: addTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedLibraryCardDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDbConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();

            var dbUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var failedLibraryCardStorageException =
                new FailedLibraryCardStorageException(
                    message: "Failed library card storage error occurred, contact support.",
                    innerException: dbUpdateConcurrencyException);

            var expectedLibraryCardDependencyValidationException =
                new LibraryCardDependencyValidationException(
                    message: "Library card dependency validation error occurred, fix the errors and try again.",
                    innerException: failedLibraryCardStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            LibraryCardDependencyValidationException actualException =
                await Assert.ThrowsAsync<LibraryCardDependencyValidationException>(
                    testCode: addTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedLibraryCardDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfDbUpdateErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();

            var dbUpdateException =
                new DbUpdateException();

            var failedLibraryCardStorageException =
                new FailedLibraryCardStorageException(
                    message: "Failed library card storage error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedLibraryCardDependencyException =
                new LibraryCardDependencyException(
                    message: "Library card dependency error occurred, contact support.",
                    innerException: failedLibraryCardStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            LibraryCardDependencyException actualException =
                await Assert.ThrowsAsync<LibraryCardDependencyException>(
                    testCode: addTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedLibraryCardDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddIfOperationCanceledErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();

            var operationCanceledException =
                new OperationCanceledException();

            var timeoutException =
                new TimeoutException(
                    "The dependency operation timed out.");

            var timeoutLibraryCardException =
                new TimeoutLibraryCardException(
                    message: "Failed library card timeout error occurred, contact support.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedLibraryCardDependencyException =
                new LibraryCardDependencyException(
                    message: "Library card dependency error occurred, contact support.",
                    innerException: timeoutLibraryCardException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(operationCanceledException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            LibraryCardDependencyException actualException =
                await Assert.ThrowsAsync<LibraryCardDependencyException>(
                    testCode: addTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedLibraryCardDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledExceptionOnAddIfCanceledAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var operationCanceledException =
                new OperationCanceledException(
                    cancellationTokenSource.Token);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(operationCanceledException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            // then
            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: addTask.AsTask);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            var serviceException = new Exception();

            var failedLibraryCardServiceException =
                new FailedLibraryCardServiceException(
                    message: "Unexpected service error occurred, contact support.",
                    innerException: serviceException);

            var expectedLibraryCardServiceException =
                new LibraryCardServiceException(
                    message: "Library card service error occurred, contact support.",
                    innerException: failedLibraryCardServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<LibraryCard> addTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(someStudent);

            LibraryCardServiceException actualException =
                await Assert.ThrowsAsync<LibraryCardServiceException>(
                    testCode: addTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedLibraryCardServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(It.IsAny<LibraryCard>(), default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
