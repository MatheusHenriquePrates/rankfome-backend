# RankFome - Backend

Sistema de gerenciamento de pedidos e produtos para delivery de alimentos desenvolvido com ASP.NET Core e PostgreSQL.

## Changelog - Versao 2.0

Esta versao inclui diversas melhorias e novas funcionalidades em relacao a versao anterior:

### Novas Funcionalidades

- **Sistema de Autenticacao JWT**: Implementacao completa de autenticacao com tokens JWT, incluindo registro e login de usuarios.
- **Controle de Usuarios**: Novo modelo de Usuario com tres niveis de acesso (Cliente, Vendedor, Dev).
- **Sistema de Lojas**: Usuarios do tipo Vendedor podem criar e gerenciar suas proprias lojas.
- **Upload de Imagens**: Endpoint dedicado para upload de imagens de produtos e lojas.
- **Enderecos de Entrega**: Pedidos agora incluem endereco completo de entrega.
- **Formas de Pagamento**: Suporte a multiplas formas de pagamento (Dinheiro, Cartao, PIX, Vale-Refeicao).
- **Status de Pedido Detalhado**: Ciclo de vida completo do pedido (Pendente, Preparando, A Caminho, Entregue, Cancelado).

### Melhorias Tecnicas

- **Autorizacao por Roles**: Controle de acesso baseado em perfis de usuario nos endpoints.
- **Documentacao de Codigo**: Todos os arquivos de codigo agora possuem comentarios XML Documentation profissionais.
- **DTOs Estruturados**: Data Transfer Objects para todas as requisicoes e respostas da API.
- **Swagger com JWT**: Documentacao interativa configurada para testar endpoints autenticados.

### Novos Endpoints

- `POST /api/Usuarios/Registro` - Registro de novos usuarios
- `POST /api/Usuarios/Login` - Autenticacao de usuarios
- `GET/POST/PUT/DELETE /api/Lojas` - CRUD completo de lojas
- `GET /api/Produtos/Loja/{lojaId}` - Produtos por loja
- `PUT /api/Pedidos/{id}/Status` - Atualizacao de status do pedido
- `POST /api/Upload` - Upload de imagens

---

## Descricao do Projeto

RankFome e uma API RESTful para gestao de pedidos de delivery que permite:
- Cadastro e autenticacao de usuarios com diferentes perfis
- Criacao e gerenciamento de lojas por vendedores
- Catalogo de produtos por loja
- Realizacao de pedidos com multiplos itens
- Acompanhamento de status de pedidos
- Upload de imagens para produtos e lojas

## Tecnologias Utilizadas

- **Framework**: .NET 8.0 / ASP.NET Core
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core 8.0.11
- **Provider**: Npgsql.EntityFrameworkCore.PostgreSQL
- **Autenticacao**: JWT Bearer Tokens
- **Documentacao**: Swagger/OpenAPI (Swashbuckle 6.6.2)
- **Arquitetura**: API RESTful

## Estrutura do Projeto

```
RankFome/
├── Controllers/              # Controllers da API
│   ├── UsuariosController.cs # Autenticacao e registro
│   ├── LojasController.cs    # Gerenciamento de lojas
│   ├── ProdutosController.cs # Gerenciamento de produtos
│   ├── PedidosController.cs  # Gerenciamento de pedidos
│   └── UploadController.cs   # Upload de imagens
├── Data/                     # Contexto do banco de dados
│   └── AppDbContext.cs
├── Helpers/                  # Classes auxiliares
│   └── JwtHelper.cs          # Geracao de tokens JWT
├── Models/                   # Modelos de dominio
│   ├── Usuario.cs
│   ├── Loja.cs
│   ├── Produto.cs
│   ├── Pedido.cs
│   └── ItemPedido.cs
├── Migrations/               # Migrations do Entity Framework
├── Properties/
│   └── launchSettings.json
├── wwwroot/                  # Arquivos estaticos
│   └── images/               # Imagens uploaded
├── appsettings.json
└── Program.cs                # Configuracao da aplicacao
```

## Modelos de Dados

### Usuario

Representa um usuario do sistema com controle de acesso.

| Propriedade  | Tipo         | Descricao                          |
|--------------|--------------|-------------------------------------|
| Id           | int          | Identificador unico                 |
| Nome         | string       | Nome completo                       |
| Idade        | int          | Idade do usuario                    |
| Localizacao  | string       | Endereco do usuario                 |
| CpfEmail     | string       | CPF ou Email (unico para login)     |
| SenhaHash    | string       | Hash SHA256 da senha                |
| Tipo         | TipoUsuario  | Cliente (0), Vendedor (1), Dev (2)  |
| DataCriacao  | DateTime     | Data de cadastro em UTC             |

### Loja

Representa uma loja/estabelecimento no sistema.

| Propriedade  | Tipo     | Descricao                      |
|--------------|----------|--------------------------------|
| Id           | int      | Identificador unico            |
| Nome         | string   | Nome comercial                 |
| Descricao    | string   | Descricao da loja              |
| LogoUrl      | string   | URL do logotipo                |
| Rua          | string   | Endereco - rua                 |
| Numero       | string   | Endereco - numero              |
| Bairro       | string   | Endereco - bairro              |
| Cidade       | string   | Endereco - cidade              |
| Estado       | string   | Endereco - estado              |
| Complemento  | string?  | Endereco - complemento         |
| Latitude     | double   | Coordenada geografica          |
| Longitude    | double   | Coordenada geografica          |
| UsuarioId    | int      | ID do proprietario             |
| DataCriacao  | DateTime | Data de cadastro em UTC        |

### Produto

Representa um produto disponivel para venda em uma loja.

| Propriedade     | Tipo     | Descricao                         |
|-----------------|----------|-----------------------------------|
| Id              | int      | Identificador unico               |
| Nome            | string   | Nome do produto                   |
| Descricao       | string   | Descricao detalhada               |
| Preco           | decimal  | Preco (10 digitos, 2 decimais)    |
| ImagemUrl       | string   | URL da imagem                     |
| Categoria       | string   | Categoria do produto              |
| Disponivel      | bool     | Status de disponibilidade         |
| AvaliacaoMedia  | double   | Media das avaliacoes (0-5)        |
| TotalAvaliacoes | int      | Quantidade de avaliacoes          |
| LojaId          | int      | ID da loja                        |
| DataCriacao     | DateTime | Data de cadastro em UTC           |

### Pedido

Representa um pedido realizado por um cliente.

| Propriedade         | Tipo           | Descricao                        |
|---------------------|----------------|----------------------------------|
| Id                  | int            | Identificador unico              |
| DataPedido          | DateTime       | Data/hora do pedido em UTC       |
| ValorTotal          | decimal        | Valor total calculado            |
| Status              | StatusPedido   | Status do pedido                 |
| FormaPagamento      | FormaPagamento | Forma de pagamento               |
| EnderecoRua         | string         | Endereco de entrega - rua        |
| EnderecoNumero      | string         | Endereco de entrega - numero     |
| EnderecoBairro      | string         | Endereco de entrega - bairro     |
| EnderecoCidade      | string         | Endereco de entrega - cidade     |
| EnderecoEstado      | string         | Endereco de entrega - estado     |
| EnderecoComplemento | string?        | Endereco de entrega - complemento|
| Observacoes         | string?        | Observacoes do cliente           |
| ClienteId           | int            | ID do cliente                    |

**StatusPedido**: Pendente (0), Preparando (1), ACaminho (2), Entregue (3), Cancelado (4)

**FormaPagamento**: Dinheiro (0), CartaoCredito (1), CartaoDebito (2), Pix (3), ValeRefeicao (4)

### ItemPedido

Representa cada item individual dentro de um pedido.

| Propriedade   | Tipo    | Descricao                         |
|---------------|---------|-----------------------------------|
| Id            | int     | Identificador unico               |
| PedidoId      | int     | ID do pedido                      |
| ProdutoId     | int     | ID do produto                     |
| Quantidade    | int     | Quantidade solicitada             |
| PrecoUnitario | decimal | Preco no momento do pedido        |
| Subtotal      | decimal | Valor total do item               |

## Endpoints da API

### Autenticacao

#### POST /api/Usuarios/Registro
Registra um novo usuario no sistema.

**Body:**
```json
{
  "nome": "string",
  "idade": 0,
  "localizacao": "string",
  "cpfEmail": "string",
  "senha": "string",
  "confirmarSenha": "string",
  "tipo": 0
}
```

**Resposta:** `200 OK` com token JWT

#### POST /api/Usuarios/Login
Autentica um usuario existente.

**Body:**
```json
{
  "cpfEmail": "string",
  "senha": "string"
}
```

**Resposta:** `200 OK` com token JWT | `401 Unauthorized`

### Lojas

| Metodo | Endpoint          | Descricao              | Autenticacao       |
|--------|-------------------|------------------------|--------------------|
| GET    | /api/Lojas        | Lista todas as lojas   | Nao                |
| GET    | /api/Lojas/{id}   | Retorna loja por ID    | Nao                |
| POST   | /api/Lojas        | Cria nova loja         | Vendedor, Dev      |
| PUT    | /api/Lojas/{id}   | Atualiza loja          | Vendedor (dono), Dev|
| DELETE | /api/Lojas/{id}   | Remove loja            | Dev                |

### Produtos

| Metodo | Endpoint                   | Descricao                 | Autenticacao       |
|--------|----------------------------|---------------------------|--------------------|
| GET    | /api/Produtos              | Lista todos os produtos   | Nao                |
| GET    | /api/Produtos/{id}         | Retorna produto por ID    | Nao                |
| GET    | /api/Produtos/Loja/{lojaId}| Produtos de uma loja      | Nao                |
| POST   | /api/Produtos              | Cria novo produto         | Vendedor, Dev      |
| PUT    | /api/Produtos/{id}         | Atualiza produto          | Vendedor (dono), Dev|
| DELETE | /api/Produtos/{id}         | Remove produto            | Vendedor (dono), Dev|

### Pedidos

| Metodo | Endpoint                  | Descricao                | Autenticacao       |
|--------|---------------------------|--------------------------|---------------------|
| GET    | /api/Pedidos              | Lista pedidos do usuario | Sim (todos)         |
| GET    | /api/Pedidos/{id}         | Retorna pedido por ID    | Sim (dono ou Dev)   |
| POST   | /api/Pedidos              | Cria novo pedido         | Sim (todos)         |
| PUT    | /api/Pedidos/{id}/Status  | Atualiza status          | Vendedor, Dev       |
| DELETE | /api/Pedidos/{id}         | Remove pedido            | Dev                 |

### Upload

#### POST /api/Upload
Realiza upload de imagem.

**Content-Type:** multipart/form-data

**Parametro:** file (IFormFile)

**Validacoes:**
- Extensoes permitidas: jpg, jpeg, png, gif, webp
- Tamanho maximo: 5MB

**Resposta:** `200 OK` com URL da imagem

## Configuracao e Instalacao

### Pre-requisitos

- .NET 8.0 SDK
- PostgreSQL 12 ou superior
- Git

### Configuracao do Banco de Dados

1. Crie um banco de dados PostgreSQL

2. Configure a connection string no arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rankfome;Username=seu_usuario;Password=sua_senha"
  },
  "Jwt": {
    "SecretKey": "SuaChaveSecretaComPeloMenos32Caracteres",
    "Issuer": "RankFome",
    "Audience": "RankFomeApp"
  }
}
```

### Instalacao

1. Clone o repositorio:
```bash
git clone <url-do-repositorio>
cd rankfome-backend/RankFome
```

2. Restaure as dependencias:
```bash
dotnet restore
```

3. Execute as migrations para criar as tabelas:
```bash
dotnet ef database update
```

4. Execute a aplicacao:
```bash
dotnet run
```

A API estara disponivel em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Autenticacao JWT

### Obtendo o Token

1. Registre um usuario ou faca login
2. Copie o token retornado na resposta

### Usando o Token

Inclua o token no header Authorization de todas as requisicoes autenticadas:

```
Authorization: Bearer <seu_token_jwt>
```

### Swagger UI

No Swagger, clique no botao "Authorize" e insira:
```
Bearer <seu_token_jwt>
```

## Banco de Dados

### Tabelas

- **Usuarios**: Armazena usuarios do sistema
- **Lojas**: Armazena lojas dos vendedores
- **Produtos**: Armazena produtos das lojas
- **Pedidos**: Armazena pedidos dos clientes
- **ItensPedido**: Relacionamento entre Pedidos e Produtos

### Relacionamentos

- Usuario (1) -> Lojas (N): Um usuario pode ter varias lojas
- Loja (1) -> Produtos (N): Uma loja pode ter varios produtos
- Pedido (1) -> ItensPedido (N): Um pedido pode ter varios itens
- Produto (1) -> ItensPedido (N): Um produto pode estar em varios itens

### Indices

- Indice unico em `Usuarios.CpfEmail` para garantir unicidade

### Exclusao em Cascata

- Deletar Usuario remove suas Lojas
- Deletar Loja remove seus Produtos

## Migrations

### Comandos Uteis

Criar nova migration:
```bash
dotnet ef migrations add NomeDaMigration
```

Aplicar migrations:
```bash
dotnet ef database update
```

Reverter ultima migration:
```bash
dotnet ef database update PreviousMigrationName
```

## Documentacao da API

A documentacao interativa esta disponivel via Swagger UI em modo de desenvolvimento.

Acesse: `https://localhost:5001/swagger`

O Swagger esta configurado para suportar autenticacao JWT, permitindo testar todos os endpoints protegidos.

## Consideracoes de Seguranca

- Configure uma chave JWT forte e unica para producao
- A configuracao CORS atual permite todas as origens - configure adequadamente para producao
- Utilize HTTPS obrigatorio em producao
- Utilize variaveis de ambiente para dados sensiveis
- Implemente rate limiting para protecao contra abuso

## Contribuindo

1. Faca um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudancas (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## Licenca

Este projeto esta sob a licenca MIT.

## Autor

Desenvolvido como projeto de estudo de ASP.NET Core, Entity Framework e autenticacao JWT.
