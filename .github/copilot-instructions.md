# Copilot Instructions

## Project Guidelines
- User uses a `Cashflow.slnx` solution file in the repo root and is sensitive to claims about non-existent `Cashflow.sln`; prefer generating or wiring up a standard `.sln` file manually instead of relying on `dotnet new sln` commands.

## Code and Database Standards
- Represent monetary values in cents using `long` in both code and database, with tests adjusted accordingly.

## API Specifications
- The `/balances/daily` endpoint should return a single snapshot per tenant per day.
- Expose `IdempotenceKey` in `TransactionsController` for API idempotency.