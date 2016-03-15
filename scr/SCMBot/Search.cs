using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SCMBot
{
    public class Search
    {
        //pointer
        private MarketAuth Auth;


        public Semaphore Sem = new Semaphore(0, 1);

        public List<SearchItem> searchList = new List<SearchItem>();

        private BackgroundWorker searchThread = new BackgroundWorker();


        public event SearchMessagesHandler SearchMessages;
        public delegate void SearchMessagesHandler(object obj, MyEventArgs e);


        public CookieContainer cookieCont = new CookieContainer();

        public Search(MarketAuth auth)
        {
            Auth = auth;

            searchThread.WorkerSupportsCancellation = true;
            searchThread.DoWork += new DoWorkEventHandler(reqThread_DoWork);

         }


        public class SearchItem
        {

            public SearchItem(string name, string game, string link, string quant, decimal startprice, string imglink)
            {
                this.Name = name;
                this.Game = game;
                this.Link = link;
                this.Quant = quant;
                this.StartPrice = startprice;
                this.ImgLink = imglink;
            }

            public string Name { set; get; }
            public string Game { set; get; }
            public string Link { set; get; }
            public string ImgLink { set; get; }
            public string Quant { set; get; }
            public decimal StartPrice { set; get; }

        }


        private void fireMessage(int code, int percent, string message)
        {
            SearchMessages(this, new MyEventArgs(code, percent, message));
        }


        public void LoadSearch(string link)
        {
            if (searchThread.IsBusy != true)
            {
                searchThread.RunWorkerAsync(link);
            }
        } 


        private void reqThread_DoWork(object sender, DoWorkEventArgs e)
        {

            var content = Auth.SendGet(e.Argument.ToString());

            searchList.Clear();
            int totalFind = 0;

            try
            {
                var searchJS = JsonConvert.DeserializeObject<Steam.SearchBody>(content);

                if (searchJS.Success)
                {
                    totalFind = searchJS.TotalCount;

                    //content = File.ReadAllText(@"C:\dollar2.html");
                    MatchCollection matches = Regex.Matches(searchJS.HtmlRes, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                    if (matches.Count != 0)
                    {
                        foreach (Match match in matches)
                        {
                            string currmatch = match.Groups[1].Value;

                            //Fix for Steam update 5/01/14 4:00 PM PST
                            string ItemUrl = Regex.Match(currmatch, "(?<==\")(.*?)(?=\" id)").ToString();

                            string ItemQuan = Regex.Match(currmatch, "(?<=num_listings_qty\">)(.*?)(?=</span>)").ToString();

                            //Fix for Steam update 3/26/14 4:00 PM PST
                            string ItemPrice = Regex.Match(currmatch, "(?<=class=\"normal_price\">)(.*?)(?=</span>)", RegexOptions.Singleline).ToString();

                            //Fix fot Steam update 3/26/14 4:00 PM PST
                            string ItemName = Regex.Match(currmatch, "(?<=listing_item_name\" style=\"color:)(.*?)(?=</span>)").ToString();
                            ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);

                            string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();

                            string ItemImg = Regex.Match(currmatch, "(?<=net/economy/image/)(.*?)(/62fx62f)", RegexOptions.Singleline).ToString();

                            //Заполняем список 
                            searchList.Add(new SearchItem(ItemName, ItemGame, ItemUrl, ItemQuan, Steam.ToDecimal(Auth.CleanPrice(ItemPrice)), ItemImg));
                        }

                        fireMessage(0, totalFind, "Items found: " + totalFind.ToString());
                    }
                    else
                        fireMessage(1, 0, "Nothing found");
                }
            }
            catch (Exception ex)
            {
                Main.AddtoLog(ex.Message);
                fireMessage(1, 0, "Error parsing search results");
            }

        }





    
    }

    }


