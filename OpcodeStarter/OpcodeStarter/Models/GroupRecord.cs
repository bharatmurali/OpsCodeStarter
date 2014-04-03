using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpcodeStarter.Models
{
    public class GroupRecord
    {
        public string id { get; set; }

        public GroupRecord() { }

        public GroupRecord(string groupId)
        {
            id = groupId;
        }
    }
}