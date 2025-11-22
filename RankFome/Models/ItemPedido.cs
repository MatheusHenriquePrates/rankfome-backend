namespace RankFome.Models
{
    /// <summary>
    /// Representa um item individual dentro de um pedido
    /// Entidade de relacionamento muitos-para-muitos entre Pedido e Produto
    /// Armazena informações específicas do produto no momento do pedido
    /// </summary>
    public class ItemPedido
    {
        /// <summary>
        /// Identificador único do item do pedido
        /// Chave primária gerada automaticamente pelo banco de dados
        /// </summary>
        public int Id { get; set; }

        // ==================================================
        // Relacionamento com Pedido
        // ==================================================

        /// <summary>
        /// ID do pedido ao qual este item pertence
        /// Chave estrangeira para a tabela Pedidos
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// Propriedade de navegação para o pedido
        /// Permite acesso ao pedido completo através do relacionamento
        /// Nullable para evitar problemas de serialização e lazy loading
        /// </summary>
        public Pedido? Pedido { get; set; }

        // ==================================================
        // Relacionamento com Produto
        // ==================================================

        /// <summary>
        /// ID do produto incluído neste item do pedido
        /// Chave estrangeira para a tabela Produtos
        /// </summary>
        public int ProdutoId { get; set; }

        /// <summary>
        /// Propriedade de navegação para o produto
        /// Permite acesso aos dados do produto através do relacionamento
        /// Nullable para evitar problemas de serialização e lazy loading
        /// </summary>
        public Produto? Produto { get; set; }

        // ==================================================
        // Dados do Item
        // ==================================================

        /// <summary>
        /// Quantidade do produto solicitada neste item
        /// Valor sempre positivo (validação deve ser feita no controller)
        /// </summary>
        public int Quantidade { get; set; }

        /// <summary>
        /// Preço unitário do produto no momento da criação do pedido
        /// Armazenado para manter histórico mesmo se o preço do produto mudar posteriormente
        /// Precisão configurada no DbContext: 10 dígitos totais, 2 casas decimais
        /// </summary>
        public decimal PrecoUnitario { get; set; }

        /// <summary>
        /// Valor total deste item (PrecoUnitario * Quantidade)
        /// Calculado no momento da criação do pedido
        /// Precisão configurada no DbContext: 10 dígitos totais, 2 casas decimais
        /// </summary>
        public decimal Subtotal { get; set; }
    }
}
