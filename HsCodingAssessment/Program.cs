using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HsCodingAssessment
{
    class Program
    {
        static readonly string IN_ADDRESS = "dataset?userKey=d92f254c155e42a0eacb139e5045";
        static readonly string OUT_ADDRESS = "result?userKey=d92f254c155e42a0eacb139e5045";

        static async Task<PartnerList> FetchData(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(IN_ADDRESS);
            response.EnsureSuccessStatusCode();
            PartnerList fetchedData = await response.Content.ReadAsAsync<PartnerList>();
            return fetchedData;
        }
        static async Task<string> SendData(HttpClient client, MeetingSchedule processedData)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(OUT_ADDRESS, processedData);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        static MeetingSchedule GenerateOutputData(PartnerList partnerList)
        {
            MeetingSchedule output = new MeetingSchedule();
            output.countries = new List<Country>();
            Dictionary<string, List<DateTime>> countryPossibleStartDatePairs = new Dictionary<string, List<DateTime>>();

            //Complete dictionary of possible starting dates for each country
            foreach (Partner partner in partnerList.partners)
            {
                if (!countryPossibleStartDatePairs.ContainsKey(partner.country))
                {
                    countryPossibleStartDatePairs.Add(partner.country, new List<DateTime>());
                }

                foreach (string availableDate in partner.availableDates)
                {
                    DateTime availableDateConverted = DateTime.Parse(availableDate);
                    if (!countryPossibleStartDatePairs[partner.country].Contains(availableDateConverted))
                    {
                        countryPossibleStartDatePairs[partner.country].Add(availableDateConverted);
                    }

                }
            }

            //Find best starting date for each country
            foreach (KeyValuePair<string, List<DateTime>> countryDatePair in countryPossibleStartDatePairs)
            {
                //Sort date, descending, to keep the earliest date possible when composing output
                countryDatePair.Value.Sort((date1, date2) => 
                                            date2.CompareTo(date1));
                
                List<Partner> potentialPartnerInThisCountry = partnerList.partners.Where(partner => 
                                                                                         partner.country == countryDatePair.Key).ToList();
                DateTime bestDateForThisCountry = new DateTime();
                List<Partner> partnerListForBestMeeting = new List<Partner>();

                //Find date for meeting with the most attendees in country
                foreach (DateTime possibleDate in countryDatePair.Value)
                {
                    List<Partner> potentialPartnerForThisMeeting = new List<Partner>();

                    foreach (Partner partner in potentialPartnerInThisCountry)
                    {
                        //Check availability for two consecutive days
                        if (partner.availableDates.Contains(possibleDate.Date.ToString("yyyy-MM-dd"))
                            && partner.availableDates.Contains(possibleDate.AddDays(1).ToString("yyyy-MM-dd")))
                        {
                            potentialPartnerForThisMeeting.Add(partner);
                        }
                    }

                    if (potentialPartnerForThisMeeting.Count >= partnerListForBestMeeting.Count)
                    {
                        bestDateForThisCountry = possibleDate;
                        partnerListForBestMeeting = potentialPartnerForThisMeeting;
                    }
                }

                //Compose output object
                Country country = new Country();
                country.attendees = new List<string>();
                country.attendeeCount = 0;
                country.name = countryDatePair.Key;

                if (partnerListForBestMeeting.Count == 0)
                {
                    country.startDate = null;
                }

                else
                {
                    country.startDate = bestDateForThisCountry.Date.ToString("yyyy-MM-dd");
                    country.attendeeCount = partnerListForBestMeeting.Count();
                    foreach (Partner partner in partnerListForBestMeeting)
                    {
                        country.attendees.Add(partner.email);
                    }
                }
                output.countries.Add(country);
            }

            return output;
        }

        //Get data, process and send
        static async Task<string> FetchProcessSend()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://candidate.hubteam.com/candidateTest/v3/problem/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                PartnerList gotResult = await FetchData(client);
                MeetingSchedule process = GenerateOutputData(gotResult);
                string result = await SendData(client, process);
                return result;
            }

            catch (Exception e)
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
}
