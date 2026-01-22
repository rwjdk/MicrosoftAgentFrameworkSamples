using AgentUserInteraction.Advanced.SharedModels;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;

#pragma warning disable MEAI001

namespace AgentUserInteraction.Advanced.BlazorWasmClient.Pages;

public partial class Home(AgentCollection agentCollection)
{
    private string? _city;
    private string? _cityStructured;
    private string? _movieName;
    private MovieResult? _movieResult;
    private string? _weatherText;
    private string? _weatherTextStructure;
    private WeatherReport? _weather;
    private string? _requestedColor;
    public static string? Color { get; set; }

    private async Task WeatherLookup()
    {
        if (!string.IsNullOrWhiteSpace(_city))
        {
            _weatherText = null;
            await foreach (AgentResponseUpdate update in agentCollection.WeatherAgent.RunStreamingAsync(_city))
            {
                _weatherText += update;
            }
        }
    }

    private async Task WeatherLookupWithStructuredContent()
    {
        if (!string.IsNullOrWhiteSpace(_cityStructured))
        {
            _weatherTextStructure = null;

            await foreach (AgentResponseUpdate update in agentCollection.WeatherAgentWithStructuredContent.RunStreamingAsync(_cityStructured))
            {
                _weatherTextStructure += update;
                foreach (AIContent content in update.Contents)
                {
                    if (content is DataContent dataContent)
                    {
                        string json = Encoding.UTF8.GetString(dataContent.Data.Span);
                        _weather = WeatherReport.FromJson(json);
                    }
                }
            }
        }
    }

    private async Task MovieLookup()
    {
        if (!string.IsNullOrWhiteSpace(_movieName))
        {
            AgentResponse response = await agentCollection.MovieAgent.RunAsync(_movieName);
            _movieResult = MovieResult.FromJson(response.Text);
        }
    }

    private async Task ChangeColor()
    {
        if (!string.IsNullOrWhiteSpace(_requestedColor))
        {
            await agentCollection.ChangeColorAgent.RunAsync($"Change Color to {_requestedColor}");
            StateHasChanged();
        }
    }
}