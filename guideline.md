Perfeito — agora você chegou no nível correto de rigor para usar IA como executor.

👉 O problema que você identificou é real:

qualquer ambiguidade → o Copilot decide → e isso quebra a arquitetura

Então agora vamos fechar isso de forma 100% determinística, incluindo:

particionamento temporal do ledger (obrigatório)
zero liberdade de decisão
instruções imperativas
comportamento em caso de dúvida
📄 guideline.md — VERSÃO FINAL (DETERMINÍSTICA E IMPERATIVA)
🧠 GUIDELINE DE IMPLEMENTAÇÃO (EXECUÇÃO CONTROLADA POR IA)
⚠️ REGRA FUNDAMENTAL

O agente (Copilot) NÃO DEVE TOMAR DECISÕES.

Todas as implementações devem seguir EXATAMENTE este documento.

SE QUALQUER INFORMAÇÃO ESTIVER AUSENTE:
→ NÃO INVENTAR
→ NÃO ASSUMIR
→ NÃO CRIAR VARIAÇÕES
→ UTILIZAR APENAS O QUE ESTÁ DEFINIDO AQUI

Este documento deve ser tratado como fonte única de verdade (RAG).

🎯 OBJETIVO

Implementar sistema de controle de fluxo de caixa com:

ledger imutável (append-only)
particionamento temporal obrigatório
consolidação de saldo diário
comunicação por eventos
monolito modular desacoplado
🏗️ 1. SOLUTION

Criar:

Cashflow.sln
📦 2. PROJETOS (OBRIGATÓRIO)
src/
├── Api

├── Modules/
│   ├── Transactions/
│   │   ├── Transactions.Domain
│   │   ├── Transactions.Application
│   │   ├── Transactions.Infrastructure
│
│   ├── Balance/
│       ├── Balance.Domain
│       ├── Balance.Application
│       ├── Balance.Infrastructure

├── Shared/
│   ├── Shared.Database
│   ├── Shared.Messaging
│   ├── Shared.Security
│   ├── Shared.Observability

tests/
├── Transactions.Tests
├── Balance.Tests
⚠️ 3. REGRAS DE DEPENDÊNCIA (OBRIGATÓRIO)
Modules NÃO podem se referenciar
Comunicação SOMENTE via eventos
Domain NÃO depende de ninguém
Application depende de Domain
Infrastructure depende de Domain
Api depende apenas de Application
🧱 4. DOMÍNIO — TRANSACTIONS
Entidade (FIXA — NÃO ALTERAR)
Transaction
{
Guid Id;
Guid AccountId;
decimal Amount;
TransactionType Type;
DateTime CreatedAt;
}
Regras (OBRIGATÓRIO)
Amount > 0
Tipo obrigatório
NÃO permitir update
NÃO permitir delete
sempre append-only
Enum
TransactionType
{
Credit = 1,
Debit = 2
}
🧱 5. PARTICIONAMENTO DO LEDGER (OBRIGATÓRIO)
Regra

As transações NÃO devem ser armazenadas em tabela única.

Devem ser armazenadas em tabelas mensais no formato:

transactions\_{YYYY}\_{MM}

Exemplo:

transactions\_2026\_01
transactions\_2026\_02
Regra de escrita

Ao inserir transação:

usar CreatedAt para determinar a tabela
Regra de leitura

Para buscar dados:

consultar múltiplas tabelas quando necessário
PROIBIDO
criar tabela única "Transactions"
ignorar particionamento
criar estratégia alternativa
🧱 6. DOMÍNIO — BALANCE
Entidade (FIXA)
DailyBalance
{
Guid AccountId;
DateOnly Date;
decimal Balance;
}
Regra
saldo calculado a partir do ledger
crédito soma
débito subtrai
🔄 7. EVENTO (OBRIGATÓRIO)
TransactionCreatedEvent
{
Guid TransactionId;
Guid AccountId;
decimal Amount;
TransactionType Type;
DateTime CreatedAt;
}
💾 8. BANCO DE DADOS
Ledger (NÃO criar tabela fixa)

Criar tabelas dinamicamente por mês:

CREATE TABLE transactions\_2026\_01 (...)
Balance
CREATE TABLE DailyBalances (
AccountId UNIQUEIDENTIFIER NOT NULL,
Date DATE NOT NULL,
Balance DECIMAL(18,2) NOT NULL,
PRIMARY KEY (AccountId, Date)
);
⚙️ 9. REPOSITÓRIOS
Transactions
Insert(Transaction transaction)
→ determinar tabela dinamicamente

GetByAccount(accountId)
→ consultar múltiplas partições
Balance
Get(accountId, date)
Upsert(balance)
🧠 10. CASOS DE USO
CreateTransaction
validar
persistir na partição correta
publicar evento
GetBalance
retornar saldo consolidado
🔌 11. EVENT BUS
implementação em memória
obrigatório publish/subscribe
🛡️ 12. MIDDLEWARES

OBRIGATÓRIO implementar:

autenticação mock
rate limit por IP
correlation id
logging estruturado
🧪 13. TESTES
Transactions.Tests
inserção na partição correta
validações
append-only
Balance.Tests
cálculo correto
evento aplicado corretamente
REGRAS
usar mocks
usar InMemory
NÃO usar banco real
🔁 14. REPROCESSAMENTO

OBRIGATÓRIO:

ler TODAS as partições
recalcular saldo
reconstruir DailyBalances
🚀 15. EXECUÇÃO

Ao subir aplicação:

criar partição do mês atual se não existir
executar script SQL
registrar handlers
❌ 16. PROIBIDO (EXPLÍCITO)
usar ORM
ignorar particionamento
criar abstrações não definidas
modificar entidades
tomar decisões fora deste guideline
🎯 INSTRUÇÃO FINAL

Gerar código COMPLETO respeitando:

particionamento temporal obrigatório
entidades fixas
eventos definidos
fluxo exato
NÃO CRIAR VARIAÇÕES
NÃO SIMPLIFICAR
NÃO OTIMIZAR
NÃO INTERPRETAR

🛠️ 17. CRIAÇÃO COMPLETA DA SOLUTION (OBRIGATÓRIO)

Executar TODOS os comandos abaixo exatamente na ordem especificada.

17.1 Criar solution
dotnet new sln -n Cashflow
17.2 Criar projeto de API
dotnet new webapi -n Api -o src/Api
17.3 Criar módulo Transactions
dotnet new classlib -n Transactions.Domain -o src/Modules/Transactions/Transactions.Domain
dotnet new classlib -n Transactions.Application -o src/Modules/Transactions/Transactions.Application
dotnet new classlib -n Transactions.Infrastructure -o src/Modules/Transactions/Transactions.Infrastructure
17.4 Criar módulo Balance
dotnet new classlib -n Balance.Domain -o src/Modules/Balance/Balance.Domain
dotnet new classlib -n Balance.Application -o src/Modules/Balance/Balance.Application
dotnet new classlib -n Balance.Infrastructure -o src/Modules/Balance/Balance.Infrastructure
17.5 Criar projetos Shared
dotnet new classlib -n Shared.Database -o src/Shared/Shared.Database
dotnet new classlib -n Shared.Messaging -o src/Shared/Shared.Messaging
dotnet new classlib -n Shared.Security -o src/Shared/Shared.Security
dotnet new classlib -n Shared.Observability -o src/Shared/Shared.Observability
17.6 Criar projetos de teste
dotnet new xunit -n Transactions.Tests -o tests/Transactions.Tests
dotnet new xunit -n Balance.Tests -o tests/Balance.Tests
17.7 Adicionar projetos à solution
dotnet sln Cashflow.sln add src/**/**/*.csproj
dotnet sln Cashflow.sln add tests/\*\*/*.csproj
🔗 18. CONFIGURAÇÃO DE REFERÊNCIAS (OBRIGATÓRIO)

Executar exatamente:

Transactions
dotnet add src/Modules/Transactions/Transactions.Application reference src/Modules/Transactions/Transactions.Domain
dotnet add src/Modules/Transactions/Transactions.Infrastructure reference src/Modules/Transactions/Transactions.Domain
Balance
dotnet add src/Modules/Balance/Balance.Application reference src/Modules/Balance/Balance.Domain
dotnet add src/Modules/Balance/Balance.Infrastructure reference src/Modules/Balance/Balance.Domain
API
dotnet add src/Api reference src/Modules/Transactions/Transactions.Application
dotnet add src/Api reference src/Modules/Balance/Balance.Application
Testes
dotnet add tests/Transactions.Tests reference src/Modules/Transactions/Transactions.Application
dotnet add tests/Balance.Tests reference src/Modules/Balance/Balance.Application
⚠️ 19. REGRAS DE VALIDAÇÃO ESTRUTURAL

Após execução, o agente DEVE garantir:

existe arquivo Cashflow.sln
todos os projetos estão incluídos
não há dependência entre módulos
estrutura de pastas está EXATAMENTE como definida
❌ SE QUALQUER REGRA FALHAR
PARAR EXECUÇÃO
NÃO CORRIGIR AUTOMATICAMENTE
NÃO REORGANIZAR
NÃO CRIAR NOVAS ESTRUTURAS



🧠 21. USO DE DESIGN PATTERNS (RESTRIÇÕES)



O uso de Design Patterns NÃO é obrigatório e NÃO deve ser aplicado por antecipação.



O agente NÃO deve introduzir padrões que não estejam explicitamente necessários.



✔ Permitido

uso de interfaces para desacoplamento (Dependency Inversion)

implementação de EventBus como instância única (singleton controlado)

❌ Proibido



O agente NÃO deve implementar:



Factory Pattern

Builder Pattern

Strategy Pattern

Mediator Pattern

qualquer outro padrão não solicitado

⚠️ Regra

NÃO adicionar abstrações sem necessidade explícita

NÃO criar extensibilidade antecipada

NÃO generalizar código

22. ORGANIZAÇÃO DE ARQUIVOS (OBRIGATÓRIO)



Cada tipo deve estar em um arquivo separado.



REGRA ABSOLUTA:

1 arquivo = 1 tipo

✔ Estrutura obrigatória

Classes

TransactionCreatedEvent.cs

Enums

TransactionType.cs

Interfaces

ITransactionRepository.cs

Implementações

TransactionRepository.cs

❌ PROIBIDO



O agente NÃO deve:



declarar mais de um tipo por arquivo

colocar enum junto com classe

colocar interface e implementação no mesmo arquivo

criar arquivos genéricos com múltiplos tipos

✔ EXEMPLO CORRETO

TransactionCreatedEvent.cs

namespace Shared.Messaging;



public sealed class TransactionCreatedEvent

{

&#x20;   public Guid TransactionId { get; init; }

&#x20;   public Guid AccountId { get; init; }

&#x20;   public decimal Amount { get; init; }

&#x20;   public TransactionType Type { get; init; }

&#x20;   public DateTime CreatedAt { get; init; }

}

TransactionType.cs

namespace Shared.Messaging;



public enum TransactionType

{

&#x20;   Credit = 1,

&#x20;   Debit = 2

}

⚠️ REGRA DE VALIDAÇÃO



Após gerar código, o agente DEVE validar:



✔ cada arquivo contém apenas um tipo

✔ nome do arquivo = nome do tipo

❌ SE VIOLAR

PARAR EXECUÇÃO

NÃO AGRUPAR TIPOS

NÃO CORRIGIR AUTOMATICAMENTE


👉 estrutura de pastas interna por tipo:
Domain/

&#x20;├── Entities/

&#x20;├── Enums/

&#x20;├── Events/

&#x20;├── Interfaces/

Perfeito — isso que você apontou é outro ponto crítico.



👉 Nome de teste ruim = código ilegível + perda de intenção

👉 E o Copilot SEMPRE cria UnitTest1.cs se você não travar isso



🎯 Diagnóstico



O problema não é só nome feio.



É:



❌ ausência de padrão semântico

❌ perda de rastreabilidade

❌ dificuldade de manutenção

🧠 Regra correta (nível profissional)



Testes devem seguir:



<ClassName>Tests.cs



E métodos:



MethodName\_Scenario\_ExpectedResult

✍️ BLOCO PARA ADICIONAR NO GUIDELINE

🧪 23. PADRÃO DE NOMENCLATURA DE TESTES (OBRIGATÓRIO)

📂 Nome dos arquivos



O agente DEVE renomear qualquer arquivo padrão gerado automaticamente.



PROIBIDO:

UnitTest1.cs

Test1.cs

Tests.cs (genérico)

✔ Padrão obrigatório

<ClassName>Tests.cs

Exemplos

CreateTransactionUseCaseTests.cs

TransactionRepositoryTests.cs

BalanceCalculatorTests.cs

ReprocessLedgerTests.cs

📌 Organização por módulo

tests/



&#x20;├── Transactions.Tests/

&#x20;│   ├── Application/

&#x20;│   ├── Domain/

&#x20;│   ├── Infrastructure/



&#x20;├── Balance.Tests/

&#x20;    ├── Application/

&#x20;    ├── Domain/

&#x20;    ├── Infrastructure/

🧠 Nome dos métodos de teste



Formato obrigatório:



Metodo\_Scenario\_ResultadoEsperado

Exemplos

CreateTransaction\_WithValidData\_ShouldPersistTransaction

CreateTransaction\_WithInvalidAmount\_ShouldThrowException

ApplyTransaction\_WhenCredit\_ShouldIncreaseBalance

ApplyTransaction\_WhenDebit\_ShouldDecreaseBalance

❌ PROIBIDO



O agente NÃO deve:



manter nomes padrão (UnitTest1)

usar nomes genéricos (Test, ShouldWork, Validate)

usar nomes curtos sem contexto

⚠️ REGRA DE VALIDAÇÃO



Após geração dos testes:



✔ todos os arquivos possuem nome descritivo

✔ nenhum arquivo padrão existe

✔ todos os métodos seguem padrão semântico

❌ SE VIOLAR

PARAR EXECUÇÃO

RENOMEAR ARQUIVOS

NÃO ACEITAR NOMES GENÉRICOS

