# SECURITY.md — Modelo de segurança do Buzzy

> Registra as regras de segurança do produto e separa intenção de implementação real.
>
> Última atualização: 2026-09-26

## Modelo atual

STATUS: PLANNED. O produto ainda não tem código. O princípio de segurança está definido: Buzzy deve ser local, previsível, transparente e auditável, com permissões mínimas e explícitas.

## Capacidades fora do MVP

O MVP não deve executar comandos arbitrários, shell, PowerShell ou CMD; iniciar processos ocultos; fazer keylogging ou captura silenciosa de tela; coletar dados remotamente; baixar ou executar código desconhecido; manter persistência escondida; nem enviar telemetria ou analytics.

Fable 5.1/Claude pode ser usado como ferramenta de desenvolvimento. Essa assistência não autoriza dependência de IA, chamadas a API ou envio de dados do usuário pelo aplicativo.

## Fronteira de permissões

STATUS: PLANNED. A Fase 0 deve identificar cada API do sistema operacional que a stack precisa e justificar seu uso. Uma capacidade futura deve seguir:

intenção explícita → capacidade específica → permissão limitada → ação permitida e auditável.

Não introduzir shell genérico, ferramenta genérica de controle do computador ou permissões amplas. A fase de hardening verifica a implementação; os limites precisam fazer parte do desenho desde a Fase 0.

## Dados e persistência

STATUS: PLANNED. A persistência permitida no MVP é local e limitada às configurações, posição, monitor preferido e demais preferências aprovadas na PRODUCT_SPEC.md. Não há memória de IA, sincronização em nuvem ou envio remoto. O formato e a localização ainda dependem da stack e serão registrados após a decisão técnica.

## Rede e dependências

STATUS: PLANNED. O MVP não precisa de backend nem de API de IA. Cada dependência deve ter justificativa e versão revisada. A necessidade de rede deve ser nula para o comportamento básico do produto.

## Verificação de segurança

STATUS: PLANNED. A Fase 0 define a estratégia e os cenários de verificação. Todas as fases testam suas próprias capacidades. A Fase 9 revisa armazenamento, permissões, dependências, processos e comunicação externa antes da verificação integrada.
