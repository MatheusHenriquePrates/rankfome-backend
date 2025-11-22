// =============================================================================
// Data: AppDbContext
// =============================================================================
// Contexto do Entity Framework Core para acesso ao banco de dados PostgreSQL.
// Define os DbSets para todas as entidades do sistema e configura os
// relacionamentos, índices e precisão de campos decimais.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using RankFome.Models;

namespace RankFome.Data
{
    /// <summary>
    /// Contexto principal do banco de dados da aplicação.
    /// Gerencia as conexões e operações com o PostgreSQL via Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração do contexto.
        /// </summary>
        /// <param name="options">Opções de configuração incluindo connection string</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // =============================================================================
        // DbSets - Tabelas do Banco de Dados
        // =============================================================================

        /// <summary>
        /// Tabela de usuários do sistema.
        /// </summary>
        public DbSet<Usuario> Usuarios { get; set; }

        /// <summary>
        /// Tabela de lojas/estabelecimentos.
        /// </summary>
        public DbSet<Loja> Lojas { get; set; }

        /// <summary>
        /// Tabela de produtos disponíveis nas lojas.
        /// </summary>
        public DbSet<Produto> Produtos { get; set; }

        /// <summary>
        /// Tabela de pedidos realizados pelos clientes.
        /// </summary>
        public DbSet<Pedido> Pedidos { get; set; }

        /// <summary>
        /// Tabela de itens de pedido (relacionamento entre Pedido e Produto).
        /// </summary>
        public DbSet<ItemPedido> ItensPedido { get; set; }

        // =============================================================================
        // Configuração do Modelo (Fluent API)
        // =============================================================================

        /// <summary>
        /// Configura o modelo de dados usando Fluent API.
        /// Define relacionamentos, índices e precisão de campos.
        /// </summary>
        /// <param name="modelBuilder">Builder para configuração do modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------------------------------------------
            // Configuração de Índices
            // -----------------------------------------------------------------

            // Índice único para CPF/Email - garante que não existam duplicatas
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.CpfEmail)
                .IsUnique();

            // -----------------------------------------------------------------
            // Configuração de Relacionamentos
            // -----------------------------------------------------------------

            // Relacionamento: Usuario (1) -> Lojas (N)
            // Quando um usuário é deletado, suas lojas também são removidas
            modelBuilder.Entity<Loja>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.Lojas)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento: Loja (1) -> Produtos (N)
            // Quando uma loja é deletada, seus produtos também são removidos
            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Loja)
                .WithMany(l => l.Produtos)
                .HasForeignKey(p => p.LojaId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------------------------------------------
            // Configuração de Precisão para Campos Decimais
            // -----------------------------------------------------------------

            // Preço do produto: até 99.999.999,99
            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasPrecision(10, 2);

            // Preço unitário do item: até 99.999.999,99
            modelBuilder.Entity<ItemPedido>()
                .Property(i => i.PrecoUnitario)
                .HasPrecision(10, 2);

            // Subtotal do item: até 99.999.999,99
            modelBuilder.Entity<ItemPedido>()
                .Property(i => i.Subtotal)
                .HasPrecision(10, 2);

            // Valor total do pedido: até 99.999.999,99
            modelBuilder.Entity<Pedido>()
                .Property(p => p.ValorTotal)
                .HasPrecision(10, 2);
        }
    }
}
