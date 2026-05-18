namespace StudentApp.Models.Foundations.Students
{
    internal class Student
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
    }
}
