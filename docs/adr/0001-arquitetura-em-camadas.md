# ADR 0001: Camadas com inversão de dependência e casos de uso independentes

**Status:** Aceita

## Contexto

O desafio pede separação de responsabilidades, SOLID e estrutura modular. Além de atender ao escopo atual,
a estrutura deve **absorver crescimento sem refatoração**: novos casos de uso, novas entidades, troca de banco.

## Decisão

### Camadas

```
Api ──► Application ──► Domain
 │           ▲
 └──► Infrastructure
```

| Projeto | Responsabilidade | Depende de |
|---|---|---|
| **Domain** | Entidade `TaskItem`, invariantes e comportamento | nada |
| **Application** | Casos de uso, contratos de entrada/saída, validação, portas (`ITaskRepository`, `IUnitOfWork`) | Domain |
| **Infrastructure** | EF Core, repositório, consultas, relógio de negócio | Application |
| **Api** | HTTP, ProblemDetails, middleware, Swagger, composição | Application, Infrastructure |

### Um handler por caso de uso

Cada operação é uma classe própria (`CreateTaskHandler`, `UpdateTaskHandler`, `DeleteTaskHandler`,
`GetTaskByIdHandler`, `ListTasksHandler`), agrupada em uma pasta com seu request e seu validador:

```
Application/Tasks/
├── Create/   CreateTaskHandler · CreateTaskRequest · CreateTaskRequestValidator
├── Update/   UpdateTaskHandler · UpdateTaskRequest · UpdateTaskRequestValidator
├── Delete/   DeleteTaskHandler
├── GetById/  GetTaskByIdHandler
├── List/     ListTasksHandler · ListTasksQuery · ListTasksQueryValidator · TaskListCriteria
└── (compartilhados) ITaskRepository · TaskResponse · TaskErrors · TaskRules · TaskMappings
```

Motivos:

- **SRP real:** cada handler recebe apenas o que usa. A exclusão não conhece validadores; a consulta não conhece
  `IUnitOfWork`. Um serviço único acumularia todas as dependências de todas as operações.
- **Open/Closed:** adicionar um caso de uso é **criar uma pasta**. Nenhum arquivo existente muda, nem o
  registro de DI, que é feito por convenção (classes públicas terminadas em `Handler`).
- **Testes focados:** cada classe de teste monta só as dependências do caso de uso que testa.
- O controller recebe o handler por ação (`[FromServices]`), então cada endpoint só resolve o que precisa.

As convenções (handlers selados, um handler não depende de outro, camadas) são **verificadas por testes de arquitetura**.

## Alternativas consideradas

- **Um `TaskService` com todas as operações:** simples no início, mas o construtor cresce a cada caso de uso
  e cada mudança toca a mesma classe.
- **MediatR:** resolveria o despacho, mas adiciona indireção, comportamento implícito (pipelines) e, desde 2025,
  licença comercial. A injeção direta do handler entrega o mesmo isolamento de forma explícita.
- **Projeto único com pastas:** fronteiras dependeriam só de disciplina.

## Consequências

- ✅ Crescimento por adição, não por modificação.
- ✅ Domínio e casos de uso testáveis sem HTTP e sem banco; troca de banco restrita à Infrastructure.
- ⚠️ Mais arquivos que um CRUD mínimo; aceito em troca de coesão e previsibilidade.
