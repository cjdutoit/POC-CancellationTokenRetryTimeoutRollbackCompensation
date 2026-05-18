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
        public async Task ShouldThrowValidationExceptionOnModifyIfStudentIsNullAndLogItAsync()
        {
            // given
            Student nullStudent = null;

            var nullStudentException =
                new NullStudentException(message: "Student is null.");

            var expectedStudentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: nullStudentException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(nullStudent);

            StudentValidationException actualStudentValidationException =
                await Assert.ThrowsAsync<StudentValidationException>(
                    testCode: modifyStudentTask.AsTask);

            // then
            actualStudentValidationException.Should().BeEquivalentTo(
                expectedStudentValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfStudentIsInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            var invalidStudent = new Student
            {
                Id = Guid.Empty,
                UserId = invalidText,
                IdentityNumber = invalidText,
                FirstName = invalidText,
                LastName = invalidText,
                BirthDate = default
            };

            var invalidStudentException =
                new InvalidStudentException(message: "Student is invalid, fix the errors and try again.");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.Id),
                value: "Id is required");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.UserId),
                value: "Text is required");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.IdentityNumber),
                value: "Text is required");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.FirstName),
                value: "Text is required");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.LastName),
                value: "Text is required");

            invalidStudentException.UpsertDataList(
                key: nameof(Student.BirthDate),
                value: "Date is required");

            var expectedStudentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(invalidStudent);

            StudentValidationException actualStudentValidationException =
                await Assert.ThrowsAsync<StudentValidationException>(
                    testCode: modifyStudentTask.AsTask);

            // then
            actualStudentValidationException.Should().BeEquivalentTo(
                expectedStudentValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(It.IsAny<Guid>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStudentIsNotFoundAndLogItAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student noStudent = null;

            var notFoundStudentException =
                new NotFoundStudentException(
                    message: $"Student not found with id: {inputStudent.Id}");

            var expectedStudentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: notFoundStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudent.Id, default))
                    .ReturnsAsync(noStudent);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(inputStudent);

            StudentValidationException actualStudentValidationException =
                await Assert.ThrowsAsync<StudentValidationException>(
                    testCode: modifyStudentTask.AsTask);

            // then
            actualStudentValidationException.Should().BeEquivalentTo(
                expectedStudentValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudent.Id, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateStudentAsync(It.IsAny<Student>(), It.IsAny<System.Threading.CancellationToken>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
