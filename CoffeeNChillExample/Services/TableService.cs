using CoffeeNChillExample.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;

namespace CoffeeNChillExample.Services
{
    public class TableService
    {
        private readonly TableClient _tableClient;

        public TableService(string connectionString, string tableName)
        {
            _tableClient = new TableClient(connectionString, tableName);

            _tableClient.CreateIfNotExists();
        }

        public async Task AddEntityAsync(MenuItem newItem)
        {
            await _tableClient.AddEntityAsync(newItem);
        }

        public async Task<List<MenuItem>> GetAllEntitiesAsync()
        {
            var allRecords = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>())
            {
                allRecords.Add(item);
            }

            return allRecords;
        }

        public async Task<List<MenuItem>> GetEntitiesByCategoryAsync(string category)
        {
            var allRecords = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>(
                item => item.PartitionKey == category))
            {
                allRecords.Add(item);
            }

            return allRecords;
        }

        public async Task<MenuItem?> GetEntityAsync(string category, string id)
        {
            try
            {
                MenuItem item = await _tableClient.GetEntityAsync<MenuItem>(
                    category,
                    id);

                return item;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateEntityAsync(MenuItem item)
        {
            await _tableClient.UpdateEntityAsync(
                item,
                item.ETag,
                TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync(string category, string id)
        {
            await _tableClient.DeleteEntityAsync(category, id);
        }
    }
}

    
