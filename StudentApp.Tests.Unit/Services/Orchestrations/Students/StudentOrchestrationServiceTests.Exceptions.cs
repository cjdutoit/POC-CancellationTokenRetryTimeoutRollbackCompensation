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
using StudentApp.Models.Foundations.Students.Exceptions;
using StudentApp.Models.Orchestrations.Students.Exceptions;

namespace StudentApp.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnOrchestrateIfStudentValidationErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();

            var invalidStudentException =
                new InvalidStudentException(
                    message: "Student is invalid, fix the errors and try again.");

            var studentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: studentValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentValidationException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            StudentOrchestrationDependencyValidationException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnOrchestrateIfLibraryCardValidationErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            Student createdStudent = someStudent;

            var invalidLibraryCardException =
                new InvalidLibraryCardException(
                    message: "Library card is invalid, fix the errors and try again.");

            var libraryCardValidationException =
                new LibraryCardValidationException(
                    message: "Library card validation error occurred, fix the errors and try again.",
                    innerException: invalidLibraryCardException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: libraryCardValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdStudent);

            this.libraryCardServiceMock.Setup(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(libraryCardValidationException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            StudentOrchestrationDependencyValidationException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.libraryCardServiceMock.Verify(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnOrchestrateIfStudentDependencyErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();

            var studentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: new Xeptions.Xeption("inner"));

            var expectedStudentOrchestrationDependencyException =
                new StudentOrchestrationDependencyException(
                    message: "Student orchestration dependency error occurred, contact support.",
                    innerException: studentDependencyException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentDependencyException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            StudentOrchestrationDependencyException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationDependencyException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnOrchestrateIfLibraryCardDependencyErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            Student createdStudent = someStudent;

            var libraryCardDependencyException =
                new LibraryCardDependencyException(
                    message: "Library card dependency error occurred, contact support.",
                    innerException: new Xeptions.Xeption("inner"));

            var expectedStudentOrchestrationDependencyException =
                new StudentOrchestrationDependencyException(
                    message: "Student orchestration dependency error occurred, contact support.",
                    innerException: libraryCardDependencyException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdStudent);

            this.libraryCardServiceMock.Setup(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(libraryCardDependencyException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            StudentOrchestrationDependencyException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationDependencyException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.libraryCardServiceMock.Verify(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledExceptionOnOrchestrateAndCompensateIfCancelledAfterStudentCreatedAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            Student createdStudent = someStudent;

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var operationCanceledException = new OperationCanceledException(cancellationTokenSource.Token);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdStudent);

            this.libraryCardServiceMock.Setup(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            this.studentServiceMock.Setup(service =>
                service.RemoveStudentByIdAsync(createdStudent.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdStudent);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            // then
            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: orchestrateTask.AsTask);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.libraryCardServiceMock.Verify(service =>
                service.AddLibraryCardApplicationAsync(createdStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.Verify(service =>
                service.RemoveStudentByIdAsync(createdStudent.Id, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnOrchestrateIfUnexpectedErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            var unexpectedException = new Exception();

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(unexpectedException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(someStudent);

            StudentOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationServiceException>(
                    testCode: orchestrateTask.AsTask);

            // then
            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(someStudent, It.IsAny<CancellationToken>()),
                Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
