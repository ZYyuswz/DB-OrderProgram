using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var details = await _orderService.GetOrderDetailsAsync(id);
            return Ok(details);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            order.OrderTime = DateTime.Now;
            order.OrderStatus = "进行中";
            var orderId = await _orderService.CreateOrderAsync(order);
            return Ok(new { OrderId = orderId });
        }

        [HttpPost("{id}/details")]
        public async Task<IActionResult> AddOrderDetail(int id, [FromBody] OrderDetail detail)
        {
            detail.OrderID = id;
            detail.Subtotal = detail.Quantity * detail.UnitPrice;
            var result = await _orderService.AddOrderDetailAsync(detail);
            if (result)
            {
                await _orderService.UpdateOrderTotalAsync(id);
            }
            return Ok();
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> CheckoutOrder(int id)
        {
            var result = await _orderService.CheckoutOrderAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }
    }
}
