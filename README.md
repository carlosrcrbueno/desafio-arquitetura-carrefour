# Desafio Arquitetura Carrefour – Cashflow API

Este repositório contém um **MVP de uma API de cashflow** construída em **.NET 8 / C# 12**.

O objetivo não é apenas “fazer funcionar”, mas **aplicar, em um monólito modular, os conceitos de arquitetura** definidos nos documentos em `docs/arquitetura` (PDF). Entre esses conceitos estão:

- Separação clara de domínios (Transações, Balance).
- Uso de eventos internos para alimentar read models.
- Idempotência em operações sensíveis.
- Tratamento consistente de valores monetários.
- Proteção da API via rate limiting distribuído (Redis).

Apesar de não estar dividido em múltiplos microserviços, a organização por módulos e os contratos entre eles foram pensados para ficar **próximos do desenho arquitetural** e facilitar uma futura evolução para serviços separados.

---

## 1. Visão Geral da Arquitetura

### 1.1. Módulos principais

- `Transactions` (`src/Modules/Transactions`)
  - Responsável pelas **transações financeiras**.
  - Entidade `Transaction` com:
    - `AmountInCents` (`long`) como valor persistido.
    - Propriedade de conveniência `Amount` (`decimal`) para uso em código.
    - Campo `IdempotenceKey` para garantir idempotência de criação.
  - Repositório `TransactionRepository`:
    - Persiste em PostgreSQL em tabelas particionadas por mês: `transactions_yyyy_MM`.
    - Usa `IdempotenceKey` com `UNIQUE` + `ON CONFLICT (IdempotenceKey) DO NOTHING` para não duplicar transações.
  - Use cases:
    - `CreateTransactionUseCase`: cria transações, aplica idempotência, publica eventos.
    - `GetTransactionsByAccountAndPeriodUseCase`: leitura por conta/período (uso interno e para testes; não exposto diretamente na collection deste MVP).

- `Balance` (`src/Modules/Balance`)
  - Responsável pelo **saldo diário consolidado por tenant/dia** (read model).
  - Entidade `DailyBalance`:
    - `TenantId`, `Date`, `BalanceInCents` (`long`).
    - Um único snapshot por `(TenantId, Date)`.
  - Repositório `DailyBalanceRepository`:
    - Tabela `DailyBalances` em PostgreSQL.
    - `PRIMARY KEY (TenantId, Date)` com `ON CONFLICT` para upsert.
  - Use cases:
    - `GetDailyBalanceUseCase`: retorna um `DailyBalanceDto?` por tenant/dia.
    - `RebuildDailyBalancesUseCase`: reprocessa todas as transações e reconstrói o read model.

- `Shared` (`src/Shared`)
  - Contratos de eventos (`TransactionCreatedEvent`).
  - Enums compartilhadas.
  - Infra de banco (`IDbConnectionFactory`) e de mensageria (`IEventBus`).

### 1.2. Eventos e read model de saldo

Quando uma transação é criada:

1. `CreateTransactionUseCase` persiste a transação.
2. Se não foi replay idempotente, publica um `TransactionCreatedEvent`.
3. O handler em `Balance` (`TransactionCreatedEventHandler`) consome o evento e:
   - Atualiza ou cria o snapshot diário em `DailyBalances`, para `(TenantId, Date)`.
   - Usa `Guid.Empty` como `AccountId` técnico no read model, pois a agregação é por tenant/dia.

Com isso, o **saldo diário é um read model** derivado do ledger de transações, em linha com o desenho arquitetural de CQRS e read models documentado em `docs/arquitetura`.

---

## 2. Representação de valores monetários

Uma decisão importante foi **representar valores monetários sempre em centavos**:

- No domínio (`Transaction`, `DailyBalance`):
  - `AmountInCents` / `BalanceInCents` (`long`) são a “verdade”.
  - Propriedades `Amount`/`Balance` em `decimal` apenas convertem `(long / 100m)` para consumo.
- No banco (PostgreSQL):
  - Colunas de valor usam `bigint`.

**Por quê?**

- Evita problemas de arredondamento e acumulação que podem surgir com `decimal` e tipos de banco `numeric(18,2)`.
- Facilita operações de soma/agrupamento em grandes volumes.
- Simplifica consistência entre código e banco (mesma unidade: centavos).

Os testes em `Transactions.Tests` e `Balance.Tests` foram ajustados para refletir essa decisão.

---

## 3. Endpoints da API

Os controllers estão em `src/Api/Controllers`. Os principais endpoints expostos para este MVP são:

### 3.1. `POST /transactions` – Criar transação (com idempotência)

**Controller**: `TransactionsController`  
**Use case**: `CreateTransactionUseCase`

Headers:

- `X-Tenant-Id`: identificação do tenant (resolvido por middleware e validado na controller).
- `X-Idempotence-Key` (opcional): chave de idempotência da requisição.
- `Authorization`: token mock.
- `Content-Type: application/json`.

Body (exemplo):

```json
{
  "amount": 1000, //em centavos
  "type": 1 //"Credit"
}
```

Fluxo:

1. `TenantId` é obtido de `HttpContext.Items["TenantId"]`.
2. `CreateTransactionRequest` recebe:
   - `TenantId` injetado.
   - `Amount` em `decimal`, convertido internamente para centavos.
   - `IdempotenceKey`:
     - Se veio no request, é usado.
     - Se não veio, o use case gera um GUID.
3. `CreateTransactionUseCase` chama `ITransactionRepository.InsertAsync(Transaction)`:
   - O repositório retorna `true` se inseriu, `false` se caiu em `ON CONFLICT` (replay idempotente).
4. Se `inserted == true`:
   - Publica `TransactionCreatedEvent`.
5. `CreateTransactionResponse` traz:
   - `TransactionId`
   - `AccountId`
   - `IsNew` (indicando se foi de fato uma nova transação).

Respostas da API:

- Sem `X-Idempotence-Key`:
  - Sempre `201 Created` com `{ transactionId, accountId }`.
- Com `X-Idempotence-Key`:
  - Primeira chamada com a chave:
    - `201 Created` com `{ transactionId, accountId }`.
  - Replays com a mesma chave:
    - `200 OK` com `{ "message": "valor já processado" }`.

**Motivação**:  
Permitir que clientes façam retries seguros (por exemplo, após timeout) sem risco de criar transações em duplicidade, e com um feedback claro da API quando um valor já foi processado.

---

### 3.2. `GET /balances/daily` – Snapshot diário por tenant/dia

**Controller**: `BalancesController`  
**Use case**: `GetDailyBalanceUseCase`

Headers:

- `X-Tenant-Id`
- `Authorization`

Query:

- `startDate`: data-alvo (um único dia).

Fluxo:

1. Valida `startDate` e `TenantId`.
2. Cria `GetDailyBalanceRequest` com `StartDate = EndDate = startDate` (um dia).
3. O use case busca um único snapshot (`DailyBalanceDto?`) por `(TenantId, Date)`.
4. Respostas:
   - `200 OK` com snapshot diário (valor em `decimal`, derivado de centavos).
   - `404 NotFound` se ainda não houver snapshot para aquele dia/tenant.

**Motivação**:  
Atender ao requisito de **“apenas um registro por dia por tenant”** para saldo diário, mantendo os detalhes de transações na base de ledger (`Transactions`).

---

### 3.3. `POST /balances/rebuild` – Reprocessar saldo diário

Headers:

- `X-Tenant-Id`
- `Authorization`

Fluxo:

- `RebuildDailyBalancesUseCase`:
  - Lê todas as transações.
  - Agrupa por `(TenantId, Date)` e soma em centavos.
  - Atualiza a tabela `DailyBalances` com snapshots consolidados.

Resposta: `204 No Content`.

**Motivação**:  
Possibilitar reconstrução do read model (saldo diário) em cenários de migração, correção ou auditoria.

---

## 4. Rate Limiting com Redis

**Arquivo**: `src/Api/Middlewares/RateLimitMiddleware.cs`

O rate limit é implementado como um middleware que:

- Usa Redis (`StackExchange.Redis`) como storage.
- Aplica um limite de **50 requisições por segundo** por **IP + rota**.
- Após estourar esse limite, aplica um **bloqueio (“freeze”) de 10 segundos** para aquela combinação IP/rota.

Algoritmo (alto nível):

1. Determina duas chaves no Redis:
   - `ratelimit:block:{ip}:{endpoint}` – indica se IP+rota estão bloqueados.
   - `ratelimit:{ip}:{endpoint}:{now}` – contador de requisições no segundo atual (`now = UnixTimeSeconds`).
2. Se a chave de bloqueio existir:
   - Retorna `429 Too Many Requests` imediatamente.
3. Caso contrário:
   - Incrementa o contador do segundo atual.
   - Se for a primeira requisição no segundo, aplica um TTL curto (3s) à chave do bucket.
   - Se o contador ultrapassar `LimitPerSecond` (50):
     - Grava `ratelimit:block:{ip}:{endpoint}` com TTL de 10s.
     - Retorna `429 Too Many Requests`.

**Motivação**:

- Proteger a API contra bursts excessivos de chamadas.
- Centralizar o estado de rate limit em Redis, permitindo múltiplas instâncias da API no futuro.
- Fornecer um comportamento previsível em testes de carga (ex.: k6) – aproximadamente metade das requisições acima de 50/s por rota/IP passam a receber 429, e em picos são todas bloqueadas até expirar o freeze.

---

## 5. Como rodar e testar

### 5.1. Subir com Docker Compose

Pré‑requisitos:

- Docker Desktop instalado e rodando.

Na raiz do repositório:

```powershell
cd C:\Users\ccarl\OneDrive\Documentos\Desafio-Arqu-Carrefour\desafio-arquitetura-carrefour
docker compose up --build
```

O Compose sobe:

- API .NET 8.
- PostgreSQL (transações, daily balances).
- Redis (rate limit).

A API expõe HTTPS em algo como `https://localhost:7165` (verificar portas no `docker-compose.yml`).

### 5.2. Testar com Postman

A collection está em `docs/postman/collection.json` e inclui, para este MVP:

- `Create Transaction`
- `Get Daily Balance`
- `Rebuild Balances`

Fluxos recomendados:

1. **Criar transação com idempotência** (`Create Transaction`):
   - Enviar uma vez **sem** `X-Idempotence-Key` para ver o fluxo normal (`201`).
   - Enviar duas vezes **com o mesmo** `X-Idempotence-Key` para observar:
     - Primeira vez: `201` com `transactionId` e `accountId`.
     - Segunda/terceira: `200` com `{ "message": "valor já processado" }`.

2. **Conferir saldo diário** (`Get Daily Balance`):
   - Usar um `startDate` correspondente ao dia da transação criada.
   - Validar que:
     - Retorna `200` com snapshot diário quando existir.
     - Retorna `404` para dias sem snapshot.

3. **Reprocessar saldo** (`Rebuild Balances`):
   - Rodar `POST /balances/rebuild`.
   - Repetir o `GET /balances/daily` e verificar que o snapshot continua consistente.

### 5.3. Testar com k6 (rate limit)

Usando um script simples de `k6` (script disponivel na pasta docs/k6), é possível:

- Simular 100+ requisições/s em `POST /transactions`.
- Ver, no relatório, quantos `status_429` vs. `status_other` aparecem por segundo.
- Ver períodos em que, após estouro do limite, todas as requisições recebem `429` durante os 10s de bloqueio.

---

## 6. MVP em Monólito Modular alinhado à documentação

Na pasta `docs/arquitetura` há um PDF com todas as definições de arquitetura: domínios, responsabilidades, fronteiras e possíveis microserviços.

Este repositório implementa um **MVP em monólito modular** que:

- Separa os domínios (`Transactions`, `Balance`) em módulos de código.
- Usa eventos internos (`TransactionCreatedEvent`) para alimentar um read model de saldo diário.
- Trabalha com valores monetários em centavos em todo o stack.
- Garante idempotência em transações via `IdempotenceKey` exposta na API.
- Protege a API com rate limiting persistido em Redis.

---

## 7. Referências de documentação e diagramas

Este repositório é um MVP que implementa, em um monólito modular, conceitos definidos em documentações e diagramas externos, que detalham:

- Visão de domínio (Transações, Balance e demais bounded contexts).
- Requisitos funcionais e não-funcionais.
- Padrões de integração, resiliente design, segurança, observabilidade etc.
- Diagramas de contexto, componentes e fluxos principais.

Esses materiais não estão públicos por padrão. Para acessá‑los, é necessário solicitar liberação de acesso por e‑mail.

### 7.1. Documentação funcional e arquitetural

- **Wiki de arquitetura e domínio**  
  URL: https://inftecbr.atlassian.net/wiki/spaces/DABC/overview  

  Esta wiki contém:
  - Descrição detalhada dos domínios e subdomínios.
  - Regras de negócio associadas ao cashflow.
  - Diretrizes de arquitetura (estilos, decisões e trade‑offs).
  - Padrões de integração e contratos de serviços planejados.

### 7.2. Diagramas de arquitetura

- **Diagramas de arquitetura (contexto, componentes, fluxos)**  
  URL: https://drive.google.com/file/d/1UD8nMWDZlyIQ3nRDyO3KRXhLvlsb0SLO/view?usp=sharing  

  Este material inclui:
  - Diagramas de alto nível (contexto, macro‑componentes).
  - Diagramas de módulos e integrações.
  - Fluxos principais de criação de transações, atualização de saldo diário e consumo de eventos.

### 7.3. Como solicitar acesso

Caso você precise acessar a documentação completa (wiki e diagramas), entre em contato por e‑mail solicitando a liberação de acesso.  
O acesso é concedido sob demanda, para garantir controle sobre quem visualiza os documentos proprietários da solução.

Ou seja, mesmo sem dividir em múltiplos serviços, o código adota as mesmas ideias de arquitetura propostas no documento, servindo como **prova de conceito** e base sólida para uma futura evolução para microserviços, se/quando necessário.
