using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities
{
    public class AdminModel : BaseModel
    {
        [Key]
        public int admin_id { get; set; }
        public Guid Guid { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? email { get; set; }
    }
}
