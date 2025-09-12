namespace ConsoleApp1.Models
{
    /// <summary>
    /// 注册请求模型
    /// </summary>
    public class RegisterRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}