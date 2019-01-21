namespace LiveDisplay.Servicios.Weather
{
    /// <summary>
    /// This class holds constant values for image resources on the OpenWeatherMap website, it might change over time.
    /// </summary>
    internal class IconsForWeatherConditions
    {
        /*I don't want magical strings, so, each one of these identifies a image that represents a Weather condition, cloudy, sunny, etc.
        for example see: https://openweathermap.org/weather-conditions
        When making the request to the API I use that Icon Id(that comes with the Response)
        to Identify a Local resource that I will use (a hd image of that Weather condition) and send it to the LockScreen as a wallpaper.
        */
        public const string ClearSky = "01d";
        public const string ClearSkyNight = "01n";
        public const string FewClouds = "02d";
        public const string FewCloudsNight = "02n";
        public const string ScatteredClouds = "03d";
        public const string ScatteredCloudsNight = "03n";
        public const string BrokenClouds = "04d";
        public const string BrokenCloudsNight = "04n";
        public const string ShowerRain = "09d";
        public const string ShowerRainNight = "09n";
        public const string Rain = "10d";
        public const string RainNight = "10n";
        public const string Thunderstorm = "11d";
        public const string ThunderstormNight = "11n";
        public const string Snow = "13d";
        public const string SnowNight = "13n";
        public const string Mist = "50d";
        public const string MistNight = "50n";
    }
}