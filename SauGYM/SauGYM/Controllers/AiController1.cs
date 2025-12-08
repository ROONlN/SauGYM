using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json; // JSON işlemleri için gerekli

namespace SauGYM.Controllers
{
    public class AiController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetAdvice(int age, int height, double weight, string gender, string goal)
        {
            string answer = "";

            // 1. Google Gemini API Anahtarın (Buraya Yapıştır)
            string apiKey = "AIzaSyCnYpBCvfl_d9nI9X8QSQVflrooi6-JHAI";

            // 2. İstek Adresi (Google'ın sunucusu)
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0:generateContent?key={apiKey}";

            // 3. Prompt (Yapay Zekaya Sorulacak Soru)
            string userPrompt = $"Ben {age} yaşında, {height} cm boyunda, {weight} kg ağırlığında bir {gender} bireyim. " +
                                $"Hedefim: {goal}. " +
                                $"Bana samimi bir spor hocası gibi hitap et. " +
                                $"Maddeler halinde günlük beslenme tavsiyeleri ve kısa bir egzersiz programı yaz.";

            // 4. JSON Verisini Hazırlama (Google'ın istediği format)
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = userPrompt } } }
                }
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // İsteği Gönder
                    var response = await httpClient.PostAsync(apiUrl, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        // Cevabı Oku
                        string responseString = await response.Content.ReadAsStringAsync();

                        // JSON'dan cevabı ayıkla (Biraz karışık bir yapısı var, buradan çekiyoruz)
                        using (JsonDocument doc = JsonDocument.Parse(responseString))
                        {
                            answer = doc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString();
                        }
                    }
                    else
                    {
                        answer = "Google Gemini servisine ulaşılamadı. Hata Kodu: " + response.StatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                answer = "Bir hata oluştu: " + ex.Message;
            }

            // Cevabı View'a taşı
            ViewBag.AiResponse = answer;

            // Form verilerini koru
            ViewBag.Age = age;
            ViewBag.Height = height;
            ViewBag.Weight = weight;

            return View("Index");
        }
    }
}