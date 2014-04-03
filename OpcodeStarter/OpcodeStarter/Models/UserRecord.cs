using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace OpcodeStarter.Models
{
    // User Record contains the following information. Groups are not written to disk in xml,
    // but are returned to the client in JSON
    public class UserRecord
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        [XmlIgnore]
        public List<string> groups;

        public UserRecord() { }
    }
}