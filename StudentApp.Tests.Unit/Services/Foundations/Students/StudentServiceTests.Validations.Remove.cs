// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Models.Foundations.Students;
using StudentApp.Models.Foundations.Students.Exceptions;

namespace StudentApp.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveIfStudentIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidStudentId = Guid.Empty;

            var invalidStudentException =
                new InvalidStudentException(message: "Student is invalid, fix the errors and try again.");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.Id),
                value: "Id is required");

            var expectedStudentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(invalidStudentId);

            StudentValidationException actualStudentValidationException =
                await Assert.ThrowsAsync<StudentValidationException>(
                    testCode: removeStudentByIdTask.AsTask);

            // then
            actualStudentValidationException.Should().BeEquivalentTo(
                expectedStudentValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveIfStudentIsNotFoundAndLogItAsync()
        {
            // given
            Guid someStudentId = Guid.NewGuid();
            Student noStudent = null;

            var notFoundStudentException =
                new NotFoundStudentException(
                    message: $"Student not found with id: {someStudentId}");

            var expectedStudentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: notFoundStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default))
                    .ReturnsAsync(noStudent);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(someStudentId);

            StudentValidationException actualStudentValidationException =
                await Assert.ThrowsAsync<StudentValidationException>(
                    testCode: removeStudentByIdTask.AsTask);

            // then
            actualStudentValidationException.Should().BeEquivalentTo(
                expectedStudentValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(someStudentId, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
