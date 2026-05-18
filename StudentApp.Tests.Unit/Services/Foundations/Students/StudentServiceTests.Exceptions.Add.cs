// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Threading;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDuplicateKeyErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
            string someMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(someMessage);

            var alreadyExistsStudentException =
                new AlreadyExistsStudentException(
                    message: "Student already exists, please try again.",
                    innerException: duplicateKeyException);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    testCode: addStudentTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should().BeEquivalentTo(
                expectedStudentDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
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

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: dbUpdateConcurrencyException);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    testCode: addStudentTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should().BeEquivalentTo(
                expectedStudentDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
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

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: dbUpdateException);

            var expectedStudentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(dbUpdateException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    testCode: addStudentTask.AsTask);

            // then
            actualStudentDependencyException.Should().BeEquivalentTo(
                expectedStudentDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
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

            var timeoutStudentException =
                new TimeoutStudentException(
                    message: "Failed student timeout error occurred, contact support.",
                    innerException: timeoutException,
                    data: timeoutException.Data);

            var expectedStudentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: timeoutStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(operationCanceledException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    testCode: addStudentTask.AsTask);

            // then
            actualStudentDependencyException.Should().BeEquivalentTo(
                expectedStudentDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
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
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(operationCanceledException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            // then
            await Assert.ThrowsAsync<OperationCanceledException>(
                testCode: addStudentTask.AsTask);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Student someStudent = CreateRandomStudent();
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
                broker.InsertStudentAsync(someStudent, default))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(someStudent);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    testCode: addStudentTask.AsTask);

            // then
            actualStudentServiceException.Should().BeEquivalentTo(
                expectedStudentServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(someStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
