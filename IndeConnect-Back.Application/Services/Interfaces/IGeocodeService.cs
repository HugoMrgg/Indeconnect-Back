
namespace IndeConnect_Back.Application.Services.Interfaces;
public interface IGeocodeService
{
    Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address);
}