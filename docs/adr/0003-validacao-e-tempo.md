# ADR 0003: Validação em duas camadas e relógio de negócio

**Status:** Aceita

## Contexto

Há dois tipos de regra:

1. **Regras de entrada**, que dependem do contexto. Exemplo: uma tarefa *nova* não pode vencer no passado,
   mas uma tarefa *existente* pode ficar atrasada com o tempo.
2. **Invariantes**, que valem sempre. Exemplo: nunca existe tarefa sem título ou com status inválido.

Além disso, "hoje" é um conceito de negócio. Um servidor em UTC diria que 22h de 10/01 em São Paulo já é 11/01.

## Decisão

### Validação

- **FluentValidation** na Application para regras de entrada: mensagens em PT-BR, erros por campo, testável.
- **A entidade protege as invariantes** com setters privados e validação em `Create`/`Update`. Ela valida
  tudo antes de alterar qualquer campo: a operação é aplicada por inteiro ou nada muda.
- Limites (`TitleMaxLength` etc.) são constantes do domínio usadas pelos validadores e pelo mapeamento do EF:
  uma única fonte da verdade.
- Regras comuns ficam em `TaskRules`; regras de paginação ficam em `PagedQueryValidator`, incluído por qualquer consulta.
- O `[Required]` implícito do MVC foi desligado; enums só são aceitos como texto (`"status": 99` é rejeitado).

### Tempo

- `DateOnly` para vencimento: vencimento é um dia, não um instante.
- `TimeProvider` (.NET 8) injetado em todo lugar que depende de tempo; nenhum `DateTime.Now` no código.
- `BusinessTimeProvider` define o fuso local como o **fuso de negócio** configurado em `Clock:TimeZone`
  (padrão `America/Sao_Paulo`). "Hoje" é sempre calculado nesse fuso, em qualquer servidor.
- O fuso é **validado na inicialização** (`ValidateOnStart`): uma configuração inválida impede a aplicação de subir,
  em vez de gerar datas erradas silenciosamente.
- Instantes (`CreatedAt`, `UpdatedAt`) continuam em UTC.

## Consequências

- ✅ Impossível persistir entidade inválida; regras explícitas e testadas.
- ✅ Comportamento de datas idêntico em desenvolvimento, container e produção.
- ✅ Suporte a múltiplos fusos (por usuário, por tenant) exige apenas outra implementação de `TimeProvider`.
