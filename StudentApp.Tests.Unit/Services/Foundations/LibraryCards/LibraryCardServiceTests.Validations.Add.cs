// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.LibraryCards.Exceptions;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Foundations.LibraryCards
{
    public partial class LibraryCardServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfStudentIsNullAndLogItAsync()
        {
            // given
            Student nullStudent = null;

            var nullLibraryCardException =
                new NullLibraryCardException(message: "Student is null.");

            var expectedLibraryCardValidationException =
                new LibraryCardValidationException(
                    message: "Library card validation error occurred, fix the errors and try again.",
                    innerException: nullLibraryCardException);

            // when
            ValueTask<LibraryCard> addLibraryCardTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(nullStudent);

            LibraryCardValidationException actualLibraryCardValidationException =
                await Assert.ThrowsAsync<LibraryCardValidationException>(
                    testCode: addLibraryCardTask.AsTask);

            // then
            actualLibraryCardValidationException.Should().BeEquivalentTo(
                expectedLibraryCardValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(
                    It.IsAny<LibraryCard>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfStudentIdIsEmptyAndLogItAsync()
        {
            // given
            Student invalidStudent = new Student
            {
                Id = Guid.Empty
            };

            var invalidLibraryCardException =
                new InvalidLibraryCardException(
                    message: "Library card is invalid, fix the errors and try again.");

            invalidLibraryCardException.UpsertDataList(
                key: nameof(Student.Id),
                value: "Id is required");

            var expectedLibraryCardValidationException =
                new LibraryCardValidationException(
                    message: "Library card validation error occurred, fix the errors and try again.",
                    innerException: invalidLibraryCardException);

            // when
            ValueTask<LibraryCard> addLibraryCardTask =
                this.libraryCardService.AddLibraryCardApplicationAsync(invalidStudent);

            LibraryCardValidationException actualLibraryCardValidationException =
                await Assert.ThrowsAsync<LibraryCardValidationException>(
                    testCode: addLibraryCardTask.AsTask);

            // then
            actualLibraryCardValidationException.Should().BeEquivalentTo(
                expectedLibraryCardValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(
                    It.IsAny<LibraryCard>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
