using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace OrderService
{
    public class MyOrderCreator : IOrderCreator
    {
        private readonly ILogger<MyOrderCreator> _logger;
        private readonly string _connectionString;

        public MyOrderCreator(ILogger<MyOrderCreator> logger,String connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> Create(OrderDetail orderDetail)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                
                
                
                var orderId = await connection.QuerySingleAsync<int>("CREATE_ORDER",
                    new { userId = 1, userName = orderDetail.User }, transaction,
                    commandType: CommandType.StoredProcedure);
                await connection.ExecuteAsync(".CREATE_ORDER_DETAILS",
                    new
                    {
                        orderId = orderId, productId = orderDetail.ProductId, quantity = orderDetail.Quantity,
                        productName = orderDetail.Name
                    }, transaction, commandType: CommandType.StoredProcedure);
               transaction.Commit();
                return orderId;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                transaction.Rollback();
                return -1;
            }
        }
    }
}