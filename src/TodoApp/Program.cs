// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#pragma warning disable CA1852

using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoApp.Data;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock>((_) => SystemClock.Instance);

builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TodoContext>((builder) => builder.UseInMemoryDatabase(databaseName: "TodoApp"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapDefaultControllerRoute();

app.Run();

namespace TodoApp
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
