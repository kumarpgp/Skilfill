using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Skilfill.Model;

namespace Skilfill
{
    class Program
    {
        public static string Token = string.Empty;
        private static string APIUrl = "http://magazinestore.azurewebsites.net";
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            Program P = new Program();
            Token = await P.GetToken();
            List<string> Subscriberid = await P.GetCatogery(Token);
           AnsResponse Resp = await P.PostResult(Token, Subscriberid);
            Console.ReadLine();
        }

        public async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync("api/token");
                if (response.IsSuccessStatusCode)
                {
                    Token = await response.Content.ReadAsStringAsync();
                    TokenInfo authToken = JsonConvert.DeserializeObject<TokenInfo>(Token);
                    if (authToken != null)
                    {
                        if (authToken.Success)
                        {
                            Token = authToken.Token;
                        }
                    }
                }

            }
            return Token;
        }

        public async Task<List<string>> GetCatogery(String authToken)
        {
            Magazine magresdata = null;
            String Magazineid = string.Empty;
            DataTable DtMagazine = new DataTable();
            DtMagazine.Columns.Add("id");
            DtMagazine.Columns.Add("name");
            DtMagazine.Columns.Add("category");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                HttpResponseMessage response = await client.GetAsync("api/categories/" + authToken);
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsStringAsync();
                    Category categoryClass = JsonConvert.DeserializeObject<Category>(readTask);
                    foreach (var i in categoryClass.data)
                    {
                        magresdata = await Getmagazines(authToken, i);
                        for (int o = 0; o < magresdata.data.Count; o++)
                        {
                            DataRow dr = DtMagazine.NewRow();
                            dr["id"] = magresdata.data[o].id;
                            dr["name"] = magresdata.data[o].name;
                            dr["category"] = magresdata.data[o].category;
                            DtMagazine.Rows.Add(dr);
                        }
                    }
                }
            }
            DataTable DtFinal = await Getsubscribers(authToken);
            DataTable DtSubscriber = DtFinal.Clone();
            string Subscriberid = string.Empty;
            string[] UniqCat = DtMagazine.DefaultView.ToTable(true, "category").AsEnumerable().Select(r => r.Field<string>("category")).ToArray();
            for(int uc=0;uc<UniqCat.Length;uc++)
            {
                Magazineid = string.Empty;
                DataRow[] drmagid = DtMagazine.Select("category in ('" + UniqCat[uc].ToString() + "')");
                for(int row=0;row<drmagid.Length;row++)
                {
                    Magazineid += "'" + drmagid[row]["id"].ToString() + "',";                    
                }
                Magazineid = Magazineid.Substring(0, Magazineid.Length - 1);
                             
                if (Subscriberid == string.Empty)
                {
                    DataRow[] result = DtFinal.Select("magazineIds in(" + Magazineid + ")");
                    for (int m = 0; m < result.Length; m++)
                    {
                        Subscriberid += "'" + result[m]["id"].ToString() + "',";
                    }
                }
                else
                {
                    DataRow[] result = DtFinal.Select("magazineIds in(" + Magazineid + ") and id in ("+ Subscriberid + ")");
                    Subscriberid = string.Empty;
                    for (int m = 0; m < result.Length; m++)
                    {
                        Subscriberid += "'" + result[m]["id"].ToString() + "',";
                    }

                }
            }
            


            Subscriberid = Subscriberid.Substring(0, Subscriberid.Length - 1);
            Subscriberid = Subscriberid.Replace("'", string.Empty);
            string[] dist = Subscriberid.Split(',');
            string[] subscriberValue = dist.Distinct().ToArray();
            List<string> SubscriberAns = new List<string>(subscriberValue);
            
            return SubscriberAns;
        }
        public async Task<Magazine> Getmagazines(String authToken, String Category)
        {
            Magazine categoryClass = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                HttpResponseMessage response = await client.GetAsync("api/magazines/" + authToken + "/" + Category);
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsStringAsync();
                    categoryClass = JsonConvert.DeserializeObject<Magazine>(readTask);
                }

            }
            return categoryClass;
        }
        public async Task<DataTable> Getsubscribers(String authToken)
        {
            subscribers subscribersClass = null;
            DataTable DtSubscriber = new DataTable();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/subscribers/" + authToken);
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsStringAsync();
                    subscribersClass = JsonConvert.DeserializeObject<subscribers>(readTask);
                    
                }

                
                DtSubscriber.Columns.Add("id");
                DtSubscriber.Columns.Add("firstName");
                DtSubscriber.Columns.Add("lastName");
                DtSubscriber.Columns.Add("magazineIds");
                for (int k = 0; k < subscribersClass.data.Count; k++)
                {
                    int[] magazineIds = subscribersClass.data[k].magazineIds;
                    for (int m = 0; m < magazineIds.Length; m++)
                    {
                        DataRow dr = DtSubscriber.NewRow();
                        dr["id"] = subscribersClass.data[k].id.ToString();
                        dr["firstName"] = subscribersClass.data[k].firstName.ToString();
                        dr["lastName"] = subscribersClass.data[k].lastName.ToString();
                        dr["magazineIds"] = magazineIds[m].ToString();
                        DtSubscriber.Rows.Add(dr);
                    }
                }
            }
            return DtSubscriber;
        }

        public async Task<AnsResponse> PostResult(string authToken,List<string> Suscriber)
        {
            AnsResponse Ansresp = null;
            using (var client = new HttpClient())
            {
                Answers AnsValue = new Answers();
                AnsValue.Subscribers = Suscriber;
                client.BaseAddress = new Uri(APIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
               // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(JsonConvert.SerializeObject(AnsValue), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/answer/" + authToken, content);
                if (response.IsSuccessStatusCode)
                {
                    string Result = await response.Content.ReadAsStringAsync();
                    Ansresp = JsonConvert.DeserializeObject<AnsResponse>(Result);

                    Console.WriteLine(Result);
                }
            }
            return Ansresp;
        }

    }
}