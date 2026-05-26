# Credit Analysis System

Este projeto utiliza uma arquitetura de microserviços com .NET (API e Worker), PostgreSQL e RabbitMQ.

## 🚀 Como Rodar com Docker Compose (Recomendado)

A maneira mais fácil de rodar o projeto completo é usando o Docker. Você não precisa ter .NET ou Node instalados na sua máquina.

1. Certifique-se de ter o Docker instalado.
2. Configure suas credenciais no arquivo `.env` na raiz do projeto e execute:
   ```bash
   docker compose up --build
   ```
3. Aguarde o build e a inicialização. A API e o Worker possuem lógica de re-tentativa para esperar o RabbitMQ ficar pronto.

4. **Acessos:**
   - **API (Swagger):** http://localhost:5261/swagger
   - **RabbitMQ Dashboard:** http://localhost:15672 (guest/guest)

5. Para parar os serviços: `docker compose down`

## 💻 Desenvolvimento Local

Caso queira rodar apenas a infraestrutura no Docker e o código na sua máquina:
1. Suba a infra: `docker compose up postgres rabbitmq -d`. O Postgres estará na porta **5433**.
2. Configure o `.env` no backend apontando para `Host=localhost;Port=5433;...`.
3. Rode o backend: `dotnet run` na pasta da API e do Worker.
