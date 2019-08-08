using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Eyedeas.PowerApps.Portals.WebApi.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eyedeas.PowerApps.Portals.WebApi.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetOperation()
        {

            Dynamics365WebapiWrapper webapiWrapper =
    new Dynamics365WebapiWrapper("https://org5b364ce1.api.crm.dynamics.com/api/data/v9.1/",
    "https://org5b364ce1.crm.dynamics.com", "eyedeasinc.com",
    "0973bcef-78db-4353-9ee0-64deddfff244",
    ":W=vV9Scu0y]5Gb4JSgshDaR]av7Sjsy", "http://localhost");

            //    webApiConfiguration.RedirectUrl);

            var response = webapiWrapper.Get("contacts");

            Assert.AreEqual((int)response.StatusCode, 200);
        }

        [TestMethod]
        public void TestPostOperation()
        {
           
            Dynamics365WebapiWrapper webapiWrapper =
                new Dynamics365WebapiWrapper("https://org5b364ce1.api.crm.dynamics.com/api/data/v9.1/",
                "https://org5b364ce1.crm.dynamics.com", "eyedeasinc.com",
                "0973bcef-78db-4353-9ee0-64deddfff244",
                ":W=vV9Scu0y]5Gb4JSgshDaR]av7Sjsy", "http://localhost");

            var body = @"{
    'name': 'Unit Test Account " + Guid.NewGuid().ToString() + @"',
    'creditonhold': false,
    'address1_latitude': 47.639583,
    'description': 'This is the description of the sample account',
    'revenue': 5000000,
    'accountcategorycode': 1
}";

            var response = webapiWrapper.Post("accounts", body);

            Assert.AreEqual((int)response.StatusCode, 204);
        }

        [TestMethod]
        public void TestPatchOperation()
        {

            Dynamics365WebapiWrapper webapiWrapper =
                new Dynamics365WebapiWrapper("https://org5b364ce1.api.crm.dynamics.com/api/data/v9.1/",
                "https://org5b364ce1.crm.dynamics.com", "eyedeasinc.com",
                "0973bcef-78db-4353-9ee0-64deddfff244",
                ":W=vV9Scu0y]5Gb4JSgshDaR]av7Sjsy", "http://localhost");

            var body = @"{'name': 'Unit Test Account " + DateTime.Now.ToString() + @"'}";

            var response = webapiWrapper.Patch("accounts(81965274-70ab-e911-a821-000d3a3b10ec)", body);

            Assert.AreEqual((int)response.StatusCode, 204);
        }


        [TestMethod]
        public void TestDeleteOperation()
        {

            Dynamics365WebapiWrapper webapiWrapper =
                new Dynamics365WebapiWrapper("https://org5b364ce1.api.crm.dynamics.com/api/data/v9.1/",
                "https://org5b364ce1.crm.dynamics.com", "eyedeasinc.com",
                "0973bcef-78db-4353-9ee0-64deddfff244",
                ":W=vV9Scu0y]5Gb4JSgshDaR]av7Sjsy", "http://localhost");


            var body = @"{
    'name': 'Unit Test Account " + Guid.NewGuid().ToString() + @"',
    'creditonhold': false,
    'address1_latitude': 47.639583,
    'description': 'This is the description of the sample account',
    'revenue': 5000000,
    'accountcategorycode': 1
}";

            //Create Sample Account
            var response = webapiWrapper.Post("accounts", body);

            Assert.AreEqual((int)response.StatusCode, 204);
            //Parse out the Id of the newly created record.
            var entityUri = response.Headers.GetValues("OData-EntityId").ToList()[0];
            string pattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
            Match m = Regex.Match(entityUri, pattern, RegexOptions.IgnoreCase);
            //update record id
            if (m.Success)
            {
                var accountId = m.Value;
                //Delete Recently create account
                response = webapiWrapper.Delete($"accounts({accountId})");
            }

            Assert.AreEqual((int)response.StatusCode, 204);
        }
    }
}
