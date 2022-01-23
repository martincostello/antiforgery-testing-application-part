// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TodoApp;

/// <summary>
/// A class representing a factory for creating instances of the application.
/// </summary>
public class TestServerFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerFixture"/> class.
    /// </summary>
    public TestServerFixture()
        : base()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
        ClientOptions.HandleCookies = true;
    }

    /// <inheritdoc/>
    public ITestOutputHelper? OutputHelper { get; set; }

    /// <summary>
    /// Gets a set of valid antiforgery tokens for the application as an asynchronous operation.
    /// </summary>
    /// <param name="httpClientFactory">
    /// An optional delegate to a method to provide an <see cref="HttpClient"/> to use to obtain the response.
    /// </param>
    /// <param name="cancellationToken">
    /// The optional <see cref="CancellationToken"/> to use for the HTTP request to obtain the response.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get a set of valid
    /// antiforgery (CSRF/XSRF) tokens to use for HTTP POST requests to the test server.
    /// </returns>
    public async Task<AntiforgeryTokens> GetAntiforgeryTokensAsync(
        Func<HttpClient>? httpClientFactory = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory?.Invoke() ?? CreateClient();

        var tokens = await httpClient
            .GetFromJsonAsync<AntiforgeryTokens>(AntiforgeryTokenController.GetTokensUri, cancellationToken)
            .ConfigureAwait(false);

        return tokens!;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAntiforgeryTokenResource()
               .ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this));
    }
}
