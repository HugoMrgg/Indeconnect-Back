    namespace IndeConnect_Back.Application.DTOs.Locations;

    public record CityDto(
        string Name,
        string Country,
        double Latitude,
        double Longitude
    );