// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using StudentApp.Brokers.Loggings;
using StudentApp.Models.Foundations.LibraryCards;
using StudentApp.Models.Foundations.Students;
using StudentApp.Services.Foundations.LibraryCards;
using StudentApp.Services.Foundations.Students;

namespace StudentApp.Services.Orchestrations.Students
{
    internal partial class StudentOrchestrationService : IStudentOrchestrationService
    {
        private readonly IStudentService studentService;
        private readonly ILibraryCardService libraryCardService;
        private readonly ILoggingBroker loggingBroker;

        public StudentOrchestrationService(
            IStudentService studentService,
            ILibraryCardService libraryCardService,
            ILoggingBroker loggingBroker)
        {
            this.studentService = studentService;
            this.libraryCardService = libraryCardService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Student> OrchestrateStudentOnboardingAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
                TryCatch(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateStudent(student);

                    Student createdStudent = null;
                    LibraryCard createdLibraryCard = null;

                    return await TryCatchWithCompensationAsync(
                        returningStudentFunction: async () =>
                        {
                            createdStudent =
                                await this.studentService.AddStudentAsync(student, cancellationToken);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                this.loggingBroker.LogWarning(
                                    $"Student '{createdStudent.Id}' was created but cancellation was " +
                                    $"requested. Library card application will NOT be submitted.");

                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            createdLibraryCard =
                                await this.libraryCardService.AddLibraryCardApplicationAsync(
                                    createdStudent, cancellationToken);

                            this.loggingBroker.LogInformation(
                                $"Student {createdStudent.Id} onboarded with library card {createdLibraryCard.Id}.");

                            return createdStudent;
                        },
                        async () => await HandleCompensation(createdStudent, createdLibraryCard, CancellationToken.None));
                });

        private async Task HandleCompensation(Student? createdStudent, LibraryCard? createdLibraryCard, CancellationToken none)
        {
            if (createdLibraryCard is not null)
            {
                await this.libraryCardService.RemoveLibraryCardByIdAsync(
                    createdLibraryCard.Id,
                    CancellationToken.None);
            }

            if (createdStudent is not null)
            {
                await this.studentService.RemoveStudentByIdAsync(
                    createdStudent.Id,
                    CancellationToken.None);
            }
        }
    }
}
