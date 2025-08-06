using System.Net.Http.Headers;
using System.Text.Json;

namespace POD.Services
{
    public static class RAGService
    {
        private static readonly string OpenAiApiKey = "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"; 
        private static readonly string EmbeddingModel = "text-embedding-ada-002";
        private static readonly string ChatModel = "gpt-4o-mini";

        // 💡 Your custom knowledge base
        private static readonly List<string> Documents = new()
        {
            "We are a print-on-demand platform that allows users to create and sell custom-designed products.",
            "Buyers can browse products, customize them using our online design tool powered by Fabric.js, and place orders easily.",
            "Sellers can register an account, upload their designs, and list them for sale. We handle the printing and shipping.",
            "You can sign up as a user by just clicking the sign up button and fill the desired information! , also as a seller its easy just fill the info about you store name and just few additional info",
            "Customization is supported on multiple products including t-shirts, mugs, hoodies, and phone cases.",
            "Shipping times usually range from 3 to 7 business days, depending on the product and destination.",
            "We support high-quality digital printing and DTG (Direct-to-Garment) printing for apparel.",
            "Sellers earn a commission on every product sold. You can set your own profit margin.",
            "We handle production, packaging, and logistics so sellers only need to focus on designing and marketing.",
            "Fabric.js is used in our design editor so users can upload images, add text, rotate, resize, and move elements freely.",
            "Payments are processed securely via Stripe and PayPal. Payouts for sellers happen every 2 weeks.",
            "Returns are accepted for damaged or misprinted products only. Customized items cannot be returned unless faulty.",
            "You can start selling by creating a free account and uploading your first design in just a few minutes."
        };

        private static List<float[]>? DocumentEmbeddings = null;

        public static async Task<string> ProcessQuery(string question)
        {
            try
            {
                // Lazy init to prevent crashing on app startup
                if (DocumentEmbeddings == null)
                {
                    Console.WriteLine("Embedding documents...");
                    DocumentEmbeddings = new List<float[]>();

                    foreach (var doc in Documents)
                    {
                        try
                        {
                            var embedding = await GetEmbedding(doc);
                            DocumentEmbeddings.Add(embedding);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("⚠️ Failed to embed a document: " + ex.Message);
                            DocumentEmbeddings.Add(new float[1536]); // Fallback: zero vector
                        }
                    }

                    Console.WriteLine("✅ Document embedding complete.");
                }

                // Embed user question
                var queryEmbedding = await GetEmbedding(question);

                // Find top-2 relevant documents
                var topDocs = DocumentEmbeddings
                    .Select((embedding, i) => new { Index = i, Score = CosineSimilarity(queryEmbedding, embedding) })
                    .OrderByDescending(x => x.Score)
                    .Take(2)
                    .Select(x => Documents[x.Index])
                    .ToList();

                var prompt = string.Join("\n", topDocs) + $"\n\nAnswer this question: {question}";

                return await AskOpenAI(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Error during ProcessQuery: " + ex.Message);
                return "Sorry, an error occurred while generating a response.";
            }
        }

        private static async Task<float[]> GetEmbedding(string input)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);

            var body = new
            {
                input,
                model = EmbeddingModel
            };

            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/embeddings", body);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("📥 Embedding response:");
            Console.WriteLine(content);

            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var msg = error.GetProperty("message").GetString();
                throw new Exception($"OpenAI Embedding API Error: {msg}");
            }

            return doc.RootElement
                      .GetProperty("data")[0]
                      .GetProperty("embedding")
                      .EnumerateArray()
                      .Select(x => x.GetSingle())
                      .ToArray();
        }

        private static async Task<string> AskOpenAI(string prompt)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);

            var body = new
            {
                model = ChatModel,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("📥 GPT Response:");
            Console.WriteLine(content);

            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var msg = error.GetProperty("message").GetString();
                throw new Exception($"OpenAI Chat API Error: {msg}");
            }

            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }

        private static double CosineSimilarity(float[] a, float[] b)
        {
            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
