using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using OpscodeStarterTests.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpscodeStarterTests
{
    [TestClass]
    public class OpsCodeStarterTests
    {
        private string _usersEndpoint = "api/users/";
        private string _groupsEndpoint = "api/groups/";
        private string _baseAddress = "http://localhost:60081/";

        // Returns a random string
        private string getString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars); ;
        }

        // Sets up the client with the base address and media type
        private void ClientSetup (ref HttpClient client)
        {
            client.BaseAddress = new Uri(_baseAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Sends an object to the server in json and returns the response
        private Task<HttpResponseMessage> SendRequest <T>(HttpClient client, T obj, string endpoint, bool post)
        {
            string json = JsonConvert.SerializeObject(obj);
            StringContent sContent = new StringContent(json);
            if (post)
                return client.PostAsync(endpoint, sContent);
            else
                return client.PutAsync(endpoint, sContent);
        }

        private T GetObjectFromResponse<T> (Task<HttpResponseMessage> response)
        {
            Task<string> userResponse = response.Result.Content.ReadAsStringAsync();
            string resp = userResponse.Result;
            JsonResult result = JsonConvert.DeserializeObject<JsonResult>(resp);
            return JsonConvert.DeserializeObject<T>(result.Data.ToString());
        }

        [TestMethod]
        [Description("This test creates and then gets a user")]
        public void BasicUserCreateAndGetTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string userId = getString();
                User testUser = new User(userId, getString(), getString());

                testUser.groups = new List<string>();

                string endpoint = _usersEndpoint + userId;
                Task<HttpResponseMessage> response = SendRequest(client, testUser, endpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(endpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                User returnedUser = GetObjectFromResponse<User>(response);
                Assert.IsTrue(returnedUser.Equals(testUser));
            }
        }

        [TestMethod]
        [Description("This test creates and then gets a group")]
        public void BasicGroupCreateAndGetTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string endpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(endpoint, null);

                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(endpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                List<string> returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 0);
            }
        }

        [TestMethod]
        [Description("This test tries to get an invalid group")]
        public void GetInvalidGroupTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string endpoint = _groupsEndpoint + "invalid";
                Task<HttpResponseMessage> response = client.GetAsync(endpoint);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.NotFound));
            }
        }

        [TestMethod]
        [Description("This test exercises the basic group membership scenario")]
        public void BasicGroupMembershipTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                string userId = getString();
                string userEndpoint = _usersEndpoint + userId;
                User testUser = new User(userId, getString(), getString());
                testUser.groups = new List<string>();
                testUser.groups.Add(groupId);

                response = SendRequest(client, testUser, userEndpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(userEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                User returnedUser = GetObjectFromResponse<User>(response);
                
                Assert.IsTrue(returnedUser.Equals(testUser));
                Assert.IsTrue(returnedUser.groups.Count == 1);
                Assert.IsTrue(returnedUser.groups[0].Equals(groupId));
            }
        }

        [TestMethod]
        [Description("This test tries to create an already existing user")]
        public void CreateExistingUserTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string userId = getString();
                User testUser = new User(userId, getString(), getString());

                testUser.groups = new List<string>();

                string endpoint = _usersEndpoint + userId;
                Task<HttpResponseMessage> response = SendRequest(client, testUser, endpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = SendRequest(client, testUser, endpoint, true);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.BadRequest));
            }
        }

        [TestMethod]
        [Description("This test  tries to create an already existing group")]
        public void CreateExistingGroupTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string endpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(endpoint, null);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.PostAsync(endpoint, null);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.BadRequest));
            }
        }

        [TestMethod]
        [Description("This test exercises the basic user update success scenario")]
        public void BasicUserUpdateTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string userId = getString();
                User testUser = new User(userId, getString(), getString());

                testUser.groups = new List<string>();

                string endpoint = _usersEndpoint + userId;
                Task<HttpResponseMessage> response = SendRequest(client, testUser, endpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                testUser.firstName = "random";

                response = SendRequest(client, testUser, endpoint, false);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(endpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                User returnedUser = GetObjectFromResponse<User>(response);
                Assert.IsTrue(returnedUser.Equals(testUser));
            }
        }

        [TestMethod]
        [Description("This test tries to update the ID of a user")]
        public void UserUpdateWithInvalidIdTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string userId = getString();
                User testUser = new User(userId, getString(), getString());

                testUser.groups = new List<string>();

                string endpoint = _usersEndpoint + userId;
                Task<HttpResponseMessage> response = SendRequest(client, testUser, endpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                testUser.id = "invalid";
                response = SendRequest(client, testUser, endpoint, false);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.BadRequest));
            }
        }

        [TestMethod]
        [Description("This test exercises the basic group update success scenario")]
        public void BasicUpdateGroupTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                string userId = getString();
                string userEndpoint = _usersEndpoint + userId;
                User testUser = new User(userId, getString(), getString());
                testUser.groups = new List<string>();
                testUser.groups.Add(groupId);

                response = SendRequest(client, testUser, userEndpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(groupEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);
                List<string> returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 1);

                response = SendRequest(client, new List<string>(), groupEndpoint, false);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(groupEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);
                returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 0);
            }
        }

        [TestMethod]
        [Description("This test exercises the basic group delete success scenario")]
        public void BasicGroupDeleteTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                string userId = getString();
                string userEndpoint = _usersEndpoint + userId;
                User testUser = new User(userId, getString(), getString());
                testUser.groups = new List<string>();
                testUser.groups.Add(groupId);

                response = SendRequest(client, testUser, userEndpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(groupEndpoint);

                List<string> returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 1);

                response = client.DeleteAsync(groupEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(groupEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);
                returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 0);
            }
        }

        [TestMethod]
        [Description("This test exercises the scenario where a group update is attempted with a list of User objects")]
        public void GroupUpdateWithUserRecordTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                string userId = getString();
                User testUser = new User(userId, getString(), getString());
                List<User> groupMembers = new List<User>();
                groupMembers.Add(testUser);
                response = SendRequest<List<User>>(client, groupMembers, groupEndpoint, false);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.BadRequest));
            }
        }

        [TestMethod]
        [Description("This test exercises the basic user delete success scenario")]
        public void BasicUserDeleteTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                string userId = getString();
                User testUser = new User(userId, getString(), getString());
                testUser.groups = new List<string>();
                testUser.groups.Add(groupId);

                string userEndpoint = _usersEndpoint + userId;
                response = SendRequest(client, testUser, userEndpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.DeleteAsync(userEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(userEndpoint);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.NotFound));

                response = client.GetAsync(groupEndpoint);
                List<string> members = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(members.Count == 0);
            }
        }

        [TestMethod]
        [Description("This test ensure the delete operations work correctly when a user and group have the same ID")]
        public void DeleteWithSameUserAndGroupNameTest()
        {
            var client = new HttpClient();
            ClientSetup(ref client);

            using (client)
            {
                string groupId = getString();
                string groupEndpoint = _groupsEndpoint + groupId;
                Task<HttpResponseMessage> response = client.PostAsync(groupEndpoint, null);

                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                string userId = groupId;
                User testUser = new User(userId, getString(), getString());
                testUser.groups = new List<string>();
                testUser.groups.Add(groupId);

                string userEndpoint = _usersEndpoint + userId;
                response = SendRequest(client, testUser, userEndpoint, true);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.DeleteAsync(userEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);

                response = client.GetAsync(userEndpoint);
                Assert.IsTrue(response.Result.StatusCode.Equals(HttpStatusCode.NotFound));

                response = client.GetAsync(groupEndpoint);
                Assert.IsTrue(response.Result.IsSuccessStatusCode);
                List<string> returnedGroups = GetObjectFromResponse<List<string>>(response);
                Assert.IsTrue(returnedGroups.Count == 0);
            }
        }
    }
}
