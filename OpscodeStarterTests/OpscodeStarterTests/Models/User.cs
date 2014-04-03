using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpscodeStarterTests.Models
{
    class User
    {
        public string id;
        public string firstName;
        public string lastName;
        public List<string> groups;

        public User(string userId, string fName, string lName)
        {
            id = userId;
            firstName = fName;
            lastName = lName;
        }
        public bool Equals(User p)
        {
            if (p.id == id && p.firstName == firstName && p.lastName == lastName)
                return true;
            else return false;
        }
    }
}
