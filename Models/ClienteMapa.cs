namespace MapaClientes.Api.Models;

public class ClienteMapa
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
