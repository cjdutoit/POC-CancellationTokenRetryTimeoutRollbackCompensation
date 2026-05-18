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
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveIfDbConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            Student someStudent = CreateRandomStudent();
            someStudent.Id = someStudentId;

            var dbUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: dbUpdateConcurrencyException);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default))
                    .ReturnsAsync(someStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteStudentAsync(someStudent, default))
                    .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    testCode: removeStudentByIdTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should().BeEquivalentTo(
                expectedStudentDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(someStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRemoveIfDbUpdateErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            Student someStudent = CreateRandomStudent();
            someStudent.Id = someStudentId;

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
                    .ReturnsAsync(someStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteStudentAsync(someStudent, default))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    testCode: removeStudentByIdTask.AsTask);

            // then
            actualStudentDependencyException.Should().BeEquivalentTo(
                expectedStudentDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(someStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            Student someStudent = CreateRandomStudent();
            someStudent.Id = someStudentId;

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
                    .ReturnsAsync(someStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteStudentAsync(someStudent, default))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    testCode: removeStudentByIdTask.AsTask);

            // then
            actualStudentServiceException.Should().BeEquivalentTo(
                expectedStudentServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(someStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
