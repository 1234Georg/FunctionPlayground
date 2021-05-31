using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace FunctionPlayground
{
    /*
    Urls:
      Azure Resource Group: https://ms.portal.azure.com/#resource/subscriptions/d7feceb8-a7a2-43a6-884d-317219b9ea5b/resourceGroups/FunctionPlayground-dev-rg/overview
      Azure Storage: https://ms.portal.azure.com/#resource/subscriptions/d7feceb8-a7a2-43a6-884d-317219b9ea5b/resourceGroups/FunctionPlayground-dev-rg/providers/Microsoft.Storage/storageAccounts/functionplayground2
      Azure Function App: http://FunctionPlayground-dev-as.azurewebsites.net
      Azure DevOps Project: FunctionPlayground (https://clt-57830401-33b2-4c94-b6b2-0ccd75eea17d.visualstudio.com/FunctionPlayground)
      Azure DevOps build pipeline: https://clt-57830401-33b2-4c94-b6b2-0ccd75eea17d.visualstudio.com/051c7269-fe8d-4a84-b6a8-053695b87718/_build/definition?definitionId=1
      Azure DevOps release pipeline: https://clt-57830401-33b2-4c94-b6b2-0ccd75eea17d.visualstudio.com/051c7269-fe8d-4a84-b6a8-053695b87718/_release?definitionId=1
      Link zur Function mit ID: http://functionplayground-dev-as.azurewebsites.net/api/2021q2/46671479
      Link zur Function: http://functionplayground-dev-as.azurewebsites.net/api/2021q2
     */
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{quartal}/{id?}")] HttpRequest req,
            [Blob("samples/VA_1.0_74_tf+{quartal}_nr+1.xml", FileAccess.Read)] Stream stream,
            string quartal, 
            string id,
            ILogger log)
        {
            string result = "";
            using (StreamReader oReader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1")))
            {
                XDocument xmlDoc = XDocument.Load(oReader);
                if (id == null)
                    result = xmlDoc.ToString();
                else
                {
                    var selected = from el in xmlDoc.Descendants()
                                   where (string)el.Attribute("V") == id
                                   select new
                                   {
                                       Titel = el.Elements().Where(_ => _.Name.LocalName == "titel").Single().Attribute("V").Value,
                                       Content = GetContent(el.Elements().Where(_ => _.Name.LocalName == "beschreibung").Single())
                                   };

                    result = $"<h3>{selected.FirstOrDefault().Titel}</h3>\n{selected.FirstOrDefault().Content}";
                }

            }
            log.LogInformation("C# HTTP trigger function processed a request.");


            string responseMessage = @$"
                <html>
                <head>
                    <meta charset='utf-8'/>
                </head>
                <body>
                    Quartal: {quartal}, id: {id}, result: </br>
                    {result}
                </body>
                </html>
                ";
            return new ContentResult { Content = responseMessage, ContentType = "text/html" };
        }

        private static string GetContent(XElement xElement)
        {
            if (!xElement.HasElements)
                return $"<{xElement.Name.LocalName}>{xElement.Value}</{xElement.Name.LocalName}>";

            string result = "";
            foreach (var child in xElement.Elements())
            {
                result += GetContent(child) + "\n";
            }
            result = $"<{xElement.Name.LocalName}>{result}</{xElement.Name.LocalName}>";
            return result;
        }
    }
}
