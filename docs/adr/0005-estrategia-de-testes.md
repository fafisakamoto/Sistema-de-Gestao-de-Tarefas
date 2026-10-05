# ADR 0005: Estratégia de testes

**Status:** Aceita

| Projeto | O que garante | Como |
|---|---|---|
| `UnitTests/Domain` | Invariantes da entidade e cálculo de atraso | Test Data Builder |
| `UnitTests/Validators` | Cada regra, incluindo limites exatos e fuso de negócio | `FluentValidation.TestHelper` |
| `UnitTests/UseCases` | Cada handler: sucesso, validação, não encontrado e propagação de cancelamento | Moq nas fronteiras, `FixedTimeProvider` |
| `IntegrationTests/Persistence` | Filtros, busca, ordenação, desempate e paginação contra o EF Core real | Banco InMemory isolado por teste |
| `IntegrationTests/Api` | Contrato HTTP: status codes, `Location`, ProblemDetails, correlation id, health, Swagger | `WebApplicationFactory<Program>` |
| `ArchitectureTests` | Camadas, convenções de handlers, repositórios internos, classes seladas | NetArchTest |

## Princípios

- **Mockar só as fronteiras** (repositório, unidade de trabalho). Os validadores são reais, porque fazem parte da regra.
- **Determinismo:** o tempo é injetado; nenhum teste depende do relógio da máquina.
- **Isolamento:** banco próprio por fixture e por teste de repositório.
- **Um arquivo de teste por caso de uso**, espelhando a estrutura de produção.
- Asserções nativas do xUnit (a FluentAssertions v8 passou a ter licença comercial).
