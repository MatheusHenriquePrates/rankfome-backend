// =============================================================================
// Helper: JwtHelper
// =============================================================================
// Classe utilitária para operações de autenticação JWT e criptografia de senhas.
// Responsável por gerar tokens JWT para usuários autenticados e
// gerenciar hash de senhas usando algoritmo SHA256.
// =============================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RankFome.Models;

namespace RankFome.Helpers
{
    /// <summary>
    /// Helper para geração de tokens JWT e gerenciamento de senhas.
    /// Configurado via injeção de dependência com valores do appsettings.json.
    /// </summary>
    public class JwtHelper
    {
        // Configurações do JWT obtidas do arquivo de configuração
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        /// <summary>
        /// Construtor que obtém configurações JWT do appsettings.json.
        /// Usa valores padrão caso as configurações não estejam definidas.
        /// </summary>
        /// <param name="configuration">Configuração da aplicação</param>
        public JwtHelper(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"] ?? "SUA_CHAVE_SECRETA_SUPER_SEGURA_AQUI_12345";
            _issuer = configuration["Jwt:Issuer"] ?? "RankFome";
            _audience = configuration["Jwt:Audience"] ?? "RankFomeApp";
        }

        // =============================================================================
        // Geração de Token JWT
        // =============================================================================

        /// <summary>
        /// Gera um token JWT para o usuário autenticado.
        /// O token inclui claims de identificação, nome, email e role.
        /// Válido por 7 dias a partir da geração.
        /// </summary>
        /// <param name="usuario">Usuário para o qual o token será gerado</param>
        /// <returns>Token JWT como string</returns>
        public string GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // Define os claims (informações) que serão incluídos no token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // ID único do usuário para identificação
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    // Nome do usuário para exibição
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    // CPF/Email para referência
                    new Claim(ClaimTypes.Email, usuario.CpfEmail),
                    // Tipo/Role do usuário para controle de acesso
                    new Claim(ClaimTypes.Role, usuario.Tipo.ToString())
                }),
                // Token expira em 7 dias
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _issuer,
                Audience = _audience,
                // Assina o token com algoritmo HMAC SHA256
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            // Cria e retorna o token serializado
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // =============================================================================
        // Gerenciamento de Senhas
        // =============================================================================

        /// <summary>
        /// Gera hash SHA256 de uma senha em texto plano.
        /// Usado para armazenar senhas de forma segura no banco de dados.
        /// </summary>
        /// <param name="senha">Senha em texto plano</param>
        /// <returns>Hash da senha em Base64</returns>
        public static string HashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifica se uma senha em texto plano corresponde ao hash armazenado.
        /// Usado durante o processo de login para validar credenciais.
        /// </summary>
        /// <param name="senha">Senha em texto plano fornecida pelo usuário</param>
        /// <param name="senhaHash">Hash da senha armazenado no banco de dados</param>
        /// <returns>True se a senha for válida, False caso contrário</returns>
        public static bool VerificarSenha(string senha, string senhaHash)
        {
            var hashDaSenha = HashSenha(senha);
            return hashDaSenha == senhaHash;
        }
    }
}
