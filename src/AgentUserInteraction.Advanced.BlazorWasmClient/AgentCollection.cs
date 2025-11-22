using Microsoft.Agents.AI;

namespace AgentUserInteraction.Advanced.BlazorWasmClient;

public record AgentCollection(
    AIAgent MovieAgent,
    ChatClientAgent WeatherAgent,
    ChatClientAgent WeatherAgentWithStructuredContent,
    ChatClientAgent ChangeColorAgent);