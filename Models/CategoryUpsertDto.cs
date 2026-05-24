namespace Accessory_DesktopApp.Models
{
    public class CategoryUpsertDto
    {
        public int? id { get; set; }

        public string? title { get; set; }

        public int parent_id { get; set; }

        // Local file path (when user picks a new image)
        public string? thumbnail_path { get; set; }
    }
}
