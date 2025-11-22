// =============================================================================
// Model: Pedido
// =============================================================================
// Representa um pedido realizado por um cliente no sistema RankFome.
// Contém informações de entrega, pagamento e status do pedido.
// Agrupa múltiplos itens (produtos) em uma única transação.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace RankFome.Models
{
    /// <summary>
    /// Enumeração que define os possíveis estados de um pedido.
    /// Representa o ciclo de vida do pedido desde a criação até a entrega.
    /// </summary>
    public enum StatusPedido
    {
        /// <summary>
        /// Pedido criado, aguardando confirmação da loja.
        /// </summary>
        Pendente = 0,

        /// <summary>
        /// Pedido confirmado e em preparação na cozinha.
        /// </summary>
        Preparando = 1,

        /// <summary>
        /// Pedido saiu para entrega.
        /// </summary>
        ACaminho = 2,

        /// <summary>
        /// Pedido entregue ao cliente com sucesso.
        /// </summary>
        Entregue = 3,

        /// <summary>
        /// Pedido cancelado pelo cliente ou pela loja.
        /// </summary>
        Cancelado = 4
    }

    /// <summary>
    /// Enumeração que define as formas de pagamento aceitas.
    /// </summary>
    public enum FormaPagamento
    {
        /// <summary>
        /// Pagamento em dinheiro na entrega.
        /// </summary>
        Dinheiro = 0,

        /// <summary>
        /// Pagamento com cartão de crédito.
        /// </summary>
        CartaoCredito = 1,

        /// <summary>
        /// Pagamento com cartão de débito.
        /// </summary>
        CartaoDebito = 2,

        /// <summary>
        /// Pagamento via PIX (transferência instantânea).
        /// </summary>
        Pix = 3,

        /// <summary>
        /// Pagamento com vale-refeição ou vale-alimentação.
        /// </summary>
        ValeRefeicao = 4
    }

    /// <summary>
    /// Entidade que representa um pedido no sistema.
    /// Armazena dados da transação, endereço de entrega e itens solicitados.
    /// </summary>
    public class Pedido
    {
        /// <summary>
        /// Identificador único do pedido (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data e hora em que o pedido foi realizado (UTC).
        /// </summary>
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Valor total do pedido em reais.
        /// Soma dos subtotais de todos os itens.
        /// Precisão: 10 dígitos totais, 2 casas decimais.
        /// </summary>
        [Required]
        public decimal ValorTotal { get; set; }

        /// <summary>
        /// Status atual do pedido no ciclo de vida.
        /// </summary>
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;

        /// <summary>
        /// Forma de pagamento escolhida pelo cliente.
        /// </summary>
        public FormaPagamento FormaPagamento { get; set; }

        // =============================================================================
        // Endereço de Entrega
        // =============================================================================

        /// <summary>
        /// Rua do endereço de entrega.
        /// </summary>
        [Required]
        public string EnderecoRua { get; set; } = string.Empty;

        /// <summary>
        /// Número do endereço de entrega.
        /// </summary>
        [Required]
        public string EnderecoNumero { get; set; } = string.Empty;

        /// <summary>
        /// Bairro do endereço de entrega.
        /// </summary>
        [Required]
        public string EnderecoBairro { get; set; } = string.Empty;

        /// <summary>
        /// Cidade do endereço de entrega.
        /// </summary>
        [Required]
        public string EnderecoCidade { get; set; } = string.Empty;

        /// <summary>
        /// Estado/UF do endereço de entrega.
        /// </summary>
        [Required]
        public string EnderecoEstado { get; set; } = string.Empty;

        /// <summary>
        /// Complemento do endereço de entrega (apartamento, bloco, etc.).
        /// Campo opcional.
        /// </summary>
        public string? EnderecoComplemento { get; set; }

        /// <summary>
        /// Observações adicionais do cliente sobre o pedido.
        /// Ex: "Sem cebola", "Troco para R$50", etc.
        /// </summary>
        public string? Observacoes { get; set; }

        // =============================================================================
        // Propriedades de Navegação (Relacionamentos)
        // =============================================================================

        /// <summary>
        /// ID do cliente que realizou o pedido (chave estrangeira).
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Propriedade de navegação para o cliente.
        /// Relacionamento: Muitos pedidos pertencem a um cliente (N:1).
        /// </summary>
        public Usuario Cliente { get; set; } = null!;

        /// <summary>
        /// Coleção de itens incluídos neste pedido.
        /// Relacionamento: Um pedido possui muitos itens (1:N).
        /// </summary>
        public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}
