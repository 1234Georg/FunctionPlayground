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
                                   select el;
                    result = selected.FirstOrDefault().ToString();
                }

            }
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = $"Quartal: {quartal}, id: {id}, \nresult: \n{result}";

            return new OkObjectResult(responseMessage);
        }
    }
}
