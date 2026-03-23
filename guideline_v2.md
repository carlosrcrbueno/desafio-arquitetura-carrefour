📄 GUIDELINE FINAL — VERSÃO CORRETA (FECHADA E EXECUTÁVEL)

👉 Essa versão está:

✔ sem contradição
✔ com ordem de execução
✔ com domínio definido
✔ com IoC correto
✔ com Shared corretamente modelado
✔ pronta para reset total
🧠 0. REGRA GLOBAL
O agente NÃO DEVE tomar decisões.

SE algo não estiver definido:
→ NÃO INVENTAR
→ NÃO INFERIR
→ PARAR EXECUÇÃO
🎯 1. OBJETIVO

Sistema de fluxo de caixa com:

ledger append-only
particionamento mensal obrigatório
saldo diário consolidado
arquitetura modular
comunicação por eventos
orientação a interfaces
inversão de controle por módulo
🚨 2. ORDEM DE EXECUÇÃO (OBRIGATÓRIO)
Criar solution
Criar projetos
Configurar referências
Criar estrutura de pastas
Implementar Shared (abstrações)
Implementar Domínio
Implementar Application
Implementar Infrastructure
Implementar EventBus
Configurar IoC por módulo
Implementar API
Ativar Swagger
Criar testes
Implementar reprocessamento
Validação final
🏗️ 3. SOLUTION
dotnet new sln -n Cashflow
📦 4. PROJETOS
dotnet new webapi -n Api -o src/Api

dotnet new classlib -n Transactions -o src/Modules/Transactions
dotnet new classlib -n Balance -o src/Modules/Balance

dotnet new classlib -n Shared -o src/Shared

dotnet new xunit -n Transactions.Tests -o tests/Transactions.Tests
dotnet new xunit -n Balance.Tests -o tests/Balance.Tests
🔗 5. REFERÊNCIAS
dotnet add src/Api reference src/Modules/Transactions
dotnet add src/Api reference src/Modules/Balance
dotnet add src/Api reference src/Shared
🧱 6. ESTRUTURA DOS MÓDULOS
Modules/

 Transactions/
  ├── Domain/
  │   ├── Entities/
  │   ├── Enums/
  │   ├── Events/
  │   ├── Interfaces/
  │
  ├── Application/
  │   ├── UseCases/
  │   ├── DTOs/
  │
  ├── Infrastructure/
      ├── Repositories/
      ├── Database/

 Balance/
  (mesma estrutura)
🧠 7. SHARED (CORRIGIDO — MODELO CERTO)
Shared = abstrações transversais da arquitetura
✔ DEVE CONTER
✔ interfaces de infraestrutura (ex: IDbConnectionFactory)
✔ interfaces de mensageria (IEventBus)
✔ interfaces de observabilidade (ILogger, ITracer)
✔ contratos de eventos (TransactionCreatedEvent)
✔ contratos compartilhados
✔ enums compartilhados
❌ PROIBIDO
❌ implementações concretas
❌ lógica de negócio
❌ repositórios concretos
📂 Estrutura
Shared/

 ├── Database/
 │   ├── IDbConnectionFactory.cs

 ├── Messaging/
 │   ├── IEventBus.cs
 │   ├── Events/

 ├── Observability/
 │   ├── ILogger.cs
 │   ├── ITracer.cs

 ├── Contracts/
 ├── Enums/
⚠️ REGRA
Shared define contratos
Implementação ocorre fora (Api ou Modules.Infrastructure)
🧱 8. DOMÍNIO — TRANSACTIONS
public class Transaction
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTime CreatedAt { get; }
}
Enum
public enum TransactionType
{
    Credit = 1,
    Debit = 2
}
Regras
Amount > 0
append-only
sem update/delete
🧱 9. DOMÍNIO — BALANCE
public class DailyBalance
{
    public Guid AccountId { get; }
    public DateOnly Date { get; }
    public decimal Balance { get; }
}
🔄 10. EVENTO
public class TransactionCreatedEvent
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
💾 11. PARTICIONAMENTO
transactions_{YYYY}_{MM}
⚙️ 12. REPOSITÓRIOS
Interface → Domain
Implementação → Infrastructure
usar ADO.NET
🔌 13. EVENT BUS
Interface → Shared
Implementação → Api
🔗 14. IOC POR MÓDULO (CRÍTICO)
public static class TransactionsModule
{
    public static IServiceCollection AddTransactions(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        return services;
    }
}
⚠️ REGRA
APENAS Program.cs chama:
builder.Services.AddTransactions();
builder.Services.AddBalance();
🌐 15. API
controllers apenas orquestram
sem regra de negócio
📚 16. SWAGGER (OBRIGATÓRIO)
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI();
🧪 17. TESTES
✔ Nome
<ClassName>Tests.cs
❌ PROIBIDO
UnitTest1.cs
📂 18. ARQUIVOS
1 arquivo = 1 tipo
🔁 19. REPROCESSAMENTO
ler todas partições
recalcular saldo
reconstruir
🧠 20. DESIGN PATTERNS

✔ DI
✔ EventBus singleton

❌ PROIBIDO
Factory
Strategy
⚠️ 21. VALIDAÇÃO FINAL
✔ shared só abstrações
✔ implementação fora do shared
✔ IoC por módulo
✔ swagger ativo
✔ particionamento correto
✔ testes corretos
✔ 1 tipo por arquivo
✔ orientação a interface aplicada
❌ 22. SE FALHAR
PARAR EXECUÇÃO

DEFINIÇÃO EXPLÍCITA DAS INTERFACES DO SHARED (OBRIGATÓRIO)

O agente DEVE implementar exatamente as interfaces abaixo, sem adicionar, remover ou modificar membros.

🔌 IDbConnectionFactory
namespace Shared.Database;

using System.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
🔌 IEventBus
namespace Shared.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T @event);
    void Subscribe<T>(Func<T, Task> handler);
}
🔌 ILogger
namespace Shared.Observability;

public interface ILogger
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception exception);
}
🔌 ITracer
namespace Shared.Observability;

public interface ITracer
{
    IDisposable StartSpan(string name);
}
⚠️ REGRAS OBRIGATÓRIAS
✔ NÃO adicionar métodos extras
✔ NÃO alterar assinaturas
✔ NÃO criar sobrecargas
✔ NÃO usar frameworks externos (ILogger<T>, etc)
❌ PROIBIDO
❌ adaptar para Microsoft.Extensions.Logging
❌ adicionar parâmetros (context, metadata, etc)
❌ criar versões genéricas diferentes
🧠 INTENÇÃO ARQUITETURAL (IMPORTANTE)

Essas interfaces existem para:

✔ desacoplar infraestrutura
✔ permitir troca de implementação
✔ manter domínio limpo
✔ forçar IoC

DEFINIÇÃO DOS USE CASES E DTOS (OBRIGATÓRIO)

O agente DEVE implementar exatamente os UseCases e DTOs abaixo.
NÃO é permitido criar novos casos, alterar nomes ou modificar assinaturas.

📦 1. TRANSACTIONS — USE CASES
✔ CreateTransactionUseCase
Interface
namespace Transactions.Application.UseCases;

using Transactions.Application.DTOs;

public interface ICreateTransactionUseCase
{
    Task<CreateTransactionResponse> ExecuteAsync(CreateTransactionRequest request);
}
Request DTO
namespace Transactions.Application.DTOs;

public class CreateTransactionRequest
{
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
}
Response DTO
namespace Transactions.Application.DTOs;

public class CreateTransactionResponse
{
    public Guid TransactionId { get; init; }
}
✔ GetTransactionsByAccountAndPeriodUseCase
Interface
namespace Transactions.Application.UseCases;

using Transactions.Application.DTOs;

public interface IGetTransactionsByAccountAndPeriodUseCase
{
    Task<IReadOnlyList<TransactionDto>> ExecuteAsync(GetTransactionsByAccountAndPeriodRequest request);
}
Request DTO
namespace Transactions.Application.DTOs;

public class GetTransactionsByAccountAndPeriodRequest
{
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
Response DTO
namespace Transactions.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}
📦 2. BALANCE — USE CASES
✔ GetDailyBalanceUseCase
Interface
namespace Balance.Application.UseCases;

using Balance.Application.DTOs;

public interface IGetDailyBalanceUseCase
{
    Task<IReadOnlyList<DailyBalanceDto>> ExecuteAsync(GetDailyBalanceRequest request);
}
Request DTO
namespace Balance.Application.DTOs;

public class GetDailyBalanceRequest
{
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
Response DTO
namespace Balance.Application.DTOs;

public class DailyBalanceDto
{
    public Guid AccountId { get; init; }
    public DateOnly Date { get; init; }
    public decimal Balance { get; init; }
}
✔ RebuildDailyBalancesUseCase
Interface
namespace Balance.Application.UseCases;

public interface IRebuildDailyBalancesUseCase
{
    Task ExecuteAsync();
}
⚠️ REGRAS OBRIGATÓRIAS
✔ NÃO criar novos use cases
✔ NÃO alterar nomes
✔ NÃO alterar assinaturas
✔ NÃO adicionar propriedades nos DTOs
✔ NÃO remover propriedades
❌ PROIBIDO
❌ criar “helpers”
❌ criar “services genéricos”
❌ adicionar campos extras
❌ alterar tipos (DateTime → DateOnly, etc)
🧠 INTENÇÃO ARQUITETURAL

Esses UseCases representam exatamente:

✔ entrada do sistema (commands e queries)
✔ fronteira da aplicação
✔ contrato estável para API

DEFINIÇÃO DOS REPOSITÓRIOS (OBRIGATÓRIO)

O agente DEVE implementar exatamente as interfaces abaixo.

📦 1. TRANSACTIONS — REPOSITÓRIO
Interface
namespace Transactions.Domain.Interfaces;

using Transactions.Domain.Entities;

public interface ITransactionRepository
{
    Task InsertAsync(Transaction transaction);

    Task<IReadOnlyList<Transaction>> GetByAccountAndPeriodAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate
    );

    Task<IReadOnlyList<Transaction>> GetAllAsync();
}
📦 2. BALANCE — REPOSITÓRIO
Interface
namespace Balance.Domain.Interfaces;

using Balance.Domain.Entities;

public interface IDailyBalanceRepository
{
    Task UpsertAsync(DailyBalance balance);

    Task<IReadOnlyList<DailyBalance>> GetByAccountAndPeriodAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate
    );

    Task DeleteAllAsync();
}
⚠️ REGRAS
✔ NÃO adicionar métodos
✔ NÃO alterar assinaturas
✔ NÃO mudar tipos de retorno
💾 ESQUEMA DE BANCO DE DADOS (OBRIGATÓRIO)
📊 1. TABELAS DE TRANSACTIONS (PARTICIONADAS)

Nome:

transactions_{YYYY}_{MM}
Estrutura
CREATE TABLE transactions_2026_01 (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
Regras
✔ criar tabela dinamicamente se não existir
✔ usar CreatedAt para definir partição
✔ Type = 1 (Credit) ou 2 (Debit)
📊 2. TABELA DE SALDO
CREATE TABLE DailyBalances (
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Date DATE NOT NULL,
    Balance DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (AccountId, Date)
);
⚙️ PROVIDER ADO.NET (OBRIGATÓRIO)
USAR: SQL SERVER
✔ Implementação
using System.Data;
using Microsoft.Data.SqlClient;
✔ Conexão
public IDbConnection CreateConnection()
{
    return new SqlConnection(connectionString);
}
❌ PROIBIDO
❌ Npgsql
❌ MySql
❌ Sqlite
❌ abstrair provider
🧠 REGRAS DE IMPLEMENTAÇÃO (OBRIGATÓRIO)
✔ ADO.NET PURO
✔ usar SqlCommand
✔ usar parâmetros (@param)
✔ usar ExecuteReader / ExecuteNonQuery
❌ PROIBIDO
❌ Entity Framework
❌ Dapper
❌ ORM
🧠 REGRAS DE PARTICIONAMENTO (CRÍTICO)
✔ INSERT
1. ler CreatedAt
2. montar nome da tabela: transactions_YYYY_MM
3. garantir existência da tabela
4. inserir
✔ SELECT
consultar múltiplas tabelas no intervalo de datas
⚠️ PROIBIDO
❌ consultar tabela única
❌ ignorar período
❌ otimizar com shortcuts
🔁 REPROCESSAMENTO (OBRIGATÓRIO)
Fluxo
1. ler TODAS as tabelas transactions_*
2. ordenar por CreatedAt
3. recalcular saldo
4. limpar DailyBalances
5. reescrever
⚠️ REGRAS
✔ não usar cache
✔ não usar atalhos
✔ recalcular do zero
🧠 VALIDAÇÃO FINAL DA INFRASTRUCTURE
✔ usa SqlConnection
✔ usa ADO.NET puro
✔ respeita particionamento
✔ repositórios seguem contrato
✔ schema respeitado
❌ SE QUALQUER ITEM FALHAR
PARAR EXECUÇÃO
NÃO CORRIGIR AUTOMATICAMENTE

🌐 DEFINIÇÃO DA API (OBRIGATÓRIO)

A API deve ser implementada utilizando ASP.NET Core Web API com Controllers (MVC).

✔ NÃO usar Minimal APIs
✔ NÃO usar variações de framework
⚠️ PADRÃO GLOBAL
✔ Controllers apenas orquestram
✔ NÃO conter lógica de negócio
✔ chamar exclusivamente UseCases
📦 1. ENDPOINT — CREATE TRANSACTION
✔ Definição
POST /transactions
✔ Request
Origem: Body (JSON)
{
  "accountId": "guid",
  "amount": 100.00,
  "type": 1
}
✔ UseCase
ICreateTransactionUseCase
✔ Responses
201 Created
{
  "transactionId": "guid"
}
400 BadRequest
{
  "error": "Invalid request data"
}
401 Unauthorized
{
  "error": "Unauthorized"
}
500 InternalServerError
{
  "error": "Internal server error"
}
📦 2. ENDPOINT — GET TRANSACTIONS
✔ Definição
GET /transactions
✔ Query Parameters
accountId (Guid) — obrigatório
startDate (DateTime) — obrigatório
endDate (DateTime) — obrigatório
✔ UseCase
IGetTransactionsByAccountAndPeriodUseCase
✔ Response
200 OK
[
  {
    "id": "guid",
    "accountId": "guid",
    "amount": 100,
    "type": 1,
    "createdAt": "2026-01-01T10:00:00"
  }
]
400 BadRequest
{
  "error": "Invalid query parameters"
}
401 Unauthorized
{
  "error": "Unauthorized"
}
500 InternalServerError
{
  "error": "Internal server error"
}
📦 3. ENDPOINT — GET DAILY BALANCE
✔ Definição
GET /balances/daily
✔ Query Parameters
accountId (Guid) — obrigatório
startDate (DateTime) — obrigatório
endDate (DateTime) — obrigatório
✔ UseCase
IGetDailyBalanceUseCase
✔ Response
200 OK
[
  {
    "accountId": "guid",
    "date": "2026-01-01",
    "balance": 1000
  }
]
400 BadRequest
{
  "error": "Invalid query parameters"
}
401 Unauthorized
{
  "error": "Unauthorized"
}
500 InternalServerError
{
  "error": "Internal server error"
}
📦 4. ENDPOINT — REBUILD BALANCES
✔ Definição
POST /balances/rebuild
✔ Request
Sem body
✔ UseCase
IRebuildDailyBalancesUseCase
✔ Responses
204 NoContent
Sem corpo
401 Unauthorized
{
  "error": "Unauthorized"
}
500 InternalServerError
{
  "error": "Internal server error"
}
🛡️ AUTENTICAÇÃO (OBRIGATÓRIO)
✔ Regra
Todas as rotas exigem header:

Authorization: Bearer {token}
✔ Comportamento
✔ Se não existir → 401
✔ Não validar token real (mock)
⚠️ PADRÃO DE ERRO GLOBAL
✔ Formato único
{
  "error": "string"
}
❌ PROIBIDO
❌ retornar exceção crua
❌ retornar stack trace
❌ formatos diferentes por endpoint
🧠 MAPEAMENTO OBRIGATÓRIO
✔ Controller → UseCase
POST /transactions → ICreateTransactionUseCase
GET /transactions → IGetTransactionsByAccountAndPeriodUseCase
GET /balances/daily → IGetDailyBalanceUseCase
POST /balances/rebuild → IRebuildDailyBalancesUseCase
⚠️ PROIBIDO
❌ acessar repositório diretamente
❌ lógica dentro do controller
❌ bypass do Application
🧠 VALIDAÇÃO FINAL DA API
✔ todos endpoints implementados
✔ rotas corretas
✔ verbos corretos
✔ status codes corretos
✔ autenticação aplicada
✔ resposta padronizada
✔ controllers sem lógica
❌ SE QUALQUER ITEM FALHAR
PARAR EXECUÇÃO
NÃO CORRIGIR AUTOMATICAMENTE


🧪 DEFINIÇÃO DE TESTES (OBRIGATÓRIO)

O agente DEVE implementar exatamente as classes de teste e métodos abaixo.

⚙️ STACK DE TESTES (OBRIGATÓRIO)
Framework: xUnit
Mock: Moq
Banco: InMemory (simulado via implementação fake do repositório)
❌ PROIBIDO
❌ usar banco real
❌ usar SQL Server em teste
❌ usar outro framework (NUnit, MSTest)
📦 1. TRANSACTIONS — TESTES
✔ 1.1 TransactionRepositoryTests
Arquivo:
Transactions.Tests/Infrastructure/TransactionRepositoryTests.cs
Métodos obrigatórios
InsertAsync_DeveSalvarNaParticaoCorreta
GetByAccountAndPeriodAsync_DeveRetornarDadosDoPeriodo
GetByAccountAndPeriodAsync_DeveConsultarMultiplasParticoes
Cenários
InsertAsync_DeveSalvarNaParticaoCorreta
Dado:
- Transaction com CreatedAt = 2026-01-10

Quando:
- InsertAsync for executado

Então:
- deve usar tabela transactions_2026_01
GetByAccountAndPeriodAsync_DeveConsultarMultiplasParticoes
Dado:
- período que atravessa meses

Então:
- deve consultar mais de uma tabela
✔ 1.2 CreateTransactionUseCaseTests
Arquivo:
Transactions.Tests/Application/CreateTransactionUseCaseTests.cs
Métodos
ExecuteAsync_ComDadosValidos_DeveCriarTransacao
ExecuteAsync_ComAmountInvalido_DeveLancarExcecao
ExecuteAsync_DevePublicarEvento
✔ 1.3 GetTransactionsByAccountAndPeriodUseCaseTests
Arquivo:
Transactions.Tests/Application/GetTransactionsByAccountAndPeriodUseCaseTests.cs
Métodos
ExecuteAsync_DeveRetornarTransacoesDoPeriodo
ExecuteAsync_SemDados_DeveRetornarListaVazia
📦 2. BALANCE — TESTES
✔ 2.1 DailyBalanceRepositoryTests
Arquivo:
Balance.Tests/Infrastructure/DailyBalanceRepositoryTests.cs
Métodos
UpsertAsync_DeveInserirNovoSaldo
UpsertAsync_DeveAtualizarSaldoExistente
GetByAccountAndPeriodAsync_DeveRetornarSaldos
✔ 2.2 GetDailyBalanceUseCaseTests
Arquivo:
Balance.Tests/Application/GetDailyBalanceUseCaseTests.cs
Métodos
ExecuteAsync_DeveRetornarSaldos
ExecuteAsync_SemDados_DeveRetornarListaVazia
✔ 2.3 RebuildDailyBalancesUseCaseTests
Arquivo:
Balance.Tests/Application/RebuildDailyBalancesUseCaseTests.cs
Métodos
ExecuteAsync_DeveReconstruirSaldos
ExecuteAsync_DeveLimparDadosAntesDeReprocessar
Cenário crítico
Dado:
- múltiplas transações

Quando:
- rebuild executado

Então:
- saldo deve ser recalculado corretamente
🧠 REGRAS DE TESTE (OBRIGATÓRIO)
✔ Estrutura AAA
Arrange
Act
Assert
✔ Nome de método
Metodo_Scenario_ResultadoEsperado
✔ Uso de Moq
✔ mockar interfaces
✔ nunca mockar classes concretas
✔ Banco em memória

Criar implementação fake:

FakeTransactionRepository
FakeDailyBalanceRepository
❌ PROIBIDO
❌ acessar banco real
❌ usar SqlConnection em testes
❌ usar infraestrutura real
🧠 VALIDAÇÕES OBRIGATÓRIAS
✔ todos use cases possuem testes
✔ repositórios possuem testes
✔ particionamento validado
✔ eventos testados
✔ reprocessamento testado
⚠️ SE QUALQUER TESTE NÃO EXISTIR
PARAR EXECUÇÃO
❌ PROIBIDO
❌ criar testes extras
❌ alterar nomes
❌ adicionar cenários não definidos
🧠 INTENÇÃO ARQUITETURAL

Os testes representam:

✔ validação do comportamento esperado
✔ garantia de integridade do ledger
✔ garantia de consistência do saldo