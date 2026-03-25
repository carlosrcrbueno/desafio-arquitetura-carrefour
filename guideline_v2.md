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

✔ Transactions
CREATE TABLE transactions_2026_01 (
    id UUID PRIMARY KEY,
    account_id UUID NOT NULL,
    amount NUMERIC(18,2) NOT NULL,
    type INT NOT NULL,
    created_at TIMESTAMP NOT NULL
);

✔ DailyBalances
CREATE TABLE daily_balances (
    account_id UUID NOT NULL,
       date DATE NOT NULL,
       balance NUMERIC(18,2) NOT NULL,
       PRIMARY KEY (account_id, date)
   );

   ✔ REGRAS SQL
   ✔ usar snake_case no banco
   ✔ tabelas em minúsculo
   ✔ colunas em minúsculo
⚙️ PROVIDER ADO.NET (OBRIGATÓRIO)
USAR: PostgreSQL
Provider: Npgsql

✔ Implementação
using System.Data;
using Npgsql;

public IDbConnection CreateConnection()
{
       return new NpgsqlConnection(connectionString);
   }

   ❌ PROIBIDO
   ❌ SqlConnection
   ❌ Microsoft.Data.SqlClient🧠 REGRAS DE IMPLEMENTAÇÃO (OBRIGATÓRIO)
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

📦 DEPENDÊNCIA (OBRIGATÓRIO)

   Package obrigatório:
   Npgsql
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

📁 24. ESTRUTURA DE DOCUMENTAÇÃO (OBRIGATÓRIO)

O agente DEVE criar a seguinte estrutura na raiz do projeto:

docs/

 ├── arquitetura/
 ├── docker/
 ├── postman/
📂 docs/arquitetura
✔ conter todos os documentos arquiteturais
✔ NÃO gerar conteúdo automaticamente
✔ apenas garantir existência da pasta
🐳 docs/docker (OBRIGATÓRIO)

O agente DEVE criar:

docs/docker/docker-compose.yml
docs/docker/Dockerfile
✔ Requisitos do docker-compose
✔ subir SQL Server
✔ subir API
✔ configurar rede entre containers
✔ configurar variáveis de ambiente
✔ garantir persistência de dados (volume)
✔ SQL Server
image: mcr.microsoft.com/mssql/server:2022-latest
✔ API
✔ build a partir do Dockerfile
✔ expor porta 5000
❌ PROIBIDO
❌ usar banco externo
❌ deixar infra fora do docker-compose
📬 docs/postman
✔ criar arquivo:
docs/postman/collection.json

✔ deve conter:
- POST /transactions
- GET /transactions
- GET /balances/daily
- POST /balances/rebuild
🧪 BLOCO 2 — TESTES DE FLUXO (CRÍTICO)
🧠 25. TESTES DE INTEGRAÇÃO / FLUXO (OBRIGATÓRIO)

O agente DEVE implementar testes de fluxo completos.

📦 25.1 Ledger → Event → Read Model
Arquivo:
Transactions.Tests/Integration/TransactionFlowTests.cs
Método obrigatório
CreateTransaction_DeveAtualizarLedgerEReadModel
Cenário
Dado:
- request válido

Quando:
- transação criada

Então:
✔ salva no ledger
✔ publica evento
✔ atualiza read model
📦 25.2 Falha + Reprocessamento
Arquivo:
Balance.Tests/Integration/ReprocessingFlowTests.cs
Método obrigatório
FalhaNoReadModel_DeveSerCorrigidaComReprocessamento
Cenário
Dado:
- transação salva no ledger
- falha simulada no read model

Quando:
- reprocessamento executado

Então:
✔ saldo reconstruído corretamente
⚙️ BANCO PARA TESTES (OBRIGATÓRIO)
✔ usar repositórios in-memory
✔ simular particionamento
✔ NÃO usar SQL Server real
✔ Implementações obrigatórias
FakeTransactionRepository
FakeDailyBalanceRepository
❌ PROIBIDO
❌ usar docker nos testes
❌ usar banco real
❌ usar infraestrutura real
🧠 VALIDAÇÃO
✔ fluxo completo testado
✔ falha simulada
✔ reprocessamento validado
⚡ BLOCO 3 — RATE LIMIT (REQUISITO FUNCIONAL)
🛡️ 26. RATE LIMIT (OBRIGATÓRIO)
✔ Regra
50 requisições por minuto por IP
✔ Aplicação
✔ aplicar em TODOS os endpoints
✔ comportamento global
✔ Implementação

Deve ser implementado como middleware.
RATE LIMIT (OBRIGATÓRIO)

Deve ser implementado como middleware.

✔ Comportamento
✔ até 50 requisições por minuto POR IP E POR ENDPOINT → permitido
✔ acima disso → bloqueado
✔ Chave de controle
{ip}:{endpoint}
✔ Armazenamento
✔ utilizar Redis
✔ usar TTL nativo do Redis (60 segundos)
✔ Response
429 Too Many Requests
{
  "error": "Seu Ip será liberado em {tempo_restante_segundos} segundos, até lá suas requisições para este endpoint estarão impedidas."
}
✔ Regra de cálculo do tempo
✔ tempo_restante deve ser obtido diretamente do TTL da chave no Redis
✔ NÃO calcular manualmente
⚠️ REGRAS
✔ rate limit deve ser aplicado por IP e por endpoint
✔ endpoints devem possuir contadores independentes
✔ Redis é a única fonte de verdade para controle e tempo
✔ middleware não deve conter lógica de negócio
❌ PROIBIDO
❌ usar controle em memória
❌ usar timestamp manual para cálculo de tempo
❌ rate limit global (sem considerar endpoint)
❌ ignorar IP
❌ implementar parcialmente
🧠 VALIDAÇÃO FINAL
✔ limite aplicado por IP + endpoint
✔ TTL de 5 minutos controlado pelo Redis
✔ resposta 429 com tempo restante correto
✔ comportamento consistente entre múltiplos endpoints
✔ não afeta lógica de negócio
🎯 RESULTADO FINAL

Agora o guideline cobre:

✔ arquitetura
✔ domínio
✔ application
✔ infrastructure
✔ api
✔ testes unitários
✔ testes de fluxo
✔ docker
✔ documentação
✔ rate limit
✔ observabilidade (via interfaces)
✔ segurança

27. CONFIGURAÇÃO DO DOCKER (OBRIGATÓRIO)
✔ docker-compose.yml — VARIÁVEIS FIXAS

O agente DEVE utilizar exatamente:

version: '3.9'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: cashflow-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql

  api:
    build:
      context: ../../
      dockerfile: docs/docker/Dockerfile
    container_name: cashflow-api
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=CashflowDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    ports:
      - "5000:5000"
    depends_on:
      - sqlserver

volumes:
  sql_data:
⚠️ REGRAS
✔ NÃO alterar senha
✔ NÃO alterar nomes de serviços
✔ NÃO alterar portas
🧪 BLOCO — TESTES DO RATE LIMIT
🧠 28. TESTES DO RATE LIMIT (OBRIGATÓRIO)
📦 Arquivo
Api.Tests/Middleware/RateLimitMiddlewareTests.cs
✔ Métodos obrigatórios
DevePermitirAte50RequisicoesPorMinuto
DeveBloquearApos50Requisicoes
DeveRetornarStatus429QuandoExcedido
✔ Cenários
DevePermitirAte50RequisicoesPorMinuto
Dado:
- mesmo IP

Quando:
- 50 requisições executadas

Então:
✔ todas devem retornar sucesso
DeveBloquearApos50Requisicoes
Dado:
- 50 requisições já realizadas

Quando:
- 51ª requisição executada

Então:
✔ deve ser bloqueada
DeveRetornarStatus429QuandoExcedido
Então:
✔ status = 429
✔ body = { "error": "Rate limit exceeded" }
⚠️ REGRAS
✔ simular HttpContext
✔ não subir servidor real
✔ não usar rede
🌐 BLOCO — TESTES DE CONTROLLER (INTEGRAÇÃO LEVE)
🧠 29. TESTES DE CONTROLLERS (OBRIGATÓRIO)
📦 Arquivo
Api.Tests/Controllers/TransactionsControllerTests.cs
Api.Tests/Controllers/BalanceControllerTests.cs
✔ TransactionsControllerTests
Métodos
PostTransactions_DeveRetornar201QuandoSucesso
PostTransactions_DeveRetornar400QuandoInvalido
GetTransactions_DeveRetornar200ComDados
✔ BalanceControllerTests
Métodos
GetDailyBalance_DeveRetornar200
PostRebuild_DeveRetornar204
✔ Regras
✔ mockar use cases com Moq
✔ NÃO acessar banco
✔ NÃO usar infraestrutura real
🔁 BLOCO — REVISÃO OBRIGATÓRIA DOS CONTROLLERS
🧠 30. VALIDAÇÃO DOS CONTROLLERS (OBRIGATÓRIO)

O agente DEVE garantir:

✔ controller NÃO contém regra de negócio
✔ apenas chama use case
✔ mapeia request/response
✔ trata status code corretamente
❌ PROIBIDO
❌ lógica no controller
❌ acesso direto ao repository
❌ cálculos no controller

Perfeito — o Copilot está 100% correto de novo.

👉 E aqui a solução é a mesma lógica dos outros pontos:

NÃO pedir pra ele criar
→ DEFINIR no guideline
🎯 O que vamos fazer

Vou te entregar um bloco determinístico de .env, incluindo:

✔ variáveis obrigatórias
✔ valores padrão
✔ integração com docker-compose
✔ como a API deve consumir
📄 BLOCO PARA ADICIONAR NO GUIDELINE — .ENV (DEFINITIVO)
⚙️ 31. ARQUIVO .ENV (OBRIGATÓRIO)

O agente DEVE criar um arquivo .env na raiz do projeto.

📂 Localização
/.env

📄 Conteúdo OBRIGATÓRIO
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=cashflowdb

API_PORT=5000

REDIS_HOST=redis
REDIS_PORT=6379

ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=cashflowdb;Username=postgres;Password=postgres
ConnectionStrings__Redis=redis:6379

🐳 USO NO DOCKER-COMPOSE (OBRIGATÓRIO)

O docker-compose.yml DEVE utilizar o .env.

✔ Regra
env_file:
  - ../../.env

✔ Exemplo (PostgreSQL)
postgres:
  image: postgres:15
  container_name: cashflow-postgres
  environment:
    - POSTGRES_USER=${POSTGRES_USER}
    - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    - POSTGRES_DB=${POSTGRES_DB}
  ports:
    - "5432:5432"
  volumes:
    - pg_data:/var/lib/postgresql/data

✔ Exemplo (API)
environment:
  - ConnectionStrings__DefaultConnection=${ConnectionStrings__DefaultConnection}
  - ConnectionStrings__Redis=${ConnectionStrings__Redis}

🧠 USO NA APLICAÇÃO (.NET)

✔ Regra
✔ usar IConfiguration
✔ NÃO ler arquivo manualmente
✔ NÃO usar bibliotecas externas para .env

✔ Exemplo
builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Configuration["ConnectionStrings:Redis"];

⚠️ REGRAS
✔ .env é a fonte única de configuração
✔ NÃO duplicar valores no código
✔ NÃO hardcodar connection strings

❌ PROIBIDO
❌ definir valores direto no Program.cs
❌ ignorar .env no docker-compose
❌ criar variáveis não definidas acima

🔐 SEGURANÇA
✔ .env NÃO deve ser commitado
✔ adicionar ao .gitignore
✔ .gitignore
.env

🧠 VALIDAÇÃO FINAL
✔ .env existe na raiz
✔ docker-compose usa env_file
✔ API consome via IConfiguration
✔ nenhuma variável hardcoded

