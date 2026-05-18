// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StudentApp.Models.Foundations.Students;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial interface IStorageBroker
    {
        ValueTask<Student> InsertStudentAsync(Student student, CancellationToken cancellationToken = default);
        IQueryable<Student> SelectAllStudents();
        ValueTask<Student> SelectStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
        ValueTask<Student> UpdateStudentAsync(Student student, CancellationToken cancellationToken = default);
        ValueTask<Student> DeleteStudentAsync(Student student, CancellationToken cancellationToken = default);
    }
}
