// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Linq.Expressions;
using GitFyle.Core.Api.Brokers.Storages;
using Moq;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;
using StudentApp.Services.Foundations.LibraryCards;
using Tynamix.ObjectFiller;
using Xeptions;

namespace StudentApp.Tests.Unit.Services.Foundations.LibraryCards
{
    public partial class LibraryCardServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly LibraryCardService libraryCardService;

        public LibraryCardServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();

            this.libraryCardService = new LibraryCardService(
                storageBroker: this.storageBrokerMock.Object);
        }

        private static Student CreateRandomStudent() =>
            CreateStudentFiller().Create();

        private static Filler<Student> CreateStudentFiller()
        {
            var filler = new Filler<Student>();

            filler.Setup()
                .OnProperty(student => student.Id).Use(Guid.NewGuid())
                .OnProperty(student => student.BirthDate).Use(GetRandomDateTimeOffset());

            return filler;
        }

        private static LibraryCard CreateRandomLibraryCard() =>
            CreateLibraryCardFiller().Create();

        private static Filler<LibraryCard> CreateLibraryCardFiller()
        {
            var filler = new Filler<LibraryCard>();

            filler.Setup()
                .OnProperty(card => card.Id).Use(Guid.NewGuid())
                .OnProperty(card => card.StudentId).Use(Guid.NewGuid())
                .OnProperty(card => card.Status).Use(LibraryCardStatus.Pending)
                .OnProperty(card => card.CreatedDate).Use(GetRandomDateTimeOffset());

            return filler;
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UnixEpoch).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);
    }
}
