// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Refit;
using TodoApp.Models;

namespace TodoApp
{
    [Headers("Content-Type: application/json")]
    public interface ITodoClient
    {
        [Post("/api/items")]
        Task<HttpResponseMessage> AddAsync([Body(BodySerializationMethod.Serialized)] string text, CancellationToken cancellationToken = default);

        [Post("/api/items/{id}/complete")]
        Task CompleteAsync(string id, CancellationToken cancellationToken = default);

        [Delete("/api/items/{id}")]
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        [Get("/api/items")]
        Task<TodoListViewModel> GetAsync(CancellationToken cancellationToken = default);

        [Get("/api/items/{id}")]
        Task<TodoItemModel?> GetAsync(string id, CancellationToken cancellationToken = default);
    }
}
