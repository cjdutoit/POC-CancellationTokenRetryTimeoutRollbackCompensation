// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentApp.Models.Foundations.Students.Exceptions;

namespace StudentApp.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public void ShouldThrowDependencyExceptionOnRetrieveAllIfDbUpdateErrorOccursAndLogIt()
        {
            // given
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
                broker.SelectAllStudents())
                    .Throws(dbUpdateException);

            // when
            System.Action retrieveAllStudentsAction =
                () => this.studentService.RetrieveAllStudentsAsync();

            FluentActions.Invoking(retrieveAllStudentsAction)
                .Should().Throw<StudentDependencyException>()
                .Which.Should().BeEquivalentTo(expectedStudentDependencyException);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllStudents(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogIt()
        {
            // given
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
                broker.SelectAllStudents())
                    .Throws(serviceException);

            // when
            System.Action retrieveAllStudentsAction =
                () => this.studentService.RetrieveAllStudentsAsync();

            FluentActions.Invoking(retrieveAllStudentsAction)
                .Should().Throw<StudentServiceException>()
                .Which.Should().BeEquivalentTo(expectedStudentServiceException);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllStudents(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
