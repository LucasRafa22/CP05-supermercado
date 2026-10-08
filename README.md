# 🛒 CP5 — Supermercado API

## 👥 Integrantes

* **Lucas Rafael Solimene** — RM 565194
* **Samyr Couto Oliveira** — RM 565562

---

## 🎯 Sobre o Projeto

Este projeto consiste em uma **API REST para gerenciamento de um supermercado**, desenvolvida em **.NET 9**, utilizando **Clean Architecture**, **Entity Framework Core** e banco de dados **Oracle**.

A aplicação possui recursos para gerenciamento de:

* Clientes
* Produtos
* Categorias
* Vendas
* Itens de venda

O projeto foi evoluído para o **CP5**, incorporando:

* Versionamento de API;
* V1 depreciada e V2 atual;
* Paginação na V2;
* Rate Limiting;
* Swagger separado por versão;
* Headers de versionamento;
* Health Check;
* Tratamento global de exceções;
* Testes automatizados;
* Persistência com Oracle;
* Banco em memória para testes de integração.

---

# 🏗️ Arquitetura

O projeto utiliza uma organização baseada em **Clean Architecture**, separando responsabilidades entre as camadas:

```text
Supermercado
│
├── Supermercado.API
│   ├── Controllers
│   ├── Middlewares
│   ├── Swagger
│   └── Program.cs
│
├── Supermercado.Application
│   ├── DTOs
│   ├── Interfaces
│   └── Services
│
├── Supermercado.Domain
│   ├── Entities
│   └── Regras de negócio
│
├── Supermercado.Infrastructure
│   ├── Data
│   ├── Repositories
│   └── Configurações do EF Core
│
├── Supermercado.Domain.Tests
├── Supermercado.Application.Tests
└── Supermercado.API.Tests
```

### Responsabilidade das camadas

| Camada             | Responsabilidade                                                   |
| ------------------ | ------------------------------------------------------------------ |
| **API**            | Controllers, HTTP, versionamento, Swagger, rate limit e middleware |
| **Application**    | DTOs, interfaces e regras de aplicação                             |
| **Domain**         | Entidades e regras de negócio                                      |
| **Infrastructure** | Entity Framework Core, Oracle e repositórios                       |
| **Tests**          | Testes unitários e testes de integração                            |

---

# 🛠️ Tecnologias Utilizadas

* **.NET 9**
* **ASP.NET Core**
* **C#**
* **Entity Framework Core 9**
* **Oracle Database**
* **Oracle.EntityFrameworkCore**
* **Swagger / OpenAPI**
* **Swashbuckle.AspNetCore**
* **ASP.NET API Versioning**
* **Microsoft.AspNetCore.RateLimiting**
* **Health Checks**
* **xUnit**
* **Moq**
* **EF Core InMemory**
* **Clean Architecture**

---

# 🗄️ Banco de Dados

O projeto utiliza **Oracle Database** como banco de dados principal.

A conexão é realizada através do:

```text
ApplicationDbContext
```

A configuração utiliza a chave:

```json
"RecommendaContextOracle"
```

Exemplo:

```json
{
  "ConnectionStrings": {
    "RecommendaContextOracle": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=<USUARIO>;Password=<SENHA>;"
  }
}
```


---

# 🚀 Como Executar o Projeto

## 1. Clonar o repositório

```bash
git clone https://github.com/LucasRafa22/CP05-supermercado.git
```

Entre na pasta:

```bash
cd CP05-supermercado
```

---

## 2. Configurar o banco Oracle

Configure a connection string no arquivo:

```text
Supermercado.API/appsettings.Development.json
```

Exemplo:

```json
{
  "ConnectionStrings": {
    "RecommendaContextOracle": "Data Source=oracle.fiap.com.br:1521/orcl;User ID=<USUARIO>;Password=<SENHA>;"
  }
}
```

---

## 3. Restaurar as dependências

Na raiz do projeto:

```bash
dotnet restore
```

---

## 4. Compilar o projeto

```bash
dotnet build
```

---

## 5. Executar a API

Entre no projeto da API:

```bash
cd Supermercado.API
```

Execute:

```bash
dotnet run
```

---

# 🔗 URLs da API

De acordo com a configuração de execução do projeto:

### HTTP

```text
http://localhost:5084
```

---

# 📚 Swagger

O Swagger permite visualizar e testar os endpoints da API.

```text
http://localhost:5084/swagger
```

O Swagger possui documentação separada para as versões da API:

```text
Supermercado API V1 - Deprecated
Supermercado API V2
```

A **V1 está marcada como deprecated**, enquanto a **V2 representa a versão atual da API**.

---

# 🔢 Versionamento da API

O projeto utiliza:

```text
Asp.Versioning.Mvc
Asp.Versioning.Mvc.ApiExplorer
```

Foram implementadas duas versões do recurso `Produto`.

## V1

A V1 é uma versão **depreciada** da API.

```text
/api/Produto
```

com:

```text
api-version=1.0
```

Retorna a lista tradicional de produtos em formato de array.

Exemplo:

```http
GET /api/Produto?api-version=1.0
```

---

## V2

A V2 é a versão atual.

```http
GET /api/Produto?api-version=2.0
```

A V2 utiliza paginação.

Exemplo:

```http
GET /api/Produto?api-version=2.0&page=1&pageSize=20
```

---

# 🔀 Formas de Selecionar a Versão

A API aceita duas formas de informar a versão.

## Query String

```http
GET /api/Produto?api-version=1.0
```

ou:

```http
GET /api/Produto?api-version=2.0
```

---

## Header

V1:

```http
GET /api/Produto
X-Api-Version: 1.0
```

V2:

```http
GET /api/Produto
X-Api-Version: 2.0
```

---

## Versão padrão

Quando nenhuma versão é informada:

```http
GET /api/Produto
```

a API utiliza automaticamente a:

```text
V2
```

Isso ocorre porque a configuração define:

```text
DefaultApiVersion = 2.0
```

e:

```text
AssumeDefaultVersionWhenUnspecified = true
```

---

# ⚠️ Headers de Versionamento

A API informa as versões disponíveis através dos headers de resposta.

### Versões suportadas

```text
api-supported-versions: 1.0, 2.0
```

### Versões depreciadas

```text
api-deprecated-versions: 1.0
```

Dessa forma, o consumidor consegue identificar que:

* `1.0` ainda está disponível;
* `1.0` está depreciada;
* `2.0` é a versão atual.

---

# 📄 Paginação — V2

A paginação foi implementada exclusivamente na **V2**.

A V1 permanece com o contrato antigo, retornando diretamente uma lista de produtos.

A V2 possui os parâmetros:

| Parâmetro  | Padrão |   Limite |
| ---------- | -----: | -------: |
| `page`     |      1 | mínimo 1 |
| `pageSize` |     20 |  1 a 100 |

Exemplo:

```http
GET /api/Produto?api-version=2.0&page=1&pageSize=20
```

---

## 📦 Resposta paginada

A V2 retorna um envelope contendo:

```json
{
  "page": 1,
  "pageSize": 20,
  "totalItems": 25,
  "totalPages": 2,
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "nome": "Arroz",
      "preco": 25.90,
      "estoque": 50,
      "categoriaId": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```

### Campos

| Campo        | Descrição                        |
| ------------ | -------------------------------- |
| `page`       | Página atual                     |
| `pageSize`   | Quantidade solicitada por página |
| `totalItems` | Quantidade total de registros    |
| `totalPages` | Quantidade total de páginas      |
| `items`      | Produtos da página atual         |

---

# 🧮 Cálculo de Páginas

O total de páginas é calculado utilizando:

```text
totalPages = ceil(totalItems / pageSize)
```

Exemplo:

```text
25 produtos
pageSize = 10

totalPages = ceil(25 / 10)
totalPages = 3
```

---

# 🚫 Validação da Paginação

## Página menor que 1

Exemplo:

```http
GET /api/Produto?api-version=2.0&page=0
```

Resultado:

```text
400 Bad Request
```

---

## PageSize maior que 100

Exemplo:

```http
GET /api/Produto?api-version=2.0&pageSize=101
```

Resultado:

```text
400 Bad Request
```

A resposta informa o parâmetro inválido.

---

## Página além do total

Caso seja solicitada uma página inexistente:

```http
GET /api/Produto?api-version=2.0&page=999&pageSize=20
```

A API retorna:

```text
200 OK
```

com:

```json
{
  "page": 999,
  "pageSize": 20,
  "totalItems": 25,
  "totalPages": 2,
  "items": []
}
```

---

# ⚙️ Paginação no Banco de Dados

A paginação é realizada diretamente através do Entity Framework Core.

O repositório utiliza:

```csharp
CountAsync()
OrderBy()
Skip()
Take()
ToListAsync()
```

A estrutura evita carregar todos os registros na memória antes da paginação.

Exemplo conceitual:

```csharp
var totalItems = await query.CountAsync();

var items = await query
    .OrderBy(p => p.Nome)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

Dessa forma, a aplicação mantém a paginação na camada de persistência.

---

# 🚦 Rate Limiting

Foi implementado **Rate Limiting nativo do ASP.NET Core** utilizando:

```text
Microsoft.AspNetCore.RateLimiting
```

A política aplicada ao cadastro de produtos é:

```text
produto-write
```

Configuração:

```text
10 requisições por minuto
```

O limite é aplicado ao:

```http
POST /api/Produto
```

---

# 🔴 Resposta 429

Quando o limite é ultrapassado, a API retorna:

```text
429 Too Many Requests
```

Além disso, a resposta possui:

```text
Retry-After
```

indicando quando o cliente poderá tentar novamente.

Exemplo:

```text
HTTP/1.1 429 Too Many Requests
Retry-After: 60
```

A resposta possui formato JSON:

```json
{
  "type": "https://httpstatuses.com/429",
  "title": "Limite de requisições excedido",
  "status": 429,
  "detail": "O limite de requisições para este endpoint foi excedido. Tente novamente após o período informado no header Retry-After."
}
```

---

# 🩺 Health Check

A API possui o endpoint:

```http
GET /health
```

O Health Check verifica:

* funcionamento da aplicação;
* conexão com o banco de dados.

Foram configurados os checks:

```text
self
database
```

### `self`

Verifica se a API está funcionando corretamente.

### `database`

Verifica o funcionamento do `ApplicationDbContext`.

---

## 📊 Status do Health Check

```text
200 OK
```

quando a aplicação está saudável.

Em caso de estado não saudável:

```text
503 Service Unavailable
```

---

# 📝 Observabilidade e Logs

A aplicação utiliza o sistema de logging do ASP.NET Core através de:

```csharp
ILogger
```

Os logs possuem informações relevantes sobre as requisições.

Nas operações de produtos são registrados dados como:

```text
TraceId
Page
PageSize
TotalItems
```

Exemplo:

```text
GET /api/Produto V2 iniciado
TraceId: ...
Page: 1
PageSize: 20
```

O `TraceId` é obtido através de:

```csharp
HttpContext.TraceIdentifier
```

Isso permite correlacionar uma requisição com os eventos registrados durante seu processamento.

---

# 🛡️ Tratamento Global de Exceções

A API possui middleware para tratamento global de exceções:

```text
ExceptionHandlerMiddleware
```

O middleware transforma exceções da aplicação em respostas HTTP padronizadas.

O projeto mantém o tratamento utilizado nas versões anteriores, incluindo respostas no padrão:

```text
ProblemDetails
```

### Mapeamento

| Exceção                | HTTP |
| ---------------------- | ---: |
| `ArgumentException`    |  400 |
| `KeyNotFoundException` |  404 |
| `Exception`            |  500 |

As respostas de erro possuem informações como:

```json
{
  "type": "...",
  "title": "...",
  "status": 400,
  "detail": "..."
}
```

---

# 🧪 Testes Automatizados

O projeto possui testes automatizados utilizando:

* **xUnit**
* **Moq**
* **EF Core InMemory**
* `WebApplicationFactory`

Os testes estão separados em:

```text
Supermercado.Domain.Tests
Supermercado.Application.Tests
Supermercado.API.Tests
```

---

# 🧱 Testes de Domínio

Os testes de domínio validam as regras de negócio das entidades.

São utilizados:

```text
[Fact]
[Theory]
```

São testados cenários válidos e inválidos.

---

# ⚙️ Testes de Application

Os testes da camada Application utilizam **Moq** para simular os repositórios.

São validados cenários como:

* criação de produto;
* validação de preço;
* tentativa de exclusão de produto inexistente;
* validação dos parâmetros de paginação;
* retorno correto dos dados paginados.

Também são verificadas as chamadas aos repositórios utilizando:

```csharp
Times.Once
```

e:

```csharp
Times.Never
```

---

# 🌐 Testes da API

A API possui testes de integração utilizando:

```text
WebApplicationFactory<Program>
```

Durante os testes é utilizado:

```text
EF Core InMemory
```

para evitar dependência do banco Oracle.

---

## 🔢 Testes de Versionamento

São testados:

* V1 retornando `200`;
* V2 retornando `200`;
* seleção da V1 através do query string;
* seleção da V2 através do query string;
* seleção da V1 através do header;
* seleção da V2 através do header;
* V2 como versão padrão;
* header `api-supported-versions`;
* header `api-deprecated-versions`.

---

## 📄 Testes de Paginação

São testados:

* página 1;
* página 2;
* `pageSize` padrão;
* `pageSize` personalizado;
* `pageSize` maior que 100;
* `page` menor que 1;
* página além do total;
* metadados da paginação.

---

## 🚦 Testes de Rate Limiting

São testados:

* limite de 10 requisições;
* retorno `429`;
* presença do `Retry-After`;
* resposta JSON;
* funcionamento do `/health` após o limite ser atingido.

---

# ✅ Resultado Final dos Testes

Comando utilizado:

```bash
dotnet test .\Supermercado.API.Tests\Supermercado.API.Tests.csproj
```

Resultado final:

```text
Resumo do teste: total: 18; falhou: 0; bem-sucedido: 18; ignorado: 0
```

### Status

```text
✅ 18 testes executados
✅ 18 testes aprovados
❌ 0 testes falhos
⏭️ 0 testes ignorados
```

---

# 🔗 Principais Endpoints

## Produtos

### V1 — Deprecated

```http
GET /api/Produto?api-version=1.0
```

### V2

```http
GET /api/Produto?api-version=2.0
```

### V2 com paginação

```http
GET /api/Produto?api-version=2.0&page=1&pageSize=20
```

### Buscar produto

```http
GET /api/Produto/{id}
```

### Criar produto

```http
POST /api/Produto
```

### Atualizar produto

```http
PUT /api/Produto/{id}
```

### Excluir produto

```http
DELETE /api/Produto/{id}
```

---

# 📌 Exemplo de Fluxo da V2

Uma requisição:

```http
GET /api/Produto?api-version=2.0&page=1&pageSize=2
```

passa pelo fluxo:

```text
Cliente
   ↓
ProdutoController
   ↓
ProdutoService
   ↓
IProdutoRepository
   ↓
ProdutoRepository
   ↓
Entity Framework Core
   ↓
Oracle
```

Para os testes:

```text
Cliente
   ↓
ProdutoController
   ↓
ProdutoService
   ↓
ProdutoRepository
   ↓
EF Core InMemory
```

---

# 🧩 Dependências Principais

### Versionamento

```xml
Asp.Versioning.Http
Asp.Versioning.Mvc
Asp.Versioning.Mvc.ApiExplorer
```

### Banco

```xml
Microsoft.EntityFrameworkCore
Oracle.EntityFrameworkCore
```

### Testes

```xml
Microsoft.EntityFrameworkCore.InMemory
xUnit
Moq
```
A solução também conta com testes automatizados para validar as principais funcionalidades implementadas, garantindo que os comportamentos de versionamento, paginação, rate limiting e health check estejam funcionando corretamente.
