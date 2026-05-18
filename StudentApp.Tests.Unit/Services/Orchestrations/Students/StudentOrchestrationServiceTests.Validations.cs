// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Models.Foundations.Students;
using StudentApp.Models.Orchestrations.Students.Exceptions;

namespace StudentApp.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnOrchestrateIfStudentIsNullAndLogItAsync()
        {
            // given
            Student nullStudent = null;

            var nullStudentOrchestrationException =
                new NullStudentOrchestrationException(
                    message: "Student is null.");

            var expectedStudentOrchestrationValidationException =
                new StudentOrchestrationValidationException(
                    message: "Student orchestration validation error occurred, fix the errors and try again.",
                    innerException: nullStudentOrchestrationException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(nullStudent);

            StudentOrchestrationValidationException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationValidationException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Never);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnOrchestrateIfStudentIdIsEmptyAndLogItAsync()
        {
            // given
            Student invalidStudent = new Student
            {
                Id = Guid.Empty
            };

            var invalidStudentOrchestrationException =
                new InvalidStudentOrchestrationException(
                    message: "Student orchestration is invalid, fix the errors and try again.");

            invalidStudentOrchestrationException.UpsertDataList(
                key: nameof(Student.Id),
                value: "Id is required");

            var expectedStudentOrchestrationValidationException =
                new StudentOrchestrationValidationException(
                    message: "Student orchestration validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentOrchestrationException);

            // when
            ValueTask<Student> orchestrateTask =
                this.studentOrchestrationService.OrchestrateStudentOnboardingAsync(invalidStudent);

            StudentOrchestrationValidationException actualException =
                await Assert.ThrowsAsync<StudentOrchestrationValidationException>(
                    testCode: orchestrateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(
                expectedStudentOrchestrationValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                Times.Never);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.libraryCardServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
