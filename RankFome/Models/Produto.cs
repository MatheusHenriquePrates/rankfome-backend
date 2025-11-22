// =============================================================================
// Model: Produto
// =============================================================================
// Representa um produto/item disponível para venda em uma loja do RankFome.
// Contém informações de preço, descrição, imagem e disponibilidade.
// Pode receber avaliações dos clientes e ser incluído em pedidos.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace RankFome.Models
{
    /// <summary>
    /// Entidade que representa um produto no catálogo de uma loja.
    /// Armazena informações comerciais e de avaliação do produto.
    /// </summary>
    public class Produto
    {
        /// <summary>
        /// Identificador único do produto (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do produto.
        /// </summary>
        [Required]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do produto (ingredientes, porção, etc.).
        /// </summary>
        [Required]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Preço de venda do produto em reais.
        /// Precisão: 10 dígitos totais, 2 casas decimais.
        /// </summary>
        [Required]
        public decimal Preco { get; set; }

        /// <summary>
        /// URL da imagem do produto.
        /// </summary>
        [Required]
        public string ImagemUrl { get; set; } = string.Empty;

        /// <summary>
        /// Categoria do produto (ex: Lanches, Bebidas, Sobremesas).
        /// </summary>
        [Required]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o produto está disponível para venda.
        /// Permite desativar temporariamente sem excluir o produto.
        /// </summary>
        public bool Disponivel { get; set; } = true;

        // =============================================================================
        // Dados de Avaliação (preparado para implementação futura)
        // =============================================================================

        /// <summary>
        /// Média das avaliações recebidas (escala de 0 a 5).
        /// </summary>
        public double AvaliacaoMedia { get; set; } = 0;

        /// <summary>
        /// Quantidade total de avaliações recebidas.
        /// </summary>
        public int TotalAvaliacoes { get; set; } = 0;

        /// <summary>
        /// Data e hora de criação do registro (UTC).
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // =============================================================================
        // Propriedades de Navegação (Relacionamentos)
        // =============================================================================

        /// <summary>
        /// ID da loja à qual o produto pertence (chave estrangeira).
        /// </summary>
        public int LojaId { get; set; }

        /// <summary>
        /// Propriedade de navegação para a loja proprietária.
        /// Relacionamento: Muitos produtos pertencem a uma loja (N:1).
        /// </summary>
        public Loja Loja { get; set; } = null!;

        /// <summary>
        /// Coleção de itens de pedido que contêm este produto.
        /// Relacionamento: Um produto pode estar em muitos itens de pedido (1:N).
        /// </summary>
        public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
    }
}
