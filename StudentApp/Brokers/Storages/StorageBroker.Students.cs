// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Brokers.Storages
{
    internal partial class StorageBroker
    {
        public DbSet<Student> Students { get; set; }

        public async ValueTask<Student> InsertStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
                await this.InsertAsync(student, cancellationToken);

        public IQueryable<Student> SelectAllStudents() =>
            this.Set<Student>();

        public async ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
                await this.SelectAsync<Student>(new object[] { studentId }, cancellationToken);

        public async ValueTask<Student> UpdateStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
                await this.UpdateAsync(student, cancellationToken);

        public async ValueTask<Student> DeleteStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
                await this.DeleteAsync(student, cancellationToken);
    }
}
