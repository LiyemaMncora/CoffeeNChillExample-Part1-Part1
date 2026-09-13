using CoffeeNChillExample.Models;
using CoffeeNChillExample.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;


namespace CoffeeNChillExample.Functions
{
    public class UpdateMenuItem
    {
        private readonly ILogger<UpdateMenuItem> _logger;

        public UpdateMenuItem(ILogger<UpdateMenuItem> logger)
        {
            _logger = logger;
        }

        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "put",
                Route = "menu/{category}/{id}")]
            HttpRequestData req,
            string category,
            string id)
        {
            try
            {
                if (string.IsNullOrEmpty(category) ||
                    string.IsNullOrEmpty(id))
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "Category and ID are required.");

                    return errorResponse;
                }

                var menuItem =
                    await JsonSerializer.DeserializeAsync<MenuItem>(req.Body);

                if (menuItem == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "Please enter valid menu item information.");

                    return errorResponse;
                }

                if (menuItem.Price < 0)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "Price cannot be negative.");

                    return errorResponse;
                }

                TableService tableService =
                    new TableService(
                        "UseDevelopmentStorage=true",
                        "Menuitems");

                MenuItem? existingItem =
                    await tableService.GetEntityAsync(category, id);

                if (existingItem == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "Menu item was not found.");

                    return errorResponse;
                }

                menuItem.PartitionKey = category;
                menuItem.RowKey = id;
                menuItem.ETag = existingItem.ETag;

                await tableService.UpdateEntityAsync(menuItem);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(menuItem);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating menu item.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while updating the menu item.");

                return response;
            }
        }
    }
}

     
