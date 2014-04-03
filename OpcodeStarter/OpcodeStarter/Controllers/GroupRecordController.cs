using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using OpcodeStarter.Models;
using Newtonsoft.Json;

namespace OpcodeStarter.Controllers
{
    public class GroupsController : ApiController
    {
        // Private variables
        private GroupRecordRepository recordRepo;
        private GroupMembershipRepository membersRepo;
        private string groupsFile = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"GroupRecords.xml");
        private string membershipFile = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"GroupMemberships.xml");

        // Initialize repositories in constructor
        public GroupsController()
        {
            recordRepo = new GroupRecordRepository(groupsFile);
            membersRepo = new GroupMembershipRepository(membershipFile);
        }

        private T DeSerializeFromJson <T>(Stream req)
        {
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            return JsonConvert.DeserializeObject<T>(json);
        }

        // Get request handler for groups
        [System.Web.Http.HttpGet]
        public ActionResult GetRecord(string id)
        {
            ActionResult result;
            GroupRecord record = recordRepo.Get(id);

            if (record == null)
            {
                  throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {
                 // If valid request, return a list of userid strings
                List<string> members = membersRepo.GetListFromMembership(id, false);
                result = new JsonResult
                {
                    Data = members,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return result;
            }
        }


        // Post request handler for groups
        [System.Web.Http.HttpPost]
        public ActionResult AddRecord(string id)
        {
            Stream req = System.Web.HttpContext.Current.Request.InputStream;
            GroupRecord rec;
            try
            {
                rec = DeSerializeFromJson<GroupRecord>(req);
            }
            catch (Exception ex)
            {
                // Try and handle malformed POST body
                 throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (!recordRepo.Add(id))
            {
                 throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // Put request handler for groups
        [System.Web.Http.HttpPut]
        public ActionResult UpdateRecord(string id)
        {
            Stream req = System.Web.HttpContext.Current.Request.InputStream;
            List<string> recs;
            try
            {
                recs = DeSerializeFromJson<List<string>>(req);
            }
            catch (Exception ex)
            {
                // Try and handle malformed POST body
                 throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // If group exists, update it
            if (recordRepo.Get(id) != null)
            {
                membersRepo.RemoveInstancesOfRecord(id, false);

                foreach (string user in recs)
                {
                    membersRepo.AddMembership(id, user);
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // Delete request handler for groups
        [System.Web.Http.HttpDelete]
        public ActionResult DeleteRecord(string id)
        {
            // If group exists, delete all member entries
            if (recordRepo.Get(id) != null)
            {
                membersRepo.RemoveInstancesOfRecord(id, false);
                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
