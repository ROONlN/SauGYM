using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

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
            string apiKey = "AIzaSyCnYpBCvfl_d9nI9X8QSQVflrooi6-JHAI"; 

            // Denenecek Modellerin Listesi (Sırayla dener, hangisi tutarsa)
            string[] modelsToTry = new[] { "gemini-1.5-flash", "gemini-pro", "gemini-1.0-pro" };

            string userPrompt = $"Ben {age} yaşında, {height} cm boyunda, {weight} kg ağırlığında bir {gender} bireyim. " +
                                $"Hedefim: {goal}. " +
                                $"Bana samimi bir spor hocası gibi hitap et. " +
                                $"Maddeler halinde günlük beslenme tavsiyeleri ve kısa bir egzersiz programı yaz. Türkçe cevap ver.";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = userPrompt } } } }
            };

            using (var httpClient = new HttpClient())
            {
                string jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Döngü ile modelleri sırayla deniyoruz
                foreach (var modelName in modelsToTry)
                {
                    string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

                    try
                    {
                        var response = await httpClient.PostAsync(apiUrl, httpContent);
                        string responseString = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            // Başarılı olursa cevabı al ve döngüden çık
                            using (JsonDocument doc = JsonDocument.Parse(responseString))
                            {
                                if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                                {
                                    answer = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                                    answer += $"\n\n(Cevap veren model: {modelName})"; // Hangi modelin çalıştığını görelim
                                    break; // İŞLEM TAMAM, DÖNGÜYÜ KIR
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Bu model hata verdi, bir sonrakine geç...
                        continue;
                    }
                }
            }

            // Eğer tüm denemeler başarısız olduysa
            if (string.IsNullOrEmpty(answer))
            {
                answer = "⚠️ Üzgünüm, Google Gemini modellerinin hiçbiri yanıt vermedi. Lütfen API Key'ini ve internet bağlantını kontrol et.";
            }

            ViewBag.AiResponse = answer;
            ViewBag.Age = age;
            ViewBag.Height = height;
            ViewBag.Weight = weight;

            return View("Index");
        }
    }
}