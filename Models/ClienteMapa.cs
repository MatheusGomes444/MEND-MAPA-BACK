namespace MapaClientes.Api.Models;

public class ClienteMapa
{
    public int Id { get; set; }
    public string Endereco { get; set; }
    public string Cliente { get; set; } // Nome do cliente final
    public string Posto { get; set; }   // Ex: Torre Norte, Repetidora X
    public string Equipamento { get; set; } // Ex: Hytera HP5, Motorola, etc.
    
    // Importante para o mapa funcionar depois
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string? NomeArquivo { get; set; }
    public string? CaminhoArquivo { get; set; }
}
