using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HsCodingAssessment
{
    class Program
    {
        static readonly string _IN_ADDRESS = "dataset?userKey=d92f254c155e42a0eacb139e5045";
        static readonly string _OUT_ADDRESS = "result?userKey=d92f254c155e42a0eacb139e5045";

        static async Task<InputObj> FetchData(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(_IN_ADDRESS);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<InputObj>();
            return result;
        }

        static OutputObj ProcessData(InputObj input)
        {
            OutputObj output = new OutputObj();
            output.countries = new List<Country>();
            Dictionary<string, List<DateTime>> countryPotentialStartDatePairs = new Dictionary<string, List<DateTime>>();

            foreach(Partner partner in input.partners)
            {
                if (!countryPotentialStartDatePairs.ContainsKey(partner.country))
                {
                    countryPotentialStartDatePairs.Add(partner.country, new List<DateTime>());
                }

                foreach(string availableDate in partner.availableDates)
                {
                    DateTime availableDateConverted = DateTime.Parse(availableDate); 
                    if (!countryPotentialStartDatePairs[partner.country].Contains(availableDateConverted))
                    {
                        countryPotentialStartDatePairs[partner.country].Add(availableDateConverted);
                    }
                    
                }
            }


            foreach(KeyValuePair<string, List<DateTime>> countryDatePair in countryPotentialStartDatePairs)
            {
                countryDatePair.Value.Sort((date1, date2) => date2.CompareTo(date1));

                List<Partner> potentialPartnerInThisCountry = input.partners.Where(partner => partner.country == countryDatePair.Key).ToList();
                DateTime bestDateForThisCountry = new DateTime();
                List<Partner> partnerListForBestMeeting = new List<Partner>();

                foreach(DateTime date in countryDatePair.Value)
                {
                    List<Partner> potentialPartnerForThisMeeting = new List<Partner>();

                    foreach (Partner partner in potentialPartnerInThisCountry)
                    {
                        if (partner.availableDates.Contains(date.Date.ToString("yyyy-MM-dd")) 
                            && partner.availableDates.Contains(date.AddDays(1).ToString("yyyy-MM-dd")))
                        {
                            potentialPartnerForThisMeeting.Add(partner);
                        }
                    }

                    if (potentialPartnerForThisMeeting.Count >= partnerListForBestMeeting.Count)
                    {
                        bestDateForThisCountry = date;
                        partnerListForBestMeeting = potentialPartnerForThisMeeting;
                    }
                }

                Country country = new Country();
                country.attendees = new List<string>();
                country.attendeeCount = 0;
                country.name = countryDatePair.Key;

                if(partnerListForBestMeeting.Count == 0)
                {
                    country.startDate = null;
                }

                else
                {
                    country.startDate = bestDateForThisCountry.Date.ToString("yyyy-MM-dd");
                    country.attendeeCount = partnerListForBestMeeting.Count();
                    foreach(Partner partner in partnerListForBestMeeting)
                    {
                        country.attendees.Add(partner.email);
                    }
                }
                output.countries.Add(country);
            }

            return output;
        }

        static async Task<string> SendData(HttpClient client, OutputObj processedData)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(_OUT_ADDRESS, processedData);
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
                OutputObj process = ProcessData(gotResult);
                string result = await SendData(client, process);
                return result;
            }

            catch(Exception e)
            {
                return e.Message;
            }

        }

        static void Main(string[] args)
        {
            string result = FetchProcessSend().Result;
            Console.WriteLine(result);
            Console.ReadLine();
        }

}
