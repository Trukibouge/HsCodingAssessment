using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HsCodingAssessment
{
    class Program
    {
        static readonly string _IN_URI = "dataset?userKey=/";
        static readonly string _OUT_URI = "result?userKey=/";

        static async Task<InputObj> FetchData(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(_IN_URI);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<InputObj>();
            return result;
        }

        static OutputObj ProcessData(InputObj input)
        {
            OutputObj output = new OutputObj();
            return output;
        }

        static async Task<string> SendData(HttpClient client, OutputObj processedData)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(_OUT_URI, processedData);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        //Get data, process it and send it to API
        static async Task<string> FetchProcessSend()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://candidate.hubteam.com/candidateTest/v3/problem/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                InputObj gotResult = await FetchData(client);
                OutputObj processedResult = ProcessData(gotResult);
                string result = await SendData(client, processedResult);
                return result;
            }

            catch(Exception e)
            {
                return e.Message;
            }

        }

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.ReadLine();
        }
    }
}
