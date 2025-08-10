using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 菜品分类实体
    /// </summary>
    [Table("CATEGORY")]
    public class Category
    {
        [Key]
        [Column("CATEGORYID")]
        public int CategoryID { get; set; }

        [Required]
        [Column("CATEGORYNAME")]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [Column("SORTORDER")]
        public int SortOrder { get; set; }

        [Column("ISACTIVE")]
        [StringLength(1)]
        public string IsActive { get; set; } = "Y";

        // 导航属性
        public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}