using DB_Prog.Models;
using DBManagement.Models;

public class TableReservationService
{
    private readonly RestaurantDbContext _db;

    public TableReservationService(RestaurantDbContext db)
    {
        _db = db;
    }

    public (bool success, string message, object data) CreateReservation(TableReservationRequest req)
    {
        // 1. 筛选合适桌台
        var table = _db.TableInfo
            .Where(t => t.Capacity >= req.PartySize && t.Status == "空闲")
            .OrderBy(t => t.Capacity)
            .FirstOrDefault();

        if (table == null)
            return (false, "没有可用桌台", null);
        int maxId = _db.TableReservation.Max(r => (int?)r.ReservationID) ?? 0;
        int newId = maxId + 1;

        // 2. 创建预约项
        var reservation = new TableReservation
        {
            ReservationID = newId,
            // ReservationID 由数据库序列自动生成
            TableID = table.TableId,
            CustomerID = req.CustomerID,
            //CustomerName = req.CustomerName,
            //ContactPhone = req.ContactPhone,
            PartySize = req.PartySize,
            ReservationTime = req.ReservationTime,
            ExpectedDuration = req.ExpectedDuration,
            Status = "已预约",
            Notes = req.Notes,
            CreateTime = DateTime.Now,
            MealTime = req.MealTime
        };

        _db.TableReservation.Add(reservation);
        _db.SaveChanges();

        // 3. 返回桌号和区域
        var result = new
        {
            TableID = table.TableId,
            TableNumber = table.TableNumber,
            Area = table.Area,
            ReservationID = reservation.ReservationID
        };

        return (true, "预约成功", result);
    }
}