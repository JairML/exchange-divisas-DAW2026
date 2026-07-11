namespace X_Chang.CORE.Core.Settings
{
    public class GroqSettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "llama-3.1-8b-instant";

        public string Endpoint { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
    }
}
