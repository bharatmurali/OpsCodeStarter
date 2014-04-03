using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Json;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using OpcodeStarter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Helpers;

namespace OpcodeStarter.Controllers
{
    public class UsersController : ApiController
    {
        // Initialize data repositories 
        private UserRecordRepository recordRepo;
        private GroupMembershipRepository membersRepo;
        private string usersFile = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"UserRecords.xml");
        private string membershipFile = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"GroupMemberships.xml");

        // Constructor initializes repositories with desitination files
        public UsersController()
        {
            recordRepo = new UserRecordRepository(usersFile);
            membersRepo = new GroupMembershipRepository(membershipFile);
        }

        // Templated class to return object from json stream
        private T DeSerializeFromJson<T> (Stream req)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;

            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        // Get request for User
        [System.Web.Http.HttpGet]
        public ActionResult GetRecord(string id)
        {
            ActionResult result;

            // This gets basic user data from user repo
            UserRecord record = recordRepo.Get(id);

            if (record == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {
                // If user is valid, get all groups he is part of
                List<string> groups = membersRepo.GetListFromMembership(id, true);
                record.groups = new List<string>(groups);
                //result = JsonConvert.SerializeObject(record);
                result = new JsonResult
                {
                    
                    Data = record,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                };
                return result;
            }
        }

        // Post request for user
        [System.Web.Http.HttpPost]
        public ActionResult PostRecord(string id)
        {
            Stream req = System.Web.HttpContext.Current.Request.InputStream;
            UserRecord rec;
            try
            {
                // Get user record from JSON body
                rec = DeSerializeFromJson<UserRecord>(req);
            }
            catch (Exception ex)
            {
                // Try and handle malformed POST body
                throw new HttpResponseException(HttpStatusCode.BadRequest); 
            }

            // If valid request, add group memberships
            if (rec.id == id && recordRepo.Add(rec))
            {
                if (rec.groups != null)
                {
                    foreach (string group in rec.groups)
                    {
                        membersRepo.AddMembership(group, rec.id);
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }


        // Put request for Users
        [System.Web.Http.HttpPut]
        public ActionResult UpdateRecord(string id)
        {
            Stream req = System.Web.HttpContext.Current.Request.InputStream;
            UserRecord rec;
            try
            {
                rec = DeSerializeFromJson<UserRecord>(req);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (rec.id != id)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            // If valid request, remove all previous membership entries and add new ones
            else if (recordRepo.Update(rec))
            {
                membersRepo.RemoveInstancesOfRecord(rec.id, true);
                foreach (string group in rec.groups)
                {
                    membersRepo.AddMembership(group, rec.id);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }


        // Delete request handler for User
        [System.Web.Http.HttpDelete]
        public ActionResult DeleteRecord(string id)
        {
            if (recordRepo.Remove(id))
            {
                membersRepo.RemoveInstancesOfRecord(id, true);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
