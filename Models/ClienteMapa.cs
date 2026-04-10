using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapaClientes.Api.Models
{
    [Table("clientes")]
    public class ClienteMapa
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("endereco")]
        public string Endereco { get; set; } = string.Empty;

        [Required]
        [Column("cliente")]
        public string Cliente { get; set; } = string.Empty;

        [Required]
        [Column("posto")]
        public string Posto { get; set; } = string.Empty;

        [Required]
        [Column("equipamento")]
        public string Equipamento { get; set; } = string.Empty;

        [Column("contrato")]
        public string? Contrato { get; set; }

        [Column("radios")]
        public int? Radios { get; set; }

        [Column("observacao")]
        public string? Observacao { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; } = 0.0;

        [Column("longitude")]
        public double Longitude { get; set; } = 0.0;

        [Column("nomearquivo")]
        public string? NomeArquivo { get; set; }

        [Column("caminhoarquivo")]
        public string? CaminhoArquivo { get; set; }

        [Column("expiraem")]
        public DateTime? ExpiraEm { get; set; }
    }
}