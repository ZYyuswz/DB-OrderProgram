using DB_Prog.Models;
using DBManagement.Models;
using DBManagement.Service;
using DBManagement.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DBManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableReservationController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly TableReservationService _service;

        public TableReservationController(RestaurantDbContext db)
        {
            _db = db;
            _service = new TableReservationService(db);
        }

        [HttpPost]
        public ActionResult<ApiResponse<object>> ReserveTable([FromBody] TableReservationRequest req)
        {
            var (success, message, data) = _service.CreateReservation(req);
            return new ApiResponse<object>(success, message, data);
        }
    }
}