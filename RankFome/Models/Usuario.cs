// =============================================================================
// Model: Usuario
// =============================================================================
// Representa um usuário do sistema RankFome.
// Pode ser um Cliente (consumidor), Vendedor (dono de loja) ou Dev (administrador).
// Responsável por autenticação e controle de acesso na aplicação.
// =============================================================================

namespace RankFome.Models
{
    /// <summary>
    /// Entidade que representa um usuário do sistema.
    /// Armazena dados pessoais, credenciais de acesso e tipo de perfil.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Idade do usuário em anos.
        /// </summary>
        public int Idade { get; set; }

        /// <summary>
        /// Localização ou endereço do usuário (cidade, bairro, etc.).
        /// </summary>
        public string Localizacao { get; set; } = string.Empty;

        /// <summary>
        /// CPF ou Email do usuário - usado como identificador único para login.
        /// Possui índice único no banco de dados para garantir unicidade.
        /// </summary>
        public string CpfEmail { get; set; } = string.Empty;

        /// <summary>
        /// Hash SHA256 da senha do usuário.
        /// A senha nunca é armazenada em texto plano por segurança.
        /// </summary>
        public string SenhaHash { get; set; } = string.Empty;

        /// <summary>
        /// Tipo/perfil do usuário que define suas permissões no sistema.
        /// </summary>
        public TipoUsuario Tipo { get; set; } = TipoUsuario.Cliente;

        /// <summary>
        /// Data e hora de criação do registro (UTC).
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // =============================================================================
        // Propriedades de Navegação (Relacionamentos)
        // =============================================================================

        /// <summary>
        /// Lista de lojas pertencentes a este usuário.
        /// Apenas usuários do tipo Vendedor ou Dev podem possuir lojas.
        /// Relacionamento: Um usuário pode ter várias lojas (1:N).
        /// </summary>
        public List<Loja> Lojas { get; set; } = new List<Loja>();
    }

    /// <summary>
    /// Enumeração que define os tipos de usuário do sistema.
    /// Cada tipo possui diferentes níveis de permissão.
    /// </summary>
    public enum TipoUsuario
    {
        /// <summary>
        /// Usuário consumidor - pode fazer pedidos e visualizar lojas/produtos.
        /// </summary>
        Cliente = 0,

        /// <summary>
        /// Usuário vendedor - pode criar lojas e gerenciar produtos.
        /// </summary>
        Vendedor = 1,

        /// <summary>
        /// Usuário desenvolvedor/administrador - acesso total ao sistema.
        /// </summary>
        Dev = 2
    }
}
