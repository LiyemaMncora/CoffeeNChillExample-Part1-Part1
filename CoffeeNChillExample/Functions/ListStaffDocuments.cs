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

    public class ListStaffDocuments
    {
        private readonly ILogger<ListStaffDocuments> _logger;

        public ListStaffDocuments(ILogger<ListStaffDocuments> logger)
        {
            _logger = logger;
        }

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "documents")]
            HttpRequestData req)
        {
            try
            {
                BlobService blobService =
                    new BlobService(
                        "UseDevelopmentStorage=true",
                        "staff-docs");

                var documents =
                    await blobService.GetAllFilesAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(documents);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing staff documents.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while listing documents.");

                return response;
            }
        }
    }
}

