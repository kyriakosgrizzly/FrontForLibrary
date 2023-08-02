using front.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace front.Controllers
{
    public class BookController : Controller
    {
        [Authorize(AuthenticationSchemes = "Cookies")]
        public async Task<IActionResult> Index(string searchString,string FilterParam)
        {
            
            List<Book> BookInfo = new List<Book>();
            using (var client = new HttpClient())
            {
                //Passing service base url
                client.BaseAddress = new Uri("https://localhost:8888/");
                client.DefaultRequestHeaders.Clear();
                
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage Res;

                if (searchString != null)
                {
                    if(FilterParam != null)
                    {
                        var f = FilterParam == "ByTitle" ? 0 : 1;
                        Res = await client.GetAsync($"api/Books/titles?title={searchString}&f={f}");
                    }
                    else Res = await client.GetAsync($"api/Books/titles?title={searchString}");
                }
                else
                {
                    Res = await client.GetAsync("api/Books/titles");
                }
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var Books = Res.Content.ReadAsStringAsync().Result;
                    //Deserializing the response recieved from web api and storing into the Employee list

                    BookInfo = JsonConvert.DeserializeObject<List<Book>>(Books);
                }
                //returning the employee list to view
                return View(BookInfo);
            }
        }
        public async Task<IActionResult> Details(int BookId)
        {

            Book BookInfo = new Book();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:8888/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage Res = await client.GetAsync($"api/Books/{BookId.ToString()}");
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var Book = Res.Content.ReadAsStringAsync().Result;
                    //Deserializing the response recieved from web api and storing into the Employee list
                    BookInfo = JsonConvert.DeserializeObject<Book>(Book);
                }
                //returning the employee list to view
                return View(BookInfo);
            }
        }
    }
}
