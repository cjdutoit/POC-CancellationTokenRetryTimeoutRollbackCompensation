// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.LibraryCards.Exceptions;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Services.Foundations.LibraryCards
{
    internal partial class LibraryCardService
    {
        private static void ValidateStudent(Student student)
        {
            ValidateStudentIsNotNull(student);

            Validate(
                (Rule: IsInvalid(student.Id), Parameter: nameof(Student.Id)));
        }

        private static void ValidateLibraryCardOnModify(LibraryCard libraryCard)
        {
            ValidateLibraryCardIsNotNull(libraryCard);

            Validate(
                (Rule: IsInvalid(libraryCard.Id), Parameter: nameof(LibraryCard.Id)),
                (Rule: IsInvalid(libraryCard.StudentId), Parameter: nameof(LibraryCard.StudentId)),
                (Rule: IsInvalid(libraryCard.CreatedDate), Parameter: nameof(LibraryCard.CreatedDate)));
        }

        private static void ValidateLibraryCardId(Guid libraryCardId) =>
            Validate((Rule: IsInvalid(libraryCardId), Parameter: nameof(LibraryCard.Id)));

        private static void ValidateStorageLibraryCard(LibraryCard maybeLibraryCard, Guid libraryCardId)
        {
            if (maybeLibraryCard is null)
            {
                throw new NotFoundLibraryCardException(
                    message: $"Library card not found with id: {libraryCardId}");
            }
        }

        private static void ValidateStudentIsNotNull(Student student)
        {
            if (student is null)
            {
                throw new NullLibraryCardException(message: "Student is null.");
            }
        }

        private static void ValidateLibraryCardIsNotNull(LibraryCard libraryCard)
        {
            if (libraryCard is null)
            {
                throw new NullLibraryCardException(message: "Library card is null.");
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidLibraryCardException =
                new InvalidLibraryCardException(
                    message: "Library card is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidLibraryCardException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidLibraryCardException.ThrowIfContainsErrors();
        }
    }
}
