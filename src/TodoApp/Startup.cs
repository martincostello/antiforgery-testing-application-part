// Copyright (c) Martin Costello, 2020. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoApp.Data;
using TodoApp.Services;

namespace TodoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClock>((_) => SystemClock.Instance);
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<ITodoService, TodoService>();

            services.AddControllersWithViews();

            services.AddDbContext<TodoContext>((builder) => builder.UseInMemoryDatabase(databaseName: "TodoApp"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints((endpoints) => endpoints.MapDefaultControllerRoute());
        }
    }
}
