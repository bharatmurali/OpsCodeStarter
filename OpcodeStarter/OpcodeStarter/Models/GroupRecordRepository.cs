using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpcodeStarter.Models
{
    // Delete and Remove methods do not exist here because a group cannot be deleted.
    // Updating a group involves changing its members, which does not require any changes to the group repo
    public class GroupRecordRepository
    {
        private File file;
        private List<GroupRecord> groups;

        // Constructor inits a File instance and a local groups cache
        public GroupRecordRepository(string fileName)
        {
            file = new File (fileName);
            groups = new List<GroupRecord>();
        }

        // Clears the local cache and refreshes it with data from disk
        private void FetchFromFile()
        {
            groups.Clear();
            file.GetObjectsFromFile(ref groups);
        }

        // Adds a new group entry to the cache and writes it to disk
        public bool Add (string id)
        {
            if (this.Get(id) != null)
                return false;

            GroupRecord addRec = new GroupRecord(id);
            groups.Add(addRec);
            
            file.WriteObjectToFile(groups);

            return true;
        }

        // Gets a group record
        public GroupRecord Get(string id)
        {
            FetchFromFile();

            foreach (GroupRecord group in groups)
            {
                if (group.id == id)
                    return group;
            }

            return null;
        }
    }
}