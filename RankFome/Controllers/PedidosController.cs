// =============================================================================
// Controller: PedidosController
// =============================================================================
// Responsável pelo gerenciamento de pedidos no sistema RankFome.
// Todos os endpoints requerem autenticação.
// Clientes podem criar pedidos e visualizar apenas os seus.
// Vendedores e Devs podem atualizar status de pedidos.
// Apenas Devs podem deletar pedidos.
// =============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RankFome.Data;
using RankFome.Models;
using System.Security.Claims;

namespace RankFome.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de pedidos.
    /// Todos os endpoints requerem autenticação JWT.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Construtor com injeção do contexto do banco de dados.
        /// </summary>
        /// <param name="context">Contexto do Entity Framework</param>
        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // =============================================================================
        // GET: api/Pedidos
        // =============================================================================
        /// <summary>
        /// Retorna lista de pedidos baseada no perfil do usuário.
        /// Dev: retorna todos os pedidos do sistema.
        /// Cliente/Vendedor: retorna apenas seus próprios pedidos.
        /// </summary>
        /// <returns>Lista de pedidos com itens e informações do cliente</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Dev tem acesso a todos os pedidos
            if (userRole == "Dev")
            {
                return await _context.Pedidos
                    .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                    .Include(p => p.Cliente)
                    .ToListAsync();
            }

            // Demais usuários veem apenas seus pedidos
            return await _context.Pedidos
                .Where(p => p.ClienteId == usuarioId)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Include(p => p.Cliente)
                .ToListAsync();
        }

        // =============================================================================
        // GET: api/Pedidos/5
        // =============================================================================
        /// <summary>
        /// Retorna detalhes de um pedido específico.
        /// Dev pode ver qualquer pedido.
        /// Cliente só pode ver seus próprios pedidos.
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <returns>Dados completos do pedido com itens</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Busca pedido com todos os relacionamentos
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            // Verifica permissão: Dev pode ver tudo, demais só seus pedidos
            if (userRole != "Dev" && pedido.ClienteId != usuarioId)
                return Forbid();

            return pedido;
        }

        // =============================================================================
        // POST: api/Pedidos
        // =============================================================================
        /// <summary>
        /// Cria um novo pedido no sistema.
        /// O pedido é automaticamente vinculado ao usuário autenticado.
        /// Valida existência de todos os produtos antes de criar.
        /// </summary>
        /// <param name="request">Dados do pedido incluindo itens e endereço de entrega</param>
        /// <returns>Dados do pedido criado com status 201</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Pedido>> CreatePedido([FromBody] CriarPedidoRequest request)
        {
            // Obtém ID do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Cria novo pedido com dados básicos
            var pedido = new Pedido
            {
                ClienteId = usuarioId,
                ValorTotal = request.ValorTotal,
                Status = StatusPedido.Pendente,
                FormaPagamento = request.FormaPagamento,
                EnderecoRua = request.EnderecoRua,
                EnderecoNumero = request.EnderecoNumero,
                EnderecoBairro = request.EnderecoBairro,
                EnderecoCidade = request.EnderecoCidade,
                EnderecoEstado = request.EnderecoEstado,
                EnderecoComplemento = request.EnderecoComplemento,
                Observacoes = request.Observacoes
            };

            // Processa cada item do pedido
            foreach (var item in request.Itens)
            {
                // Valida existência do produto
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                if (produto == null)
                    return BadRequest(new { message = $"Produto {item.ProdutoId} não encontrado" });

                // Adiciona item ao pedido com preço atual do produto
                pedido.Itens.Add(new ItemPedido
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco
                });
            }

            // Persiste pedido no banco de dados
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
        }

        // =============================================================================
        // PUT: api/Pedidos/5/Status
        // =============================================================================
        /// <summary>
        /// Atualiza o status de um pedido.
        /// Requer autenticação com role Vendedor ou Dev.
        /// Usado para acompanhar o ciclo de vida do pedido.
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <param name="request">Novo status do pedido</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound();

            // Atualiza status do pedido
            pedido.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status atualizado com sucesso" });
        }

        // =============================================================================
        // DELETE: api/Pedidos/5
        // =============================================================================
        /// <summary>
        /// Remove um pedido do sistema.
        /// Requer autenticação com role Dev (apenas administradores).
        /// Operação irreversível - usar com cautela.
        /// </summary>
        /// <param name="id">ID do pedido a ser removido</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Dev")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound();

            // Remove pedido do banco de dados
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido deletado com sucesso" });
        }
    }

    // =============================================================================
    // DTOs (Data Transfer Objects)
    // =============================================================================

    /// <summary>
    /// DTO para requisição de criação de pedido.
    /// Contém todos os dados necessários para criar um pedido completo.
    /// </summary>
    public class CriarPedidoRequest
    {
        /// <summary>Valor total do pedido em reais</summary>
        public decimal ValorTotal { get; set; }

        /// <summary>Forma de pagamento escolhida</summary>
        public FormaPagamento FormaPagamento { get; set; }

        /// <summary>Rua do endereço de entrega</summary>
        public string EnderecoRua { get; set; } = string.Empty;

        /// <summary>Número do endereço de entrega</summary>
        public string EnderecoNumero { get; set; } = string.Empty;

        /// <summary>Bairro do endereço de entrega</summary>
        public string EnderecoBairro { get; set; } = string.Empty;

        /// <summary>Cidade do endereço de entrega</summary>
        public string EnderecoCidade { get; set; } = string.Empty;

        /// <summary>Estado do endereço de entrega</summary>
        public string EnderecoEstado { get; set; } = string.Empty;

        /// <summary>Complemento do endereço (opcional)</summary>
        public string? EnderecoComplemento { get; set; }

        /// <summary>Observações adicionais do cliente (opcional)</summary>
        public string? Observacoes { get; set; }

        /// <summary>Lista de itens do pedido</summary>
        public List<ItemPedidoRequest> Itens { get; set; } = new();
    }

    /// <summary>
    /// DTO para item individual do pedido.
    /// </summary>
    public class ItemPedidoRequest
    {
        /// <summary>ID do produto a ser adicionado</summary>
        public int ProdutoId { get; set; }

        /// <summary>Quantidade do produto</summary>
        public int Quantidade { get; set; }
    }

    /// <summary>
    /// DTO para atualização de status do pedido.
    /// </summary>
    public class UpdateStatusRequest
    {
        /// <summary>Novo status do pedido</summary>
        public StatusPedido Status { get; set; }
    }
}
