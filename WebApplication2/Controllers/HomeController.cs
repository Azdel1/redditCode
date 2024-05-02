using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }
       
        public IActionResult FetchDataAsync()
        {
            Console.WriteLine("fetch data called");
            return Ok();
        }
        [HttpPost]
        public ActionResult UpdateOrder()
        {
            // some code
            Console.WriteLine("fetch data called");
            return Json(new { success = true, message = "Order updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> FetchDataAsync1([FromBody] string startTime)
        {
            Console.WriteLine("fetch data called");
            try
            {
                // Convert the start time to Unix timestamp
                DateTime parsedStartTime = DateTime.Parse(startTime);
                //DateTime parsedStartTime = new DateTime(2024, 5, 2, 15, 44, 22, 216);
                long unixStartTime = (long)(parsedStartTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


                var subreddit = "movies";
                var apiUrl = $"https://oauth.reddit.com/r/{subreddit}/new?after={unixStartTime}";

                using (var client = _httpClientFactory.CreateClient())
                {
                    // Add the necessary headers for authorization and user agent
                    client.DefaultRequestHeaders.Add("Authorization", $"bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IlNIQTI1NjpzS3dsMnlsV0VtMjVmcXhwTU40cWY4MXE2OWFFdWFyMnpLMUdhVGxjdWNZIiwidHlwIjoiSldUIn0.eyJzdWIiOiJ1c2VyIiwiZXhwIjoxNzE0NjYyNzMxLjYzNzc0NCwiaWF0IjoxNzE0NTc2MzMxLjYzNzc0NCwianRpIjoiLUt1ZDF3RGtzYTJSSVllLWFidERSZkozbjZpNWtBIiwiY2lkIjoiN3U1cTN3TEhaX2oteVgxVEV3QXBYUSIsImxpZCI6InQyX2tsM3ZtbWNsIiwiYWlkIjoidDJfa2wzdm1tY2wiLCJsY2EiOjE2NDcwMDcwMjk2NDAsInNjcCI6ImVKeUtWdEpTaWdVRUFBRF9fd056QVNjIiwiZmxvIjo5fQ.WCRCo8KhALAp75wLUXcsq5o-8k46WnUch5AwvMTRgUAiaf97a1fxLHZEm-YgwlKuAsbAW6N-6daWYvSnoxvJWQtLL5fI9a58qIoSJuSZCPOEuUSvaePGfyK4fF9nyYt64i-yBFk6P1ZpMYICSXFh_ANGGXVNdZk25gayM5CxozITZ9vHyQLZQvgHIilTgS-7InE2lv8IX3D5XGjacbbcCAjunpnkabC2aoqYqG-6FZ64cypoPfXf0VtI2dFZ7jVpfvpH-tDC9K1IprM6-rtpiFiXtththh1cSn9HT2JRTEVX_raqqWIgXGsTIO1_5JgpoQgPm7QK2pseRZvcK2Yorg");
                    client.DefaultRequestHeaders.Add("User-Agent", "ChangeMeClient/0.1 by YourUsername");

                    // Make the GET request to the Reddit API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful (status code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        string redditData = await response.Content.ReadAsStringAsync();

                        // Deserialize the JSON response into a dynamic object
                        dynamic jsonData = JsonConvert.DeserializeObject(redditData);

                        // Extract the required data
                        var mostUpvotedPost = ((IEnumerable<dynamic>)jsonData.data.children)
                            .OrderByDescending(p => p.data.ups)
                            .FirstOrDefault();

                        var mostUpvotedPostUrl = mostUpvotedPost?.data.url.ToString();
                        var mostUpvotedPostUpvotes = mostUpvotedPost?.data.ups ?? 0;

                        // Explicitly cast jsonData.data.children to IEnumerable<dynamic>
                        var userPostsCounts = ((IEnumerable<dynamic>)jsonData.data.children)
                            .GroupBy(p => p.data.author.ToString())
                            .Select(g => new
                            {
                                Username = g.Key,
                                PostCount = g.Count()
                            })
                            .OrderByDescending(g => g.PostCount)
                            .FirstOrDefault();

                        var mostActiveUser = userPostsCounts?.Username;
                        var mostActiveUserPostCount = userPostsCounts?.PostCount ?? 0;

                        // Return the extracted data
                        return Ok(new
                        {
                            MostUpvotedPostUrl = mostUpvotedPostUrl ?? "No post after the start time",
                            MostUpvotedPostUpvotes = mostUpvotedPostUpvotes,
                            MostActiveUser = mostActiveUser ?? "No users",
                            MostActiveUserPostCount = mostActiveUserPostCount
                        });
                    }
                    else
                    {
                        // If the request fails, return an error message
                        return StatusCode((int)response.StatusCode, $"Failed to retrieve data. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, return an error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public IActionResult End()
        {
            return Json(new { message = "End action called" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
