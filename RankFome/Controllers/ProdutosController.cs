// =============================================================================
// Controller: ProdutosController
// =============================================================================
// Responsável pelo CRUD completo de produtos no sistema RankFome.
// Endpoints públicos para listagem e consulta de produtos.
// Endpoints protegidos para criação, atualização e exclusão (requer autenticação).
// Vendedores só podem gerenciar produtos de suas próprias lojas.
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
    /// Controller responsável pelo gerenciamento de produtos.
    /// Permite operações CRUD com controle de acesso baseado em roles e propriedade da loja.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Construtor com injeção do contexto do banco de dados.
        /// </summary>
        /// <param name="context">Contexto do Entity Framework</param>
        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        // =============================================================================
        // GET: api/Produtos
        // =============================================================================
        /// <summary>
        /// Retorna lista de todos os produtos cadastrados.
        /// Endpoint público, não requer autenticação.
        /// </summary>
        /// <returns>Lista de produtos com informações da loja</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos
                .Include(p => p.Loja)
                .ToListAsync();
        }

        // =============================================================================
        // GET: api/Produtos/5
        // =============================================================================
        /// <summary>
        /// Retorna detalhes de um produto específico.
        /// Endpoint público, não requer autenticação.
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Dados completos do produto</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            return produto;
        }

        // =============================================================================
        // GET: api/Produtos/Loja/5
        // =============================================================================
        /// <summary>
        /// Retorna todos os produtos de uma loja específica.
        /// Endpoint público, não requer autenticação.
        /// Útil para exibir o cardápio de uma loja.
        /// </summary>
        /// <param name="lojaId">ID da loja</param>
        /// <returns>Lista de produtos da loja</returns>
        [HttpGet("Loja/{lojaId}")]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutosByLoja(int lojaId)
        {
            var produtos = await _context.Produtos
                .Where(p => p.LojaId == lojaId)
                .ToListAsync();

            return Ok(produtos);
        }

        // =============================================================================
        // POST: api/Produtos
        // =============================================================================
        /// <summary>
        /// Cria um novo produto no sistema.
        /// Requer autenticação com role Vendedor ou Dev.
        /// Vendedor só pode criar produtos em suas próprias lojas.
        /// </summary>
        /// <param name="request">Dados do produto a ser criado</param>
        /// <returns>Dados do produto criado com status 201</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpPost]
        public async Task<ActionResult<Produto>> CreateProduto([FromBody] CriarProdutoRequest request)
        {
            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Verifica se a loja existe
            var loja = await _context.Lojas.FindAsync(request.LojaId);
            if (loja == null)
                return BadRequest(new { message = "Loja não encontrada" });

            // Verifica permissão: Dev pode tudo, Vendedor só sua loja
            if (userRole != "Dev" && loja.UsuarioId != usuarioId)
                return Forbid();

            // Cria novo produto
            var produto = new Produto
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                Preco = request.Preco,
                ImagemUrl = request.ImagemUrl,
                Categoria = request.Categoria,
                Disponivel = request.Disponivel,
                LojaId = request.LojaId
            };

            // Persiste no banco de dados
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }

        // =============================================================================
        // PUT: api/Produtos/5
        // =============================================================================
        /// <summary>
        /// Atualiza dados de um produto existente.
        /// Requer autenticação. Vendedor só pode editar produtos de sua loja.
        /// Dev pode editar qualquer produto.
        /// </summary>
        /// <param name="id">ID do produto a ser atualizado</param>
        /// <param name="request">Novos dados do produto</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduto(int id, [FromBody] AtualizarProdutoRequest request)
        {
            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Busca produto com relacionamento da loja para verificar proprietário
            var produto = await _context.Produtos
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            // Verifica permissão: Dev pode tudo, Vendedor só sua loja
            if (userRole != "Dev" && produto.Loja.UsuarioId != usuarioId)
                return Forbid();

            // Atualiza propriedades do produto
            produto.Nome = request.Nome;
            produto.Descricao = request.Descricao;
            produto.Preco = request.Preco;
            produto.ImagemUrl = request.ImagemUrl;
            produto.Categoria = request.Categoria;
            produto.Disponivel = request.Disponivel;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Produto atualizado com sucesso" });
        }

        // =============================================================================
        // DELETE: api/Produtos/5
        // =============================================================================
        /// <summary>
        /// Remove um produto do sistema.
        /// Requer autenticação. Vendedor só pode deletar produtos de sua loja.
        /// Dev pode deletar qualquer produto.
        /// </summary>
        /// <param name="id">ID do produto a ser removido</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Busca produto com relacionamento da loja para verificar proprietário
            var produto = await _context.Produtos
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                return NotFound();

            // Verifica permissão: Dev pode tudo, Vendedor só sua loja
            if (userRole != "Dev" && produto.Loja.UsuarioId != usuarioId)
                return Forbid();

            // Remove produto do banco de dados
            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produto deletado com sucesso" });
        }
    }

    // =============================================================================
    // DTOs (Data Transfer Objects)
    // =============================================================================

    /// <summary>
    /// DTO para requisição de criação de produto.
    /// </summary>
    public class CriarProdutoRequest
    {
        /// <summary>Nome do produto</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição detalhada do produto</summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>Preço de venda em reais</summary>
        public decimal Preco { get; set; }

        /// <summary>URL da imagem do produto</summary>
        public string ImagemUrl { get; set; } = string.Empty;

        /// <summary>Categoria do produto (ex: Lanches, Bebidas)</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Indica se o produto está disponível para venda</summary>
        public bool Disponivel { get; set; } = true;

        /// <summary>ID da loja à qual o produto pertence</summary>
        public int LojaId { get; set; }
    }

    /// <summary>
    /// DTO para requisição de atualização de produto.
    /// </summary>
    public class AtualizarProdutoRequest
    {
        /// <summary>Nome do produto</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição detalhada do produto</summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>Preço de venda em reais</summary>
        public decimal Preco { get; set; }

        /// <summary>URL da imagem do produto</summary>
        public string ImagemUrl { get; set; } = string.Empty;

        /// <summary>Categoria do produto</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Indica se o produto está disponível para venda</summary>
        public bool Disponivel { get; set; }
    }
}
