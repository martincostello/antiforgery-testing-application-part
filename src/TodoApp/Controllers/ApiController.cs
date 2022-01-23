// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

public class ApiController : Controller
{
    private readonly ITodoService _service;

    public ApiController(ITodoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("api/items")]
    public async Task<IActionResult> GetItems(CancellationToken cancellationToken = default)
    {
        TodoListViewModel model = await _service.GetListAsync(cancellationToken);
        return Json(model);
    }

    [HttpGet]
    [Route("api/items/{id}", Name = nameof(GetItem))]
    public async Task<IActionResult> GetItem([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        TodoItemModel? model = await _service.GetAsync(id, cancellationToken);

        if (model == null)
        {
            return NotFound();
        }

        return Json(model);
    }

    [HttpPost]
    [Route("api/items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem([FromBody] string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest();
        }

        string id = await _service.AddItemAsync(text, cancellationToken);

        return CreatedAtRoute(nameof(GetItem), new { id }, new { id });
    }

    [HttpPost]
    [Route("api/items/{id}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteItem([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        bool? result = await _service.CompleteItemAsync(id, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        if (!result.Value)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpDelete]
    [Route("api/items/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        if (!await _service.DeleteItemAsync(id, cancellationToken))
        {
            return NotFound();
        }

        return NoContent();
    }
}
