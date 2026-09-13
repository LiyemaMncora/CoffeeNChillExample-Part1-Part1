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
    public class DownloadStaffDocument
    {
        private readonly ILogger<DownloadStaffDocument> _logger;

        public DownloadStaffDocument(
            ILogger<DownloadStaffDocument> logger)
        {
            _logger = logger;
        }

        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "documents/download/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "File name is required.");

                    return errorResponse;
                }

                BlobService blobService =
                    new BlobService(
                        "UseDevelopmentStorage=true",
                        "staff-docs");

                var (fileStream, contentType) =
                    await blobService.DownloadFileAsync(fileName);

                if (fileStream == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "File was not found.");

                    return errorResponse;
                }

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                response.Headers.Add(
                    "Content-Type",
                    string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);

                response.Headers.Add(
                    "Content-Disposition",
                    $"attachment; filename=\"{fileName}\"");

                await fileStream.CopyToAsync(response.Body);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error downloading document.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while downloading the document.");

                return response;
            }
        }
    }
}

      
