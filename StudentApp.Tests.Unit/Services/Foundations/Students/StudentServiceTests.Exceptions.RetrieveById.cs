// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentApp.Models.Foundations.Students;
using StudentApp.Models.Foundations.Students.Exceptions;

namespace StudentApp.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfDbUpdateErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            var dbUpdateException = new DbUpdateException();

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedStudentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<Student> retrieveStudentByIdTask =
                this.studentService.RetrieveStudentByIdAsync(someStudentId);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    testCode: retrieveStudentByIdTask.AsTask);

            // then
            actualStudentDependencyException.Should().BeEquivalentTo(
                expectedStudentDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedStudentServiceException =
                new FailedStudentServiceException(
                    message: "Unexpected service error occurred, contact support.",
                    innerException: serviceException);

            var expectedStudentServiceException =
                new StudentServiceException(
                    message: "Student service error occurred, contact support.",
                    innerException: failedStudentServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> retrieveStudentByIdTask =
                this.studentService.RetrieveStudentByIdAsync(someStudentId);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    testCode: retrieveStudentByIdTask.AsTask);

            // then
            actualStudentServiceException.Should().BeEquivalentTo(
                expectedStudentServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
