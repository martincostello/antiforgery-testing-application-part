# ASP.NET Core Application Part For Testing Anti-forgery Protected Resources

[![Build status](https://github.com/martincostello/antiforgery-testing-application-part/workflows/build/badge.svg?branch=main&event=push)](https://github.com/martincostello/antiforgery-testing-application-part/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush) [![Open in Visual Studio Code](https://open.vscode.dev/badges/open-in-vscode.svg)](https://open.vscode.dev/martincostello/antiforgery-testing-application-part)

## Introduction

An example application that demonstrates using [ASP.NET Core Application Parts](https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts "Share controllers, views, Razor Pages and more with Application Parts") for easier integration testing of HTTP resources that are protected by the [anti-forgery](https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery "Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core") features of ASP.NET Core, such as `[ValidateAntiforgeryToken]`.

## How It Works

To avoid the danger of having unsafe code that returns valid CSRF tokens in the application itself, instead we can use Application Parts to inject additional functionality into the server at runtime when running integration tests using [`WebApplicationFactory<TEntryPoint>`](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests "Integration tests in ASP.NET Core").

The test project contains the [`AntiforgeryTokenController`](https://github.com/martincostello/antiforgery-testing-application-part/blob/main/tests/TodoApp.Tests/AntiforgeryTokenController.cs). This contains an [HTTP GET resource](https://github.com/martincostello/antiforgery-testing-application-part/blob/f8985fe1bbaa800cf73bc62bb85949c1c0a8a698/tests/TodoApp.Tests/AntiforgeryTokenController.cs#L39-L65) that uses the Antiforgery features to return a JSON payload containing valid CSRF tokens and the relevant cookie/form/header names to validate requests:

```csharp
public IActionResult GetAntiforgeryTokens(
    [FromServices] IAntiforgery antiforgery,
    [FromServices] IOptions<AntiforgeryOptions> options)
{
    AntiforgeryTokenSet tokens = antiforgery.GetTokens(HttpContext);

    var model = new AntiforgeryTokens()
    {
        CookieName = options.Value.Cookie.Name,
        CookieValue = tokens.CookieToken,
        FormFieldName = options.Value.FormFieldName,
        HeaderName = tokens.HeaderName,
        RequestToken = tokens.RequestToken,
    };

    return Json(model);
}
```

This is then configured as an Application Part by the [`ConfigureAntiforgeryTokenResource()`](https://github.com/martincostello/antiforgery-testing-application-part/blob/f8985fe1bbaa800cf73bc62bb85949c1c0a8a698/tests/TodoApp.Tests/IWebHostBuilderExtensions.cs#L26-L39) method, which is [registered with the test server](https://github.com/martincostello/antiforgery-testing-application-part/blob/f8985fe1bbaa800cf73bc62bb85949c1c0a8a698/tests/TodoApp.Tests/TestServerFixture.cs#L82):

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureAntiforgeryTokenResource();
}
```

This then allows tests to use the [`GetAntiforgeryTokensAsync()`](https://github.com/martincostello/antiforgery-testing-application-part/blob/f8985fe1bbaa800cf73bc62bb85949c1c0a8a698/tests/TodoApp.Tests/TestServerFixture.cs#L52-L64) to perform an HTTP GET to the application to obtain valid CSRF tokens to use:

```csharp
public async Task<AntiforgeryTokens> GetAntiforgeryTokensAsync()
{
    using var httpClient = CreateClient();
    using var response = await httpClient.GetAsync(AntiforgeryTokenController.GetTokensUri);
    return JsonSerializer.Deserialize<AntiforgeryTokens>(await response.Content.ReadAsStringAsync());
}
```

The tests then use this to configure an `HttpClient` with CSRF tokens so that HTTP POST/DELETE etc. requests to the application pass the checks by the antiforgery protections.

```csharp
[Fact]
public async Task Can_Create_Todo_Item_With_Html_Form()
{
    // Arrange - Get valid CSRF tokens and parameter names from the server
    AntiforgeryTokens tokens = await Fixture.GetAntiforgeryTokensAsync();

    // Configure a handler with the CSRF cookie
    using var cookieHandler = new CookieContainerHandler();
    cookieHandler.Container.Add(
        Fixture.Server.BaseAddress,
        new Cookie(tokens.CookieName, tokens.CookieValue));

    // Create an HTTP client and add the CSRF cookie
    using var httpClient = Fixture.CreateDefaultClient(cookieHandler);

    // Create form content to create a new item with the CSRF parameter added
    var form = new Dictionary<string, string>()
    {
        [tokens.FormFieldName] = tokens.RequestToken,
        ["text"] = "Buy milk",
    };

    // Act - Create a new item
    using var content = new FormUrlEncodedContent(form);
    using var response = await httpClient.PostAsync("home/additem", content);

    // Assert - The item was created
    response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
}
```

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub](https://github.com/martincostello/antiforgery-testing-application-part/issues "Issues for this project on GitHub.com").

## Repository

The repository is hosted in [GitHub](https://github.com/martincostello/antiforgery-testing-application-part "This project on GitHub.com"): https://github.com/martincostello/antiforgery-testing-application-part.git

## License

This project is licensed under the [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license") license.

## Building and Testing

Compiling the application yourself requires Git and the [.NET SDK](https://www.microsoft.com/net/download/core "Download the .NET SDK") to be installed.

To build and test the application locally from a terminal/command-line, run the following set of commands:

```powershell
git clone https://github.com/martincostello/antiforgery-testing-application-part.git
cd antiforgery-testing-application-part
./build.ps1
```
