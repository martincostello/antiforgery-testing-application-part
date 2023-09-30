// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Humanizer;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

/// <summary>
/// A class representing the class for managing TODO items. This class cannot be inherited.
/// </summary>
/// <param name="repository">The <see cref="ITodoRepository"/> to use.</param>
public sealed class TodoService(ITodoRepository repository) : ITodoService
{
    /// <inheritdoc />
    public async Task<string> AddItemAsync(string text, CancellationToken cancellationToken)
    {
        TodoItem item = await repository.AddItemAsync(text, cancellationToken);

        return item.Id.ToString();
    }

    /// <inheritdoc />
    public async Task<bool?> CompleteItemAsync(string id, CancellationToken cancellationToken)
    {
        return await repository.CompleteItemAsync(new Guid(id), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteItemAsync(string id, CancellationToken cancellationToken)
    {
        return await repository.DeleteItemAsync(new Guid(id), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken)
    {
        IList<TodoItem> items = await repository.GetItemsAsync(cancellationToken);

        var result = new TodoListViewModel();

        foreach (var todo in items)
        {
            result.Items.Add(MapItem(todo));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TodoItemModel?> GetAsync(string id, CancellationToken cancellationToken)
    {
        TodoItem? item = await repository.GetItemAsync(new Guid(id), cancellationToken);

        if (item is null)
        {
            return null;
        }

        return MapItem(item);
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new TodoItemModel()
        {
            Id = item.Id.ToString(),
            IsCompleted = item.CompletedAt.HasValue,
            LastUpdated = (item.CompletedAt ?? item.CreatedAt).Humanize(),
            Text = item.Text,
        };
    }
}
