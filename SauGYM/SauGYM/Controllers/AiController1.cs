using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SauGYM.Controllers
{
    public class AiController : Controller
    {
        private readonly HttpClient _httpClient;

        public AiController()
        {
            _httpClient = new HttpClient();
            // Zaman aşımını biraz artıralım, resim oluşturmak bazen 10-15 sn sürebilir.
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =======================================================
        // 1. ÖZELLİK: HAYALİNDEKİ VÜCUDU ÇİZ (Görsel Üretimi)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> GenerateTransformationImage(string goal, string gender)
        {
            // Basit Kontrol
            if (string.IsNullOrEmpty(goal) || string.IsNullOrEmpty(gender))
            {
                ViewBag.ImageError = "Lütfen görsel oluşturmak için hedef ve cinsiyet seçin.";
                return View("Index");
            }

            // Pollinations.ai için İngilizce prompt hazırlıyoruz.
            // Örn: "Fit bir erkeğin lüks spor salonunda gerçekçi fotoğrafı, hedef: kas yapmak..."
            string prompt = $"realistic fitness photo of a fit {gender} inside a luxury gym, body transformation goal: {goal}, 8k resolution, cinematic lighting, full body shot, highly detailed, motivational atmosphere";

            // URL Encoding
            string encodedPrompt = Uri.EscapeDataString(prompt);

            // Pollinations.ai API (Tamamen Ücretsiz)
            string apiUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=1024&height=1024&nologo=true&seed={new Random().Next(0, 1000)}";

            try
            {
                // Resmi sunucu tarafında indirip Base64 formatına çeviriyoruz.
                // Bu sayede resim tarayıcıda anında görünür.
                var imageBytes = await _httpClient.GetByteArrayAsync(apiUrl);
                string base64Image = Convert.ToBase64String(imageBytes);

                ViewBag.GeneratedImage = base64Image;
                ViewBag.SuccessMessage = "Simülasyon görseli başarıyla oluşturuldu!";
            }
            catch (Exception ex)
            {
                ViewBag.ImageError = "Görsel servisine erişilemedi: " + ex.Message;
            }

            // Form verilerini geri gönderelim
            ViewBag.Goal = goal;
            ViewBag.Gender = gender;

            return View("Index");
        }

        // =======================================================
        // 2. ÖZELLİK: SPOR VE DİYET TAVSİYESİ (Metin Üretimi)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> GetFitnessPlan(int weight, int height, string goal, int age)
        {
            // SENİN API KEY'İN (Buraya ekledim)
            string apiKey = "AIzaSyCUOrZ40dKZwGQvf77un62st2YMB3cUgo8";

            string prompt = $"{age} yaşında, {weight} kg ağırlığında, {height} cm boyunda ve amacı '{goal}' olan biri için samimi bir dille spor ve diyet tavsiyesi ver. Cevabı kısa maddeler halinde Türkçe ver.";

           
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var requestData = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);

                    // JSON içinden cevabı çekme
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString();
                        ViewBag.Result = text;
                    }
                    else
                    {
                        ViewBag.Result = "Yapay zeka cevap veremedi.";
                    }
                }
                else
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Google Hatası: {response.StatusCode} - Detay: {errorDetail}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Bağlantı hatası: " + ex.Message;
            }

            // Verileri koru
            ViewBag.Weight = weight;
            ViewBag.Height = height;
            ViewBag.Goal = goal;
            ViewBag.Age = age;

            return View("Index");
        }
    }
}