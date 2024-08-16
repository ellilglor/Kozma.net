using Kozma.net.Enums;

namespace Kozma.net.Models;

public class BoxData(double price, BoxCurrency currency, string url, string gif, string page)
{
    public double Price { get; set; } = price;
    public BoxCurrency Currency { get; set; } = currency;
    public string Url { get; set; } = url;
    public string Gif { get; set; } = gif;
    public string Page { get; set; } = page;
}
