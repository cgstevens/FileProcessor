namespace SharedLibrary.Models
{
    public class LocationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FileSettingsModel FileSettings { get; set; }

        public LocationModel()
        {
        }

        public LocationModel(int id, string name) : this()
        {
            Id = id;
            Name = name;
            FileSettings = new FileSettingsModel();
        }

        public LocationModel(int id, string name, FileSettingsModel fileSettings) : this(id, name)
        {
            FileSettings = fileSettings;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return $"{Name} ({Id})";
            }

            return Id.ToString();
        }
    }
}
