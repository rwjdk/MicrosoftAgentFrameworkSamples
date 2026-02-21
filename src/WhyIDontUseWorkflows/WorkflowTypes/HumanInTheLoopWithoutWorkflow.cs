using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;

namespace WhyIDontUseWorkflows.WorkflowTypes;

public static class HumanInTheLoopWithoutWorkflow
{
    public static async Task Run()
    {
        Console.Clear();
        Secrets secrets = SecretManager.GetSecrets();
        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        ChatClient chatClient = client.GetChatClient("gpt-4.1");
        var agent = chatClient.AsAIAgent(instructions: "You are the judge in a guessing game where it is about guessing animals. " +
                                                       "Each hint should only give one fact");

        List<string> animals = ["Wolf", "Eagle", "Tiger", "Dolphin", "Elephant", "Grizzly Bear", "Mouse", "Dog", "Shark", "Panda"];

        var animalToGuess = animals[Random.Shared.Next(animals.Count)];
        var initialHintResponse = await agent.RunAsync<string>($"Make a vague hint for the animal: '{animalToGuess}'");

        int numberOfTries = 0;
        IList<string> hintsGiven = [];
        Console.WriteLine("Guess what animal I'm thinking of");
        string hint = initialHintResponse.Result;
        while (true)
        {
            numberOfTries++;
            Console.WriteLine($"Hint: {hint}");
            Console.Write("Your Guess: ");
            var guess = Console.ReadLine();
            var input = $"Is this the right answer for guessing the animal is '{animalToGuess}'? (allow for spelling errors of the animal): {guess}";
            AgentResponse<bool> isRightAnswerResponse = await agent.RunAsync<bool>(input);
            if (isRightAnswerResponse.Result)
            {
                Utils.WriteLineGreen($"You guessed it. " +
                                  $"The answer is indeed a {animalToGuess}. " +
                                  $"Tries used: {numberOfTries}");
                break;
            }

            //Not correct answer: Let's generate a new Hint
            var newHintPrompt = $"Generate a hint for a child to guess the animal '{animalToGuess}'. " +
                                $"Hints already given: {string.Join(" | ", hintsGiven)} " +
                                $"so make the new hint unique and do not repeat the same hint parts";
            AgentResponse<string> hintResponse = await agent.RunAsync<string>(newHintPrompt);
            var newHint = hintResponse.Result;
            hintsGiven.Add(newHint);
            hint = newHint;
            Console.WriteLine("That is not the animal I'm thinking of");
        }
    }
}