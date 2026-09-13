using CoffeeNChillExample.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace CoffeeNChillExample.Functions
{
    public class DeleteMenuItem
    {
        private readonly ILogger<DeleteMenuItem> _logger;

        public DeleteMenuItem(ILogger<DeleteMenuItem> logger)
        {
            _logger = logger;
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
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

                TableService tableService =
                    new TableService(
                        "UseDevelopmentStorage=true",
                        "Menuitems");

                var existingItem =
                    await tableService.GetEntityAsync(category, id);

                if (existingItem == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "Menu item was not found.");

                    return errorResponse;
                }

                await tableService.DeleteEntityAsync(category, id);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Menu item deleted successfully.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting menu item.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while deleting the menu item.");

                return response;
            }
        }
    }
}

      
