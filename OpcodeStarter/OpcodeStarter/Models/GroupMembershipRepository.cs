using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpcodeStarter.Models
{
    // This class is the repository to store all data mapping a group to a user
    public class GroupMembershipRepository
    {
        private File file;
        private List<GroupMembership> groupMemberships;

        // Controller inits new File object and local cache
        public GroupMembershipRepository(string fileName)
        {
            file = new File (fileName);
            groupMemberships = new List<GroupMembership>();
        }

        // Clears the local cache and updates it with data from disk
        private void FetchFromFile()
        {
            groupMemberships.Clear();
            file.GetObjectsFromFile(ref groupMemberships);
        }

        // Returns a list of members/groups depending on getUsers param
        public List<string> GetListFromMembership(string id, bool getUsers)                                                                                                                                                          
        {
            FetchFromFile();
            List<string> ids = new List<string>();

            foreach (GroupMembership membership in groupMemberships)
            {
                // If this is set, return a set of groups that map to this userid
                if (!getUsers && membership._groupId == id)
                {
                    ids.Add(membership._userId);
                }
                // If this is set, return a set of users that map to this groupid
                else if (getUsers && membership._userId == id)
                {
                    ids.Add(membership._groupId);
                }
            }

            return ids;
        }

        // Removes all mappings that correspond to a certain id depending on idIsUser param
        public bool RemoveInstancesOfRecord(string id, bool idIsUser)
        {
            FetchFromFile();

            // If the id is a user, delete all mapping with this userid set
            if (idIsUser)
            {
                groupMemberships.RemoveAll(record => record._userId == id);
            }
            // If the id is a group, delete all mappings with this groupid set
            else if (!idIsUser)
            {
                groupMemberships.RemoveAll(record => record._groupId == id);
            }

            file.WriteObjectToFile(groupMemberships);

            return true;   
        }

        // Adds a new mapping between a group and a user
        public void AddMembership(string groupId, string userId)
        {
            FetchFromFile();

            foreach (GroupMembership membership in groupMemberships)
            {
                if (membership._groupId == groupId && membership._userId == userId)
                {
                    return;
                }
            }

            GroupMembership newMember = new GroupMembership(groupId, userId);
            groupMemberships.Add(newMember);

            file.WriteObjectToFile(groupMemberships);

            return;
        }
    }
}