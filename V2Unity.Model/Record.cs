namespace V2Unity.Model
{
    public class Record : Entity
    {
        public string? DeviceId { get; set; }
        public string? Name { get; set; }
        public string? Stage { get; set; }
        public Difficulty? Difficulty { get; set; }
        public _VEHICLE Vehicle { get; set; }
        public int TotalEnemiesDestroyed { get; set; } = 0;
    }
}
