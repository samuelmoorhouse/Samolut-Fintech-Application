namespace Samolut_Fintech_Application.Models.DatabaseModels
{
    public class Customer
    {
        public Guid id { get; set; }
        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public int phonenumber { get; set; }

        public string password { get; set; }


    }
}
