namespace Clicker.Domain
{
    public sealed class WeatherForecast
    {
        public int Temperature { get; }
        public string Unit { get; }
        public string Summary { get; }
        public string Icon { get; }

        public WeatherForecast(int temperature, string unit, string summary, string icon)
        {
            Temperature = temperature;
            Unit = unit;
            Summary = summary;
            Icon = icon;
        }
    }

    public sealed class Breed
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public Breed(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
