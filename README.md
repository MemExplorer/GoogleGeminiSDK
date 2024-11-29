# GoogleGeminiSDK
An unofficial Google Gemini SDK for C#

# Features
Feel free to contribute to support other features.
- [ ] Fetching Models
 - [X] GenerateContent
 - [ ] StreamGenerateContent (Partially supported in `GeminiChat` class)
 - [X] File Attachments (Supported files: BMP, GIF, JPEG, PDF, PNG)
 - [X] Function Call
 - [X] Options (Almost everything is supported. See `GeminiSettings` class)
 - [ ] Grounding (Partially supported and untested)
 - [ ] Code Execution
 - [ ] Caching
 - [ ] Tuning
# Setup

To use the library, install the [nuget package](https://www.nuget.org/packages/GoogleGeminiSDK) using the command below
```cli
NuGet\Install-Package GoogleGeminiSDK
```
Currently, the example is using `AIFunctionFactory.Create` from [`Microsoft.Extensions.AI`](https://www.nuget.org/packages/Microsoft.Extensions.AI) to extract function metadata. If you plan using the function call feature, please install [`Microsoft.Extensions.AI`](https://www.nuget.org/packages/Microsoft.Extensions.AI) using the command below
```cli
NuGet\Install-Package Microsoft.Extensions.AI
```
# Get started
This library provides two primary classes: `GeminiChat` and `GeminiChatClient`.

- **`GeminiChat`**: A user-friendly interface for accessing the Gemini API, designed for simplicity and ease of use.
- **`GeminiChatClient`**: A more flexible implementation that leverages `Microsoft.Extensions.AI.Abstraction` for advanced use cases and integration into existing applications.

To setup a working chat conversation with Gemini, you can use the code below as an example.
```cs
[Description("Gets the temperature of the city  using the name of the city")]
static float GetTemperature([Description("Name of the city")] string city) =>
    city.ToLower() == "new york" ? 1f : 28f;

var gemini = new GeminiChat("API_KEY", "MODEL_ID");
var settings = new GeminiSettings();
settings.Functions = [AIFunctionFactory.Create(GetTemperature)];
do
{
    Console.Write("User: ");
    string? userMsg = Console.ReadLine();
    if (userMsg == null)
        continue;

    if (userMsg.ToLower() == "exit")
        break;

    var response = await gemini.SendMessage(userMsg, settings: settings);
    Console.WriteLine("Model: " + response?.Text?.TrimEnd('\n'));
}
while (true);
```
