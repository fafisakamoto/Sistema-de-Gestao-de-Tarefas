# ADR 0006: O que foi deliberadamente deixado de fora

**Status:** Aceita

| Não utilizado | Motivo |
|---|---|
| **MediatR** | Handlers injetados diretamente entregam o mesmo isolamento sem indireção nem licença comercial (desde 2025). |
| **AutoMapper** | Um único DTO de saída; o mapeamento manual é explícito e verificado pelo compilador. Também passou a ter licença comercial. |
| **Repositório genérico** | Ver [ADR 0004](0004-persistencia-consultas.md). |
| **Specification pattern** | Os filtros são composição simples de `Where`. Sem regras de consulta compartilhadas, seria abstração sem uso. |
| **Domain events** | Nenhum efeito colateral reage a mudanças de tarefa no escopo atual. |

Os itens abaixo são **funcionalidades novas**, não ajustes da estrutura atual. Cada uma se encaixa nos pontos
de extensão existentes, sem refatoração:

| Funcionalidade | Onde entra |
|---|---|
| Banco relacional + migrations | `Infrastructure/DependencyInjection.cs` e pacote do provider |
| Autenticação JWT e tarefas por usuário | Middleware na Api + filtro por usuário no repositório |
| Concorrência otimista (`ETag` / `If-Match`) | Token de versão na entidade + novo `ErrorType.Conflict` mapeado em `ProblemFrom` |
| OpenTelemetry | Composição na Api; os logs já são estruturados e correlacionados |
| Rate limiting | Middleware nativo do .NET 8 na Api |
