// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using EFxceptions.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using StudentApp.Models.Foundations.Students;
using StudentApp.Models.Foundations.Students.Exceptions;
using Xeptions;

namespace StudentApp.Services.Foundations.Students
{
    internal partial class StudentService
    {
        private delegate ValueTask<Student> ReturningStudentFunction();
        private delegate ValueTask<IQueryable<Student>> ReturningStudentsFunction();

        private async ValueTask<Student> TryCatch(ReturningStudentFunction returningStudentFunction)
        {
            try
            {
                return await returningStudentFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var timeoutException = new TimeoutException("The dependency operation timed out.");

                var timeoutStudentException =
                    new TimeoutStudentException(
                        message: "Failed student timeout error occurred, contact support.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyException(timeoutStudentException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (NullStudentException nullStudentException)
            {
                throw CreateAndLogValidationException(nullStudentException);
            }
            catch (InvalidStudentException invalidStudentException)
            {
                throw CreateAndLogValidationException(invalidStudentException);
            }
            catch (NotFoundStudentException notFoundStudentException)
            {
                throw CreateAndLogValidationException(notFoundStudentException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsStudentException =
                    new AlreadyExistsStudentException(
                        message: "Student already exists, please try again.",
                        innerException: duplicateKeyException);

                throw CreateAndLogDependencyValidationException(alreadyExistsStudentException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: dbUpdateConcurrencyException);

                throw CreateAndLogDependencyValidationException(failedStudentStorageException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: dbUpdateException);

                throw await CreateAndLogDependencyException(failedStudentStorageException);
            }
            catch (Exception exception)
            {
                var failedStudentServiceException =
                    new FailedStudentServiceException(
                        message: "Unexpected service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedStudentServiceException);
            }
        }

        private ValueTask<IQueryable<Student>> TryCatch(ReturningStudentsFunction returningStudentsFunction)
        {
            try
            {
                return returningStudentsFunction();
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: dbUpdateConcurrencyException);

                throw new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: dbUpdateException);

                throw new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);
            }
            catch (Exception exception)
            {
                var failedStudentServiceException =
                    new FailedStudentServiceException(
                        message: "Unexpected service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedStudentServiceException);
            }
        }

        private async ValueTask<Student> TryCatchWithRetry(
            ReturningStudentFunction returningStudentFunction,
            Func<ValueTask<Xeption>> createAndLogExceptionAsync)
        {
            try
            {
                return await returningStudentFunction();
            }
            catch
            {
                throw await createAndLogExceptionAsync();
            }
        }

        private async ValueTask<IQueryable<Student>> TryCatchWithRetry(
            ReturningStudentsFunction returningStudentsFunction,
            Func<ValueTask<Xeption>> createAndLogExceptionAsync)
        {
            try
            {
                return await returningStudentsFunction();
            }
            catch
            {
                throw await createAndLogExceptionAsync();
            }
        }

        private StudentValidationException CreateAndLogValidationException(Xeption exception)
        {
            var studentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: exception);

            return studentValidationException;
        }

        private StudentDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var studentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            return studentDependencyValidationException;
        }

        private async ValueTask<StudentDependencyException> CreateAndLogDependencyException(Xeption exception)
        {
            var studentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: exception);

            return studentDependencyException;
        }

        private StudentServiceException CreateAndLogServiceException(Xeption exception)
        {
            var studentServiceException =
                new StudentServiceException(
                    message: "Student service error occurred, contact support.",
                    innerException: exception);

            return studentServiceException;
        }
    }
}
