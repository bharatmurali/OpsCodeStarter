using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpcodeStarter.Models
{
    public class UserRecordRepository
    {
        private File file;
        private List<UserRecord> users;

         // Constructor
        public UserRecordRepository(string fileName)
        {
            // Instantiates a file to write to
            file = new File (fileName);
            users = new List<UserRecord>();
        }

        // Clears the cache and updates it with latest from file
        private void FetchFromFile()
        {
            users.Clear();
            file.GetObjectsFromFile(ref users);
        }

        // Returns file entry from cache
        public UserRecord Get(string userId)
        {
            FetchFromFile();

            foreach (UserRecord userRec in users)
            {
                if (userRec.id == userId)
                {
                    return userRec;
                }
            }
            return null;
        }

        // Adds a file entry to the cache if it does not already exist and writes to the file
        public bool Add(UserRecord item)
        {
            FetchFromFile();

            if (this.Get(item.id) != null)
                return false;

            users.Add(item);
            file.WriteObjectToFile(users);

            return true;
        }

        // Deletes a file entry from the cache and writes it to disk
        public bool Remove(string userid)
        {
            FetchFromFile();

            foreach (UserRecord user in users)
            {
                if (user.id == userid)
                {
                    users.Remove(user);
                    file.WriteObjectToFile(users);
                    return true;
                }
            }

            return false;
        }
        
        // Updates existing entry and cache and writes it to disk
        public bool Update(UserRecord item)
        {
            if (!this.Remove(item.id)) return false;

            this.Add(item);

            return true;
        }
    }
}