// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldOrchestrateStudentOnboardingAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student createdStudent = inputStudent.DeepClone();
            Student expectedStudent = createdStudent.DeepClone();

            LibraryCard createdLibraryCard =
                CreateRandomLibraryCard(createdStudent.Id);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdStudent);

            this.libraryCardServiceMock.Setup(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdLibraryCard);

            // when
            Student actualStudent =
                await this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(inputStudent);

            // then
            actualStudent.Should().BeEquivalentTo(expectedStudent);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.libraryCardServiceMock.Verify(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"Student {createdStudent.Id} onboarded with library card {createdLibraryCard.Id}."),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
