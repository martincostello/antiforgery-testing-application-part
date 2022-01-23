// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Refit;
using Shouldly;
using TodoApp.Models;
using Xunit;
using Xunit.Abstractions;

namespace TodoApp
{
    /// <summary>
    /// A class containing tests for the TodoApp's API.
    /// </summary>
    public class ApiTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The test output helper to use.</param>
        public ApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task Cannot_Create_Todo_Item_With_Html_Form_Without_Csrf_Parameter()
        {
            // Arrange = Create an HTTP client without CSRF configured
            using var httpClient = Fixture.CreateDefaultClient();

            // Create form content to create a new item without the CSRF parameter
            var form = new Dictionary<string, string>()
            {
                ["text"] = "Buy cheese",
            };

            using var content = new FormUrlEncodedContent(form);

            // Act - Create a new item
            using var response = await httpClient.PostAsync("home/additem", content);

            // Assert - The request failed due to missing CSRF tokens
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Cannot_Manage_Todo_Items_With_Json_Api_Without_Csrf_Header()
        {
            // Arrange = Create an HTTP client without CSRF configured
            using var httpClient = Fixture.CreateDefaultClient();
            var client = RestService.For<ITodoClient>(httpClient);

            // Act and Assert - All write operations fail
            using var response = await client.AddAsync("Buy tomatoes");
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            (await Assert.ThrowsAsync<ApiException>(() => client.CompleteAsync("my-id"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

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

        [Fact]
        public async Task Can_Create_Todo_Item_With_Json()
        {
            // Arrange - Get valid CSRF tokens and parameter names from the server
            AntiforgeryTokens tokens = await Fixture.GetAntiforgeryTokensAsync();

            // Configure a handler with the CSRF cookie
            using var cookieHandler = new CookieContainerHandler();
            cookieHandler.Container.Add(
                Fixture.Server.BaseAddress,
                new Cookie(tokens.CookieName, tokens.CookieValue));

            // Create an HTTP client and add the CSRF header
            using var httpClient = Fixture.CreateDefaultClient(cookieHandler);
            httpClient.DefaultRequestHeaders.Add(tokens.HeaderName, tokens.RequestToken);

            var client = RestService.For<ITodoClient>(httpClient);

            // Act - Create a new item
            using var response = await client.AddAsync("Buy bread");

            // Assert - The item was created
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task Can_Manage_Todo_Items_With_Json_Api()
        {
            // Arrange - Create ITodoClient with CSRF cookie and header configured
            ITodoClient client = await CreateTodoClientAsync();

            // Act - Get all the items
            TodoListViewModel items = await client.GetAsync();

            // Assert - There should be no items
            items.ShouldNotBeNull();
            items.Items.ShouldNotBeNull();

            int beforeCount = items.Items.Count;

            // Arrange
            string text = "Buy eggs";

            // Act - Add a new item
            var response = await client.AddAsync(text);

            // Assert - An item was created
            response.EnsureSuccessStatusCode();

            // Arrange - Get the new item's Id
            response.Headers.Location.ShouldNotBeNull();
            string id = response.Headers.Location.Segments.Last();

            // Act - Get the item
            TodoItemModel? item = await client.GetAsync(id);

            // Assert - Validate the item was created correctly
            item.ShouldNotBeNull();
            item.Id.ShouldBe(id);
            item.IsCompleted.ShouldBeFalse();
            item.LastUpdated.ShouldNotBeNull();
            item.Text.ShouldBe(text);

            // Act - Mark the item as being completed
            await client.CompleteAsync(id);

            // Assert - The item was completed
            item = await client.GetAsync(id);

            item.ShouldNotBeNull();
            item.Id.ShouldBe(id);
            item.Text.ShouldBe(text);
            item.IsCompleted.ShouldBeTrue();

            // Act - Get all the items
            items = await client.GetAsync();

            // Assert - The item was completed
            items.ShouldNotBeNull();
            items.Items.ShouldNotBeNull();
            items.Items.Count.ShouldBe(beforeCount + 1);
            item = items.Items.Last();

            item.ShouldNotBeNull();
            item.Id.ShouldBe(id);
            item.Text.ShouldBe(text);
            item.IsCompleted.ShouldBeTrue();
            item.LastUpdated.ShouldNotBeNull();

            // Act - Delete the item
            await client.DeleteAsync(id);

            // Assert - The item no longer exists
            items = await client.GetAsync();

            items.ShouldNotBeNull();
            items.Items.ShouldNotBeNull();
            items.Items.Count.ShouldBe(beforeCount);

            var exception = await Assert.ThrowsAsync<ApiException>(() => client.GetAsync(id));
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Creates an <see cref="ITodoClient"/> to use as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that returns an <see cref="ITodoClient"/>
        /// to use to interact with the application.
        /// </returns>
        private async Task<ITodoClient> CreateTodoClientAsync()
        {
            // Get valid CSRF tokens and parameter names from the server
            AntiforgeryTokens tokens = await Fixture.GetAntiforgeryTokensAsync();

            // Configure a handler with the CSRF cookie
            var cookieHandler = new CookieContainerHandler();
            cookieHandler.Container.Add(
                Fixture.Server.BaseAddress,
                new Cookie(tokens.CookieName, tokens.CookieValue));

            // Create an HTTP client and add the CSRF header
            var httpClient = Fixture.CreateDefaultClient(cookieHandler);

            httpClient.DefaultRequestHeaders.Add(tokens.HeaderName, tokens.RequestToken);

            // Create a client to interact with the API using the CSRF-configured HttpClient
            return RestService.For<ITodoClient>(httpClient);
        }
    }
}
