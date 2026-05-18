// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveAllStudents()
        {
            // given
            IQueryable<Student> randomStudents =
                new[] { CreateRandomStudent() }.AsQueryable();

            IQueryable<Student> storageStudents = randomStudents;
            IQueryable<Student> expectedStudents = storageStudents;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllStudents())
                    .Returns(storageStudents);

            // when
            IQueryable<Student> actualStudents =
                await this.studentService.RetrieveAllStudentsAsync();

            // then
            actualStudents.Should().BeEquivalentTo(expectedStudents);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllStudents(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
