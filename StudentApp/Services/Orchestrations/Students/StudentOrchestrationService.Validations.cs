// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using StudentApp.Models.Foundations.Students;
using StudentApp.Models.Orchestrations.Students.Exceptions;

namespace StudentApp.Services.Orchestrations.Students
{
    internal partial class StudentOrchestrationService
    {
        private static void ValidateStudent(Student student)
        {
            ValidateStudentIsNotNull(student);

            Validate(
                (Rule: IsInvalid(student.Id), Parameter: nameof(Student.Id)));
        }

        private static void ValidateStudentIsNotNull(Student student)
        {
            if (student is null)
            {
                throw new NullStudentOrchestrationException(
                    message: "Student is null.");
            }
        }

        private static dynamic IsInvalid(System.Guid id) => new
        {
            Condition = id == System.Guid.Empty,
            Message = "Id is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidStudentOrchestrationException =
                new InvalidStudentOrchestrationException(
                    message: "Student orchestration is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidStudentOrchestrationException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidStudentOrchestrationException.ThrowIfContainsErrors();
        }
    }
}
