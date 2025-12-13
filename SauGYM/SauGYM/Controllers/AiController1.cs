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
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =======================================================
        // FEATURE 1: IMAGE GENERATION (Pollinations - English)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> GenerateTransformationImage(string goal, string gender)
        {
            if (string.IsNullOrEmpty(goal) || string.IsNullOrEmpty(gender))
            {
                ViewBag.ImageError = "Please select a goal and gender to generate an image.";
                return View("Index");
            }

            string prompt = $"realistic fitness photo of a fit {gender} inside a luxury gym, body transformation goal: {goal}, 8k resolution, cinematic lighting, full body shot, highly detailed, motivational atmosphere";
            string encodedPrompt = Uri.EscapeDataString(prompt);

            string apiUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=1024&height=1024&nologo=true&seed={new Random().Next(0, 1000)}";

            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(apiUrl);
                string base64Image = Convert.ToBase64String(imageBytes);

                ViewBag.GeneratedImage = base64Image;
                ViewBag.SuccessMessage = "Simulation generated successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.ImageError = "Service unavailable: " + ex.Message;
            }

            ViewBag.Goal = goal;
            ViewBag.Gender = gender;
            return View("Index");
        }

        // =======================================================
        // FEATURE 2: FITNESS PLAN (Local Expert System - English)
        // =======================================================
        [HttpPost]
        public IActionResult GetFitnessPlan(int weight, int height, string goal, int age)
        {
            double heightM = height / 100.0;
            double bmi = weight / (heightM * heightM);

            string status, diet, workout;

            // 1. BMI Analysis
            if (bmi < 18.5)
            {
                status = "Underweight";
                diet = "- Eat 6 meals a day.\n- Increase healthy carbs (Rice, Oats).\n- Add peanut butter and nuts to your diet.";
            }
            else if (bmi < 25)
            {
                status = "Ideal Weight";
                diet = "- Maintain balanced nutrition.\n- High protein breakfast.\n- Avoid processed sugars.";
            }
            else if (bmi < 30)
            {
                status = "Overweight";
                diet = "- No carbs after 7 PM.\n- Cut out white bread completely.\n- Drink at least 3 liters of water daily.";
            }
            else
            {
                status = "Obese";
                diet = "- Create a calorie deficit immediately.\n- Eliminate processed foods and soda.\n- Focus on vegetables and lean meat.";
            }

            // 2. Goal Analysis
            if (goal.Contains("Muscle")) workout = "- 4 days/week Hypertrophy Training.\n- Hit each muscle group twice a week.\n- Rest 90sec between sets.";
            else if (goal.Contains("Weight")) workout = "- 3 days/week HIIT Cardio.\n- 30min morning walk (fasted).\n- Full Body compound movements.";
            else workout = "- 3 days/week Pilates/Yoga.\n- Regular nature walks.\n- Core strengthening exercises.";

            // 3. Result Text
            string resultText = $"Hello! Here is your AI analysis for age {age}:\n\n" +
                                $"📊 **Body Status:** {status} (BMI: {bmi:F1})\n\n" +
                                $"🍎 **Nutrition Plan:**\n{diet}\n\n" +
                                $"💪 **Workout Plan:**\n{workout}\n\n" +
                                $"Stay consistent to achieve your '{goal}' goal. Let's go!";

            ViewBag.Result = resultText;

            ViewBag.Weight = weight;
            ViewBag.Height = height;
            ViewBag.Goal = goal;
            ViewBag.Age = age;

            return View("Index");
        }
    }
}