// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Foundations.LibraryCards
{
    public partial class LibraryCardServiceTests
    {
        [Fact]
        public async Task ShouldAddLibraryCardApplicationAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;

            LibraryCard persistedLibraryCard = new LibraryCard
            {
                Id = Guid.NewGuid(),
                StudentId = inputStudent.Id,
                Status = LibraryCardStatus.Pending,
                CreatedDate = DateTimeOffset.UtcNow
            };

            LibraryCard expectedLibraryCard = persistedLibraryCard.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.InsertLibraryCardAsync(
                    It.Is<LibraryCard>(card =>
                        card.StudentId == inputStudent.Id &&
                        card.Status == LibraryCardStatus.Pending),
                    default))
                .ReturnsAsync(persistedLibraryCard);

            // when
            LibraryCard actualLibraryCard =
                await this.libraryCardService.AddLibraryCardApplicationAsync(inputStudent);

            // then
            actualLibraryCard.Should().BeEquivalentTo(expectedLibraryCard);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertLibraryCardAsync(
                    It.Is<LibraryCard>(card =>
                        card.StudentId == inputStudent.Id &&
                        card.Status == LibraryCardStatus.Pending),
                    default),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
