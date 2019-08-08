using Eyedeas.PowerApps.Portals.WebApi.Utils;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Eyedeas.PowerApps.Portals.WebApi
{
    public class ExecuteWebAPIOperation : PluginBase
    {
        private string _secureConfig = null;
        private string _unsecureConfig = null;

        public ExecuteWebAPIOperation(string unsecure, string secureConfig)
            : base(typeof(ExecuteWebAPIOperation))
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecure;
        }

        protected override void ExecuteCrmPlugin(LocalPluginContext localContext)
        {
            if (localContext == null)
                throw new ArgumentNullException(nameof(localContext));

            localContext.Trace("Invoked Plugin Execution");

            if (localContext.PluginExecutionContext.PrimaryEntityName != "eyedeas_webapioperation")
            {
                localContext.Trace($"Invalid Primary Entity {localContext.PluginExecutionContext.PrimaryEntityName}");
                return;
            }

            localContext.Trace($"PreImage count: {localContext.PluginExecutionContext.PreEntityImages.Count}");
            localContext.Trace($"PostImage count: {localContext.PluginExecutionContext.PostEntityImages.Count}");

            if (localContext.PluginExecutionContext.InputParameters.Contains("Target") && localContext.PluginExecutionContext.InputParameters["Target"] is Entity)
            {
                localContext.Trace($"Loading Webapi wrapper");

                var webApiConfiguration = JSonUtils.Deserialize<WebAPIOperationConfiguration>(_unsecureConfig);

                Dynamics365WebapiWrapper webapiWrapper =
                    new Dynamics365WebapiWrapper(webApiConfiguration.Dynamics365WebApiAddress,
                    webApiConfiguration.ResourceURI, webApiConfiguration.Authority,
                    webApiConfiguration.ClientId, webApiConfiguration.ClientSecret,
                    webApiConfiguration.RedirectUrl);

                //Dynamics365WebapiWrapper webapiWrapper =
                //    new Dynamics365WebapiWrapper("https://org5b364ce1.api.crm.dynamics.com/api/data/v9.1/",
                //    "https://org5b364ce1.crm.dynamics.com", "eyedeasinc.com",
                //    "0973bcef-78db-4353-9ee0-64deddfff244",
                //    ":W=vV9Scu0y]5Gb4JSgshDaR]av7Sjsy", "http://localhost");

                Entity webApiOperation = (Entity)localContext.PluginExecutionContext.InputParameters["Target"];
                //var webApiOperation = localContext.OrganizationService.Retrieve("eyedeas_webapioperation", entity.Id, new ColumnSet(true));

                var request = webApiOperation.GetAttributeValue<string>("eyedeas_request");
                var method = webApiOperation.GetAttributeValue<OptionSetValue>("eyedeas_method").Value;
                var body = webApiOperation.GetAttributeValue<string>("eyedeas_body");
                HttpResponseMessage response = new HttpResponseMessage();

                //POST
                if (method == 850240001) {
                    response = webapiWrapper.Post(request, body);
                    //Parse out the Id of the newly created record.
                    var entityUri = response.Headers.GetValues("OData-EntityId").ToList()[0];
                    string pattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
                    Match m = Regex.Match(entityUri, pattern, RegexOptions.IgnoreCase);
                    //update record id
                    if (m.Success) {
                        webApiOperation["eyedeas_recordid"] = m.Value;
                    }
                }
                //PATCH
                else if (method == 850240002) response = webapiWrapper.Patch(request, body);
                //DELETE
                else if (method == 850240003) response = webapiWrapper.Delete(request);
                //GET
                else response = webapiWrapper.Get(request);

                localContext.Trace($"Executed WebApi Operation with code : " + (int)response.StatusCode);

                webApiOperation["eyedeas_response"] = response.Content.ReadAsStringAsync().Result;
                webApiOperation["eyedeas_httpresponsestatuscode"] = new OptionSetValue((int)response.StatusCode);
                webApiOperation["eyedeas_exceptionmessage"] = response.ReasonPhrase;

                //localContext.OrganizationService.Update(webApiOperation);
            }
        }
    }
}
