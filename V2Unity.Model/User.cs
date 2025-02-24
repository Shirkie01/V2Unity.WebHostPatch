namespace V2Unity.Model
{
    public class User : Entity
    {
        public string Name { get; set; } = string.Empty;

        public string? DeviceId { get; set; }        
    }
}
