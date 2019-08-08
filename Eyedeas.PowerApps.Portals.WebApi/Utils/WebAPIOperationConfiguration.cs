using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eyedeas.PowerApps.Portals.WebApi.Utils
{
    public class WebAPIOperationConfiguration
    {
        public string Dynamics365WebApiAddress { get; set; }
        public string ResourceURI { get; set;}
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }
    }
}
