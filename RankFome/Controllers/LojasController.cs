// =============================================================================
// Controller: LojasController
// =============================================================================
// Responsável pelo CRUD completo de lojas no sistema RankFome.
// Endpoints públicos para listagem e consulta de lojas.
// Endpoints protegidos para criação, atualização e exclusão (requer autenticação).
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
    /// Controller responsável pelo gerenciamento de lojas.
    /// Permite operações CRUD com controle de acesso baseado em roles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LojasController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Construtor com injeção do contexto do banco de dados.
        /// </summary>
        /// <param name="context">Contexto do Entity Framework</param>
        public LojasController(AppDbContext context)
        {
            _context = context;
        }

        // =============================================================================
        // GET: api/Lojas
        // =============================================================================
        /// <summary>
        /// Retorna lista de todas as lojas cadastradas.
        /// Endpoint público, não requer autenticação.
        /// </summary>
        /// <returns>Lista de lojas com informações resumidas</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LojaDTO>>> GetLojas()
        {
            // Carrega lojas com relacionamentos necessários
            var lojas = await _context.Lojas
                .Include(l => l.Usuario)
                .Include(l => l.Produtos)
                .ToListAsync();

            // Mapeia entidades para DTOs
            return lojas.Select(l => new LojaDTO
            {
                Id = l.Id,
                Nome = l.Nome,
                Descricao = l.Descricao,
                LogoUrl = l.LogoUrl,
                Rua = l.Rua,
                Numero = l.Numero,
                Bairro = l.Bairro,
                Cidade = l.Cidade,
                Estado = l.Estado,
                Complemento = l.Complemento,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                DataCriacao = l.DataCriacao,
                QuantidadeProdutos = l.Produtos.Count,
                Dono = new DonoDTO
                {
                    Id = l.Usuario.Id,
                    Nome = l.Usuario.Nome
                }
            }).ToList();
        }

        // =============================================================================
        // GET: api/Lojas/5
        // =============================================================================
        /// <summary>
        /// Retorna detalhes de uma loja específica incluindo seus produtos.
        /// Endpoint público, não requer autenticação.
        /// </summary>
        /// <param name="id">ID da loja</param>
        /// <returns>Detalhes completos da loja com lista de produtos</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LojaDetalhesDTO>> GetLoja(int id)
        {
            // Busca loja com relacionamentos
            var loja = await _context.Lojas
                .Include(l => l.Usuario)
                .Include(l => l.Produtos)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loja == null)
                return NotFound();

            // Mapeia para DTO com detalhes completos
            return new LojaDetalhesDTO
            {
                Id = loja.Id,
                Nome = loja.Nome,
                Descricao = loja.Descricao,
                LogoUrl = loja.LogoUrl,
                Rua = loja.Rua,
                Numero = loja.Numero,
                Bairro = loja.Bairro,
                Cidade = loja.Cidade,
                Estado = loja.Estado,
                Complemento = loja.Complemento,
                Latitude = loja.Latitude,
                Longitude = loja.Longitude,
                DataCriacao = loja.DataCriacao,
                Dono = new DonoDTO
                {
                    Id = loja.Usuario.Id,
                    Nome = loja.Usuario.Nome
                },
                Produtos = loja.Produtos.Select(p => new ProdutoSimplificadoDTO
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    Preco = p.Preco,
                    ImagemUrl = p.ImagemUrl,
                    Categoria = p.Categoria,
                    Disponivel = p.Disponivel
                }).ToList()
            };
        }

        // =============================================================================
        // POST: api/Lojas
        // =============================================================================
        /// <summary>
        /// Cria uma nova loja no sistema.
        /// Requer autenticação com role Vendedor ou Dev.
        /// A loja é automaticamente vinculada ao usuário autenticado.
        /// </summary>
        /// <param name="request">Dados da loja a ser criada</param>
        /// <returns>Dados da loja criada com status 201</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpPost]
        public async Task<ActionResult<LojaDTO>> CreateLoja([FromBody] CriarLojaRequest request)
        {
            // Obtém ID do usuário autenticado a partir do token JWT
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Cria nova entidade Loja
            var loja = new Loja
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                LogoUrl = request.LogoUrl,
                Rua = request.Rua,
                Numero = request.Numero ?? "S/N",
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Estado = request.Estado,
                Complemento = request.Complemento,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                UsuarioId = usuarioId
            };

            // Persiste no banco de dados
            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();

            // Retorna loja criada com status 201 Created
            return CreatedAtAction(nameof(GetLoja), new { id = loja.Id }, new LojaDTO
            {
                Id = loja.Id,
                Nome = loja.Nome,
                Descricao = loja.Descricao,
                LogoUrl = loja.LogoUrl,
                Rua = loja.Rua,
                Numero = loja.Numero,
                Bairro = loja.Bairro,
                Cidade = loja.Cidade,
                Estado = loja.Estado,
                Complemento = loja.Complemento,
                Latitude = loja.Latitude,
                Longitude = loja.Longitude,
                DataCriacao = loja.DataCriacao,
                QuantidadeProdutos = 0,
                Dono = new DonoDTO
                {
                    Id = usuarioId,
                    Nome = User.FindFirst(ClaimTypes.Name)?.Value ?? ""
                }
            });
        }

        // =============================================================================
        // PUT: api/Lojas/5
        // =============================================================================
        /// <summary>
        /// Atualiza dados de uma loja existente.
        /// Requer autenticação. Vendedor só pode editar sua própria loja.
        /// Dev pode editar qualquer loja.
        /// </summary>
        /// <param name="id">ID da loja a ser atualizada</param>
        /// <param name="request">Novos dados da loja</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Vendedor,Dev")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoja(int id, [FromBody] AtualizarLojaRequest request)
        {
            var loja = await _context.Lojas.FindAsync(id);

            if (loja == null)
                return NotFound();

            // Obtém dados do usuário autenticado
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Verifica permissão: Dev pode tudo, Vendedor só sua loja
            if (userRole != "Dev" && loja.UsuarioId != usuarioId)
                return Forbid();

            // Atualiza propriedades da loja
            loja.Nome = request.Nome;
            loja.Descricao = request.Descricao;
            loja.LogoUrl = request.LogoUrl;
            loja.Rua = request.Rua;
            loja.Numero = request.Numero ?? "S/N";
            loja.Bairro = request.Bairro;
            loja.Cidade = request.Cidade;
            loja.Estado = request.Estado;
            loja.Complemento = request.Complemento;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Loja atualizada com sucesso" });
        }

        // =============================================================================
        // DELETE: api/Lojas/5
        // =============================================================================
        /// <summary>
        /// Remove uma loja do sistema.
        /// Requer autenticação com role Dev (apenas administradores).
        /// Remove também todos os produtos associados (cascade).
        /// </summary>
        /// <param name="id">ID da loja a ser removida</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [Authorize(Roles = "Dev")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoja(int id)
        {
            var loja = await _context.Lojas.FindAsync(id);

            if (loja == null)
                return NotFound();

            // Remove loja (produtos são removidos em cascade)
            _context.Lojas.Remove(loja);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Loja deletada com sucesso" });
        }
    }

    // =============================================================================
    // DTOs (Data Transfer Objects)
    // =============================================================================

    /// <summary>
    /// DTO para retorno de lista de lojas (informações resumidas).
    /// </summary>
    public class LojaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DataCriacao { get; set; }
        public int QuantidadeProdutos { get; set; }
        public DonoDTO Dono { get; set; } = null!;
    }

    /// <summary>
    /// DTO para retorno de detalhes de uma loja (inclui lista de produtos).
    /// </summary>
    public class LojaDetalhesDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DataCriacao { get; set; }
        public DonoDTO Dono { get; set; } = null!;
        public List<ProdutoSimplificadoDTO> Produtos { get; set; } = new();
    }

    /// <summary>
    /// DTO para informações do proprietário da loja.
    /// </summary>
    public class DonoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para informações simplificadas de produto (usado na listagem de lojas).
    /// </summary>
    public class ProdutoSimplificadoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string ImagemUrl { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public bool Disponivel { get; set; }
    }

    /// <summary>
    /// DTO para requisição de criação de loja.
    /// </summary>
    public class CriarLojaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// DTO para requisição de atualização de loja.
    /// </summary>
    public class AtualizarLojaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Complemento { get; set; }
    }
}
