using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using ProjectAssistant.Business.Services.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectAssistant.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatAIController : ControllerBase
{
    private readonly SystemSettingsConfigurationService systemSettingsConfigurationService;

    public ChatAIController(ILogger<ChatAIController> logger,
        SystemSettingsConfigurationService systemSettingsConfigurationService)
    {
        this.systemSettingsConfigurationService = systemSettingsConfigurationService;
    }

    [HttpPost]
    public string PostChatMessage([FromBody] string prompt)
    {
        string result = string.Empty;

        var endpoint = new Uri(systemSettingsConfigurationService.Value.AILicenseKey.AzureOpenAIEndPoint);
        var model = systemSettingsConfigurationService.Value.AILicenseKey.AzureOpenAIModelName;
        var deploymentName = systemSettingsConfigurationService.Value.AILicenseKey.AzureOpenAIModelName;
        var apiKey = systemSettingsConfigurationService.Value.AILicenseKey.AzureOpenAIKey;

        AzureOpenAIClient azureClient = new(
            endpoint,
            new AzureKeyCredential(apiKey));
        ChatClient chatClient = azureClient.GetChatClient(deploymentName);

        var requestOptions = new ChatCompletionOptions()
        {
            //MaxOutputTokenCount = 100000,
             Temperature = 0.7f,
        };

        List<ChatMessage> messages = new List<ChatMessage>()
        {
            new UserChatMessage(prompt),
        };

        var response = chatClient.CompleteChat(messages);
        result = response.Value.Content[0].Text;



        return result;
    }
}
