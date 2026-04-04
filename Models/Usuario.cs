using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("usuarios")] // tabela no Postgres
public class Usuario
{
    [Key]
    [Column("id")]           // <--- aqui, id minúsculo
    public int Id { get; set; }

    [Required]
    [Column("nome")]         // minúsculo
    public string Nome { get; set; } = "";

    [Required]
    [Column("email")]        // minúsculo
    public string Email { get; set; } = "";

    [Required]
    [Column("senha_hash")]   // minúsculo, com underline
    public string SenhaHash { get; set; } = "";
}