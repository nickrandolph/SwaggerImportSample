

using System.Diagnostics;

namespace SwaggerImport.Data
{
    public partial class swaggerClient 
    {
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {
            Debug.WriteLine(url);
        }
    }
}
