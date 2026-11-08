using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace RestaurantLab.Services
{
    public class OrderHistoryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int OriginalOrderId { get; set; }
        public int EmployeeId { get; set; }
        public int TableId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "paid";

        public List<OrderItemEmbed> Items { get; set; } = new();
        public PaymentEmbed Payment { get; set; } = new();
    }

    public class OrderItemEmbed
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PaymentEmbed
    {
        public int MethodId { get; set; }
        public string MethodName { get; set; } = "";
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class MongoOrderService
    {
        private readonly IMongoCollection<OrderHistoryDocument> _ordersCollection;

        public MongoOrderService(string connectionString = "mongodb://127.0.0.1:27017")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("RestaurantDb_NoSQL");
            _ordersCollection = database.GetCollection<OrderHistoryDocument>("order_history");
        }

        public async Task InsertOrderAsync(OrderHistoryDocument order)
        {
            await _ordersCollection.InsertOneAsync(order);
        }

        public async Task<OrderHistoryDocument?> GetOrderByIdAsync(int originalOrderId)
        {
            return await _ordersCollection
                .Find(o => o.OriginalOrderId == originalOrderId)
                .FirstOrDefaultAsync();
        }
    }
}