// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using EFxceptions;
using GitFyle.Core.Api.Brokers.Storages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentApp.Models.Foundations.Students;

namespace StudentApp.Brokers.Storages
{
    internal partial class StorageBroker : EFxceptionsContext, IStorageBroker
    {
        private readonly IConfiguration configuration;

        public StorageBroker(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.Database.Migrate();
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            string connectionString = this.configuration
                .GetConnectionString(name: "DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            AddStudentConfigurations(modelBuilder.Entity<Student>());
        }

        private async ValueTask<T> InsertAsync<T>(T @object, CancellationToken cancellationToken = default)
        {
            this.Entry(@object).State = EntityState.Added;
            await this.SaveChangesAsync(cancellationToken);
            DetachEntity(@object);

            return @object;
        }

        public async ValueTask<IQueryable<T>> SelectAllAsync<T>() where T : class =>
            this.Set<T>();

        public async ValueTask<T> SelectAsync<T>(object[] objectIds, CancellationToken cancellationToken = default)
            where T : class =>
                await this.FindAsync<T>(objectIds, cancellationToken);

        private async ValueTask<T> UpdateAsync<T>(T @object, CancellationToken cancellationToken = default)
        {
            this.Entry(@object).State = EntityState.Modified;
            await this.SaveChangesAsync(cancellationToken);
            DetachEntity(@object);

            return @object;
        }

        private async ValueTask<T> DeleteAsync<T>(T @object, CancellationToken cancellationToken = default)
        {
            this.Entry(@object).State = EntityState.Deleted;
            await this.SaveChangesAsync(cancellationToken);
            DetachEntity(@object);

            return @object;
        }

        public async ValueTask BulkInsertAsync<T>(IEnumerable<T> objects, CancellationToken cancellationToken = default)
            where T : class =>
        await this.AddRangeAsync(objects, cancellationToken);

        public async ValueTask BulkUpdateAsync<T>(IEnumerable<T> objects, CancellationToken cancellationToken = default)
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.UpdateRange(objects);
        }

        public async ValueTask BulkDeleteAsync<T>(IEnumerable<T> objects, CancellationToken cancellationToken = default)
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.RemoveRange(objects);
        }

        private void DetachEntity<T>(T @object)
        {
            this.Entry(@object).State = EntityState.Detached;
        }
    }
}