using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using SynapseSRE.Domain.Interfaces;
using System.ClientModel;
namespace SynapseSRE.Infrastructure.Services;

public class OpenAIEdgeAgent : IAgentService
{
    private readonly ChatClient _client;
    private readonly string _model;

public OpenAIEdgeAgent(IConfiguration config)
{
    var apiKey = config["AI:ProviderKey"] ?? "none";
    var baseUrl = config["AI:BaseUrl"] ?? "https://api.openai.com/v1";
    _model = config["AI:ModelId"] ?? "gpt-4o";

    var options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
    
    var credential = new ApiKeyCredential(apiKey); 
    
    var fullClient = new OpenAIClient(credential, options);
    _client = fullClient.GetChatClient(_model);
}

    public async Task<string> AnalyzeIncidentAsync(string title, string description, string? logContent)
    {
        var systemPrompt = "Eres un experto SRE. Analiza el incidente y responde brevemente.";
        var userPrompt = $"Título: {title}\nDescripción: {description}\nLogs: {logContent ?? "No logs provided"}";

        try 
        {
            ChatCompletion completion = await _client.CompleteChatAsync(
                [new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)]);
            
            return completion.Content[0].Text;
        }
        catch (Exception ex) 
        {
            return $"Error de conexión con la IA: {ex.Message}";
        }
    }
}