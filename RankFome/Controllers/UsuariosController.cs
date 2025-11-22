// =============================================================================
// Controller: UsuariosController
// =============================================================================
// Responsável pelo gerenciamento de autenticação e registro de usuários.
// Endpoints públicos para registro e login com geração de tokens JWT.
// Não requer autenticação prévia para acesso.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RankFome.Data;
using RankFome.Helpers;
using RankFome.Models;

namespace RankFome.Controllers
{
    /// <summary>
    /// Controller responsável pela autenticação e gerenciamento de usuários.
    /// Fornece endpoints para registro de novos usuários e login.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="jwtHelper">Helper para geração de tokens JWT</param>
        public UsuariosController(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        // =============================================================================
        // POST: api/Usuarios/Registro
        // =============================================================================
        /// <summary>
        /// Registra um novo usuário no sistema.
        /// Valida unicidade do CPF/Email e confirmação de senha.
        /// </summary>
        /// <param name="request">Dados do usuário para registro</param>
        /// <returns>Dados do usuário criado e token JWT para autenticação</returns>
        [HttpPost("Registro")]
        public async Task<ActionResult> Registro([FromBody] RegistroRequest request)
        {
            // Verifica se já existe usuário com o mesmo CPF/Email
            if (await _context.Usuarios.AnyAsync(u => u.CpfEmail == request.CpfEmail))
            {
                return BadRequest(new { message = "CPF/Email já cadastrado" });
            }

            // Valida se a senha e confirmação são iguais
            if (request.Senha != request.ConfirmarSenha)
            {
                return BadRequest(new { message = "As senhas não coincidem" });
            }

            // Cria novo usuário com senha criptografada
            var usuario = new Usuario
            {
                Nome = request.Nome,
                Idade = request.Idade,
                Localizacao = request.Localizacao,
                CpfEmail = request.CpfEmail,
                SenhaHash = JwtHelper.HashSenha(request.Senha),
                Tipo = request.Tipo
            };

            // Persiste no banco de dados
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Gera token JWT para autenticação imediata
            var token = _jwtHelper.GerarToken(usuario);

            return Ok(new
            {
                message = "Usuário criado com sucesso",
                usuario = new
                {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    cpfEmail = usuario.CpfEmail,
                    tipo = usuario.Tipo.ToString()
                },
                token
            });
        }

        // =============================================================================
        // POST: api/Usuarios/Login
        // =============================================================================
        /// <summary>
        /// Autentica um usuário existente no sistema.
        /// Valida credenciais e retorna token JWT para acesso aos endpoints protegidos.
        /// </summary>
        /// <param name="request">Credenciais do usuário (CPF/Email e senha)</param>
        /// <returns>Dados do usuário e token JWT em caso de sucesso</returns>
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            // Busca usuário pelo CPF/Email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CpfEmail == request.CpfEmail);

            // Retorna erro genérico para evitar enumeração de usuários
            if (usuario == null)
            {
                return Unauthorized(new { message = "CPF/Email ou senha incorretos" });
            }

            // Verifica se a senha está correta
            if (!JwtHelper.VerificarSenha(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { message = "CPF/Email ou senha incorretos" });
            }

            // Gera token JWT para o usuário autenticado
            var token = _jwtHelper.GerarToken(usuario);

            return Ok(new
            {
                message = "Login realizado com sucesso",
                usuario = new
                {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    cpfEmail = usuario.CpfEmail,
                    tipo = usuario.Tipo.ToString()
                },
                token
            });
        }
    }

    // =============================================================================
    // DTOs (Data Transfer Objects)
    // =============================================================================

    /// <summary>
    /// DTO para requisição de registro de novo usuário.
    /// Contém todos os dados necessários para criar uma conta.
    /// </summary>
    public class RegistroRequest
    {
        /// <summary>Nome completo do usuário</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Idade do usuário em anos</summary>
        public int Idade { get; set; }

        /// <summary>Localização/endereço do usuário</summary>
        public string Localizacao { get; set; } = string.Empty;

        /// <summary>CPF ou Email para identificação única</summary>
        public string CpfEmail { get; set; } = string.Empty;

        /// <summary>Senha do usuário (será criptografada)</summary>
        public string Senha { get; set; } = string.Empty;

        /// <summary>Confirmação da senha para validação</summary>
        public string ConfirmarSenha { get; set; } = string.Empty;

        /// <summary>Tipo de usuário (Cliente, Vendedor ou Dev)</summary>
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente;
    }

    /// <summary>
    /// DTO para requisição de login.
    /// Contém as credenciais necessárias para autenticação.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>CPF ou Email cadastrado</summary>
        public string CpfEmail { get; set; } = string.Empty;

        /// <summary>Senha do usuário</summary>
        public string Senha { get; set; } = string.Empty;
    }
}
