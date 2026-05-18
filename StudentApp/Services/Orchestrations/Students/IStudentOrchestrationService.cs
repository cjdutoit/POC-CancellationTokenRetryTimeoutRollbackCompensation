// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Services.Orchestrations.Students
{
    internal interface IStudentOrchestrationService
    {
        ValueTask<Student> OrchestrateStudentOnboardingAsync(
            Student student,
            CancellationToken cancellationToken = default);
    }
}
