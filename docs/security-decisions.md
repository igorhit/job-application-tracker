# Decisões de Segurança — Application Tracker

## Estado atual

Este documento registra as decisões de segurança planejadas para o Projeto 2.

Como a implementação ainda não começou, ele existe para capturar direção arquitetural e evitar que a segurança fique como preocupação tardia.

## Decisões já encaminhadas

### Autenticação obrigatória

O projeto terá autenticação desde o início. Empresas, candidaturas e notas serão sempre isoladas por usuário.

### Isolamento por usuário

Toda leitura e escrita de dados deve respeitar escopo do usuário autenticado. O sistema não deve permitir acesso a registros de terceiros.

### Seed demo controlado

Haverá seed demo para facilitar avaliação do projeto, mas ele deve ser controlado por configuração e não misturado silenciosamente com ambientes não destinados a demonstração.

## Decisões a definir durante a implementação

### Estratégia de autenticação

Opções em avaliação:

- JWT
- cookies HTTP-only

A decisão final deve equilibrar simplicidade de implementação, segurança razoável e boa ergonomia para o frontend.

### Armazenamento de sessão

Se a aplicação usar refresh token ou sessões mais longas, a estratégia de renovação e invalidação deve ser documentada.

### Proteção básica de aplicação

Itens esperados:

- validação de entrada
- tratamento centralizado de erros
- headers de segurança no backend
- rate limiting se fizer sentido para endpoints sensíveis
- segredos fora do repositório

### Dados demo

O seed de demonstração deve usar credenciais explícitas no README, sem depender de conhecimento implícito do avaliador.
