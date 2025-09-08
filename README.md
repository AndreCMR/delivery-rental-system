# Delivery Rental System  
Sistema para gerenciamento de aluguel de motos e entregadores, seguindo Clean Architecture, .NET 8, integração com PostgreSQL, RabbitMQ e MinIO.

## Sumário
- [Descrição](#descrição)  
- [Arquitetura](#arquitetura)  
- [Tecnologias](#tecnologias)  
- [Como rodar](#como-rodar)  
- [Testes](#testes)  
- [APIs e Casos de Uso](#apis-e-casos-de-uso)  
- [Ambiente Docker](#ambiente-docker)  
- [Contato](#contato)  

---

## Descrição
O **Delivery Rental System** é uma aplicação backend para gerenciar motos, entregadores e locações.  

Principais funcionalidades:
- Cadastro, consulta e alteração de motos com validações de placa única.  
- Cadastro e atualização de entregadores, incluindo upload e atualização da imagem da CNH.  
- Registro e controle de locações de motos, respeitando planos de dias e valores definidos.  
- Publicação de eventos de moto cadastrada em RabbitMQ.  
- Consumo de eventos para notificar quando o ano da moto for **2024** e persistência em banco.  
- Armazenamento de imagens de CNH em **MinIO** (ou outro storage configurado).  

---

## Arquitetura
- **Domain**: Entidades, interfaces, eventos de domínio e regras de negócio.  
- **Application**: Commands, Queries, validações (`FluentValidation`), Use Cases e orquestração.  
- **Infrastructure**:  
  - Repositórios (Entity Framework Core).  
  - Mensageria (RabbitMQ via MassTransit).  
  - Storage (MinIO).  
  - Contexto de dados (SQLite em memória para testes / PostgreSQL em produção).  
- **Presentation (API)**: Controllers REST, middlewares de exceção e configuração de dependências.  
- **Tests**: Testes de unidade e integração com **xUnit** e **FluentAssertions**.  

---

## Tecnologias
- .NET 8  
- Entity Framework Core  
- PostgreSQL / SQLite (para testes de integração)  
- RabbitMQ (MassTransit)  
- MinIO (storage de imagens)  
- xUnit e FluentAssertions (testes)  
- Docker e Docker Compose  

---

## Como rodar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- [Docker](https://www.docker.com/) e [Docker Compose](https://docs.docker.com/compose/)  

### Passos
1. Clone o repositório:
   ```bash
   git clone <url-do-repo>
   cd delivery-rental-system
   ```

2. Configure as variáveis de ambiente no `.env` (exemplo em `appsettings.Development.json`).

3. Suba os serviços com Docker Compose:
   ```bash
   docker-compose --env-file .env up --build
   ```

4. Acesse a API via Swagger:
   - [http://localhost:5000/swagger](http://localhost:5000/swagger)  

---

## Testes
Testes de unidade e integração estão no projeto `IntegrationTests/`.  

Para rodar:
```bash
dotnet test
```

Os testes de integração utilizam:
- **SQLite in-memory** para persistência.  
- **MassTransit TestHarness** para validar eventos publicados e consumidos.  
- Reset de banco (`IAsyncLifetime`) para isolamento de cada teste.  

---

## APIs e Casos de Uso
- **Motos**
  - Criar, consultar, alterar placa e remover motos.  
  - Validação de duplicidade de placa.  
  - Geração de evento de moto cadastrada e consumo para motos do ano 2024.  

- **Entregadores**
  - Cadastro e atualização (nome, CNPJ, CNH, tipo de CNH, data de nascimento).  
  - Upload/atualização de imagem da CNH (png ou bmp) em MinIO.  

- **Locações**
  - Aluguel de motos em planos de 7, 15, 30, 45 e 50 dias, com preços definidos.  
  - Validação de entregador habilitado na categoria A.  
  - Cálculo de multas por devolução antecipada e acréscimo por atraso.  
  - Regra: não é possível remover uma moto com locações associadas.  

📌 Consulte o Swagger da aplicação para detalhes de endpoints e contratos.  

---

## Ambiente Docker
- **delivery-rental-system-api**: API principal (.NET 8).  
- **postgres**: Banco de dados relacional.  
- **rabbitmq**: Servidor de mensageria.  
- **minio**: Armazenamento de imagens (CNH).  

---
