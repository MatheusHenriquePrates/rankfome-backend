// =============================================================================
// Model: Loja
// =============================================================================
// Representa uma loja/estabelecimento cadastrado no sistema RankFome.
// Contém informações comerciais, endereço e coordenadas geográficas.
// Pertence a um usuário (Vendedor ou Dev) e pode ter múltiplos produtos.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace RankFome.Models
{
    /// <summary>
    /// Entidade que representa uma loja no sistema.
    /// Armazena dados do estabelecimento, endereço completo e geolocalização.
    /// </summary>
    public class Loja
    {
        /// <summary>
        /// Identificador único da loja (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome comercial da loja.
        /// </summary>
        [Required]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição da loja (tipo de comida, especialidades, etc.).
        /// </summary>
        [Required]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// URL da imagem do logotipo da loja.
        /// </summary>
        [Required]
        public string LogoUrl { get; set; } = string.Empty;

        // =============================================================================
        // Endereço da Loja
        // =============================================================================

        /// <summary>
        /// Nome da rua/avenida onde a loja está localizada.
        /// </summary>
        [Required]
        public string Rua { get; set; } = string.Empty;

        /// <summary>
        /// Número do endereço. Pode ser vazio para endereços sem número.
        /// </summary>
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Bairro onde a loja está localizada.
        /// </summary>
        [Required]
        public string Bairro { get; set; } = string.Empty;

        /// <summary>
        /// Cidade onde a loja está localizada.
        /// </summary>
        [Required]
        public string Cidade { get; set; } = string.Empty;

        /// <summary>
        /// Estado/UF onde a loja está localizada.
        /// </summary>
        [Required]
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Informações complementares do endereço (apartamento, bloco, etc.).
        /// Campo opcional.
        /// </summary>
        public string? Complemento { get; set; }

        // =============================================================================
        // Coordenadas Geográficas
        // =============================================================================

        /// <summary>
        /// Latitude da localização da loja para exibição em mapas.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude da localização da loja para exibição em mapas.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Data e hora de criação do registro (UTC).
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // =============================================================================
        // Propriedades de Navegação (Relacionamentos)
        // =============================================================================

        /// <summary>
        /// ID do usuário proprietário da loja (chave estrangeira).
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Propriedade de navegação para o usuário proprietário.
        /// Relacionamento: Muitas lojas pertencem a um usuário (N:1).
        /// </summary>
        public Usuario Usuario { get; set; } = null!;

        /// <summary>
        /// Coleção de produtos oferecidos pela loja.
        /// Relacionamento: Uma loja possui muitos produtos (1:N).
        /// </summary>
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
