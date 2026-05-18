// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Linq.Expressions;
using Moq;
using StudentApp.Brokers.Loggings;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;
using StudentApp.Services.Foundations.LibraryCards;
using StudentApp.Services.Foundations.Students;
using StudentApp.Services.Orchestrations.Students;
using Tynamix.ObjectFiller;
using Xeptions;

namespace StudentApp.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        private readonly Mock<IStudentService> studentServiceMock;
        private readonly Mock<ILibraryCardService> libraryCardServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly StudentOrchestrationService studentOrchestrationService;

        public StudentOrchestrationServiceTests()
        {
            this.studentServiceMock = new Mock<IStudentService>();
            this.libraryCardServiceMock = new Mock<ILibraryCardService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.studentOrchestrationService = new StudentOrchestrationService(
                studentService: this.studentServiceMock.Object,
                libraryCardService: this.libraryCardServiceMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
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

        private static LibraryCard CreateRandomLibraryCard(Guid studentId) =>
            CreateLibraryCardFiller(studentId).Create();

        private static Filler<LibraryCard> CreateLibraryCardFiller(Guid studentId)
        {
            var filler = new Filler<LibraryCard>();

            filler.Setup()
                .OnProperty(card => card.Id).Use(Guid.NewGuid())
                .OnProperty(card => card.StudentId).Use(studentId)
                .OnProperty(card => card.Status).Use(LibraryCardStatus.Pending)
                .OnProperty(card => card.CreatedDate).Use(GetRandomDateTimeOffset());

            return filler;
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UnixEpoch).GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);
    }
}
