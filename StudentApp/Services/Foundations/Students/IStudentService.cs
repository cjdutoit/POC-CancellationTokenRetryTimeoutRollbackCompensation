// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using StudentApp.Models.Foundations.Students;

namespace StudentApp.Services.Foundations.Students
{
    internal interface IStudentService
    {
        ValueTask<Student> AddStudentAsync(Student student, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<Student>> RetrieveAllStudentsAsync();
        ValueTask<Student> RetrieveStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
        ValueTask<Student> ModifyStudentAsync(Student student, CancellationToken cancellationToken = default);
        ValueTask<Student> RemoveStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    }
}
