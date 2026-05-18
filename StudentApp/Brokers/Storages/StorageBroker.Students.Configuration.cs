// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Brokers.Storages
{
    internal partial class StorageBroker
    {
        private static void AddStudentConfigurations(EntityTypeBuilder<Student> studentBuilder)
        {
            studentBuilder.ToTable("Students");

            studentBuilder.HasKey(student => student.Id);

            studentBuilder.Property(student => student.Id)
                .IsRequired();

            studentBuilder.Property(student => student.UserId)
                .IsRequired();

            studentBuilder.Property(student => student.IdentityNumber)
                .IsRequired();

            studentBuilder.Property(student => student.FirstName)
                .IsRequired();

            studentBuilder.Property(student => student.MiddleName);

            studentBuilder.Property(student => student.LastName)
                .IsRequired();

            studentBuilder.Property(student => student.BirthDate)
                .IsRequired();
        }
    }
}
