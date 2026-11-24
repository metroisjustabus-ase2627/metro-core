# Metro Core

Projeto de API .NET 10 em Clean Architecture.

## Arquitetura

A solução segue Clean Architecture com CQRS (MediatR). A regra de dependência é **de fora para dentro**: Presentation e Infrastructure dependem de Application/Domain; o Domain não conhece frameworks nem banco. O MediatR fica só em `Metro.Application` e `Metro.Api`.

```
src/
├── Presentation/         # Host HTTP (controllers, DI, pipeline)
│   └── Metro.Api
├── Application/          # Use cases (commands, queries, handlers, ApiResult, MediatR)
│   └── Metro.Application
├── Domain/               # Regras de negócio e contratos
│   ├── Metro.Domain      # Entidades, interfaces de repositório/serviços
│   └── Metro.Shared      # Abstrações compartilhadas (Entity, UoW…)
└── Infrastructure/       # Implementações técnicas
    ├── Metro.Infrastructure.PostgreSQL        # EF Core (writes + migrations)
    ├── Metro.Infrastructure.PostgreSQL.Dapper # Dapper (reads)
    ├── Metro.Infrastructure.Auth              # JWT e hash de senha
    ├── Metro.Infrastructure.Email             # SMTP (MailKit)
    └── Metro.Infrastructure.File              # Storage local de arquivos

tests/
├── Metro.Application.Tests  # Testes unitários dos handlers
└── Metro.Domain.Tests       # Testes de regras de domínio
```



### Fluxo de uma request

1. Controller (Presentation) recebe HTTP e monta um `Command` ou `Query`.
2. MediatR despacha para o handler correspondente na Application.
3. Handler usa repositórios/serviços (interfaces do Domain) e retorna `ApiResult<T>`.
4. `ApiController.FromResult` mapeia o status HTTP.

**Writes** (create/update/delete) passam pelo EF Core + Unit of Work.  
**Reads** (get/list) usam Dapper via query repositories.


### Como adicionar um novo módulo

1. Criar entidades/interfaces em `Metro.Domain`.
2. Criar commands/queries/handlers/view models em `Metro.Application`.
3. Criar validators FluentValidation do command/query (pipeline MediatR).
4. Implementar repositórios/mappings na Infrastructure.
5. Expor controller em `Metro.Api/Controllers` no padrão REST (`/{id}` na rota).
6. Se precisar de tabela nova: gerar migration (seção Comandos).

---

## Comandos

Na raiz do repositório:

```bash
make watch                          # API com hot reload
make run                            # API sem watch
make build                          # Compila a solution
make clean                          # Limpa bin/ e obj/
make test                           # Roda os testes unitários
make migration name=NomeDaMigration # Cria uma migration
make db-update                      # Aplica migrations no banco
make docker-up                      # Build + sobe API + Postgres no Docker
make docker-start                   # Liga containers já criados (sem rebuild)
make docker-down                    # Para os containers
make docker-logs                    # Segue os logs da API
```

---

## CI / CD / CDE e Deploy em VMs DEI (Build Only Once)

As responsabilidades estão separadas em 3 workflows (`T03`, `ADR-004`):
1. **CI (`.github/workflows/ci.yml`)**: `dotnet build -c Release`, testes unitários xUnit com cobertura Cobertura (`coverlet.collector`), formatação (`.editorconfig`), `.NET Analyzers`, SCA NuGet bloqueante, Gitleaks secret scan (`CON-DEP-05`) e publicação da **imagem Docker imutável** `ghcr.io/metroisjustabus-ase2627/metro-core:<commit-sha>` com `APP_VERSION=<commit-sha>` em `GET /health` (`CON-CI-02`).
2. **CD — Development VM (`.github/workflows/cd-dev.yml`)**: dispara automaticamente após CI verde em `main`, faz `pull` da mesma imagem `<commit-sha>` na VM Dev (`docker-compose.dev.yml`), valida `/health` (status `Healthy` + SHA) e executa rollback automático em caso de falha (`CON-CI-04`).
3. **CDE — Production VM (`.github/workflows/cd-prod.yml`)**: promoção controlada (`workflow_dispatch` + aprovação humana no GitHub Environment `production`) da mesma imagem `<commit-sha>` validada em Dev (`docker-compose.prod.yml`), com smoke check `/health` + rollback automático.

### Provisionar e ativar as VMs DEI (`CON-DEP-02`, `CON-DEP-03`)
1. Correr `sudo bash infra/bootstrap-vm.sh dev` (ou `prod`) na VM Ubuntu do DEI.
2. Criar `.env.dev` / `.env.prod` na VM (baseado em `.env.example`, com `JWT_KEY`, `POSTGRES_PASSWORD`, `SEED_STAFF_EMAIL` e `SEED_STAFF_PASSWORD`).
3. **Recomendado (self-hosted runner na VM DEI):** registar o runner com labels `self-hosted,linux,dev` (ou `prod`) e definir a repository variable `DEV_RUNNER` / `PROD_RUNNER` (ou `CI_RUNNER`).
4. **Alternativa (SSH):** definir `DEV_SSH_HOST`/`DEV_SSH_USER` + secret `DEV_SSH_KEY` (ou `PROD_*`).


