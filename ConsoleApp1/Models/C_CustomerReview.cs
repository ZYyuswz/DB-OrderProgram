namespace ConsoleApp1.Models
{
    /// <summary>
    /// �ͻ����۱�ʵ����
    /// </summary>
    public class CustomerReview
    {
        public int ReviewID { get; set; }
        public int CustomerID { get; set; }
        public int OrderID { get; set; }
        public int StoreID { get; set; }
        public int OverallRating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewTime { get; set; }
        public string Status { get; set; } = "�����";

        // �������ԣ�������ʾ������Ϣ��
        public string? CustomerName { get; set; }
        public string? StoreName { get; set; }
        public string? OrderTime { get; set; }
    }
}
