// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldModifyStudentAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student storageStudent = inputStudent.DeepClone();
            Student updatedStudent = inputStudent.DeepClone();
            Student expectedStudent = updatedStudent.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudent.Id, default))
                    .ReturnsAsync(storageStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateStudentAsync(inputStudent, default))
                    .ReturnsAsync(updatedStudent);

            // when
            Student actualStudent =
                await this.studentService.ModifyStudentAsync(inputStudent);

            // then
            actualStudent.Should().BeEquivalentTo(expectedStudent);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudent.Id, default),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateStudentAsync(inputStudent, default),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
