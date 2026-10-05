# ADR 0004: Persistência, paginação e ordenação

**Status:** Aceita

## Decisão

### Repositório e Unit of Work

- `ITaskRepository` específico do agregado, com apenas o que os casos de uso usam.
- **Repositórios não salvam.** O handler confirma via `IUnitOfWork`, implementado pelo próprio `AppDbContext`.
  Um caso de uso que altere vários agregados confirma tudo em uma única transação, sem mudança estrutural.
- Mapeamento em `IEntityTypeConfiguration`; índices em `Status` e `DueDate`.

### Consultas reutilizáveis

A paginação e a ordenação não pertencem a tarefas; são infraestrutura de consulta:

| Componente | Camada | Papel |
|---|---|---|
| `PagedQuery` | Application | Base de qualquer consulta paginada (`page`, `pageSize`, limites) |
| `PagedQueryValidator` | Application | Regras de paginação, incluídas por qualquer validador de consulta |
| `PageRequest` / `PagedResult<T>` | Application | Contrato de entrada e saída da paginação |
| `SortDirection` | Application | Direção de ordenação |
| `QueryableExtensions` | Infrastructure | `ToPagedResultAsync`, `OrderByDirection`, `ThenByDirection` para qualquer `IQueryable` |

Uma nova listagem paginada e ordenável (de outra entidade) herda `PagedQuery`, inclui `PagedQueryValidator`
e chama `ToPagedResultAsync`, sem duplicar nada.

### Regras da listagem de tarefas

- Ordenação por campos permitidos (`DueDate`, `CreatedAt`, `Title`) e direção (`Asc`/`Desc`). Os campos são um
  enum, nunca texto livre, o que impede ordenar por colunas arbitrárias.
- Tarefas sem vencimento ficam sempre por último ao ordenar por vencimento, em qualquer direção.
- **Desempate estável** (criação, depois id): sem ele, itens podem repetir ou sumir entre páginas.
- `pageSize` limitado a 100: nenhuma requisição carrega a base inteira.
- Leituras com `AsNoTracking`.

## Alternativas consideradas

- **`DbContext` direto nos handlers:** válido, mas acoplaria a Application ao EF Core (o que é verificado por teste).
- **Repositório genérico:** esconde a intenção e tende a vazar `IQueryable`.
- **Paginação por cursor (keyset):** melhor para volumes muito grandes e listas infinitas. Com a ordenação estável
  já existente, adotá-la seria uma nova implementação de consulta, sem mudar handlers ou contratos.

## Consequências

- ✅ Troca de provider restrita a `Infrastructure/DependencyInjection.cs`.
- ✅ Filtros, ordenação e paginação testados contra o EF Core real.
- ⚠️ O InMemory não é relacional; em produção, testes contra o banco real (Testcontainers) complementariam.
