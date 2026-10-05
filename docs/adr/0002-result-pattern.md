# ADR 0002: Result pattern para erros esperados

**Status:** Aceita

## Contexto

Os casos de uso têm falhas **esperadas** (entrada inválida, tarefa inexistente). Lançar exceções para isso
faz com que a assinatura não revele que a operação pode falhar, transforma exceções em controle de fluxo
e mistura erros de negócio com bugs nos logs e métricas.

## Decisão

- Handlers retornam `Result` / `Result<T>` com um `Error` (`Code`, `Message`, `Type`).
- A Application não conhece HTTP. A tradução `ErrorType → status` fica em um único método:
  `ApiControllerBase.ProblemFrom`. Um novo tipo de erro (ex.: `Conflict`) exige mudar apenas esse ponto.
- Exceções ficam para o excepcional (bugs, infraestrutura, violação de invariante do domínio), tratadas pelo
  `GlobalExceptionHandler`.
- Todo erro tem um código estável (`Task.NotFound`, `Validation.Failed`), exposto como `errorCode` no ProblemDetails.

## Alternativas consideradas

- **Exceções + middleware:** menos código, com os problemas descritos acima.
- **FluentResults / ErrorOr / OneOf:** boas opções; uma implementação de cerca de 40 linhas evita uma dependência
  para algo simples e mantém o contrato sob controle.

## Consequências

- ✅ Assinaturas honestas; logs de nível Error refletem apenas problemas reais.
- ✅ Testes verificam o tipo de erro sem `Assert.ThrowsAsync`.
- ⚠️ Os controllers checam `IsSuccess`; o padrão é uniforme e a tradução é centralizada.
