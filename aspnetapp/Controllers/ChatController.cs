using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly string _apiUrl;

    public ChatController()
    {
        _apiUrl = "http://20.127.103.201";
    }

    [HttpPost]
    public async Task<IActionResult> GenerateResponseAsync([FromBody] ChatRequest request)
    {
        var payload = new
        {
            model = "gpt-3.5-turbo",
            stream = true,
            messages = request.inputMsg,
            temperature = 0.7
        };

        using (var client = new HttpClient())
        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiUrl))
        {
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            using (var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.IsSuccessStatusCode)
                {
                    Response.StatusCode = (int)response.StatusCode;

                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            await Response.WriteAsync(line);
                            await Response.Body.FlushAsync();  
                        }
                    }

                    return new EmptyResult();
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error: " + response.StatusCode);
                }
            }
        }
    }
}

public class ChatRequest
{
    public string inputMsg { get; set; }
}
