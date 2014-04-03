using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpcodeStarter.Models
{
    // This class maps a group to a user
    public class GroupMembership
    {
        public string _groupId { get; set; }
        public string _userId { get; set; }

        public GroupMembership() { }

        public GroupMembership(string groupId, string userId)
        {
            _groupId = groupId;
            _userId = userId;
        }
    }
}