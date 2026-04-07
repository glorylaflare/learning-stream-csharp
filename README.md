# Learning Stream C# - SFTP

Este projeto foi desenvolvido como um estudo prático para entender e implementar o conceito de SFTP (Secure File Transfer Protocol) em aplicações .NET. O foco principal é aprender como transferir arquivos de forma segura usando SFTP, incluindo autenticação com senha ou chave privada.

## Descrição do Projeto

O projeto simula um worker (serviço em background) que executa tarefas periodicamente para processar e enviar dados. A lógica implementada inclui:

1. **Leitura de Dados**: Conecta ao MongoDB e recupera registros de uma coleção de pessoas.
2. **Geração de CSV**: Converte os dados em um arquivo CSV estruturado.
3. **Compactação**: Cria um arquivo ZIP contendo o CSV.
4. **Envio via SFTP**: Transfere o arquivo ZIP para um servidor SFTP remoto de forma segura.

Este fluxo demonstra um caso de uso comum em sistemas de integração de dados, onde informações precisam ser exportadas e enviadas para sistemas externos.

## Tecnologias Utilizadas

- **.NET 8**: Framework principal para desenvolvimento.
- **MongoDB.Driver**: Para interação com o banco de dados MongoDB.
- **Renci.SshNet**: Biblioteca para implementação do cliente SFTP.
- **Microsoft.Extensions.Hosting**: Para criação e gerenciamento do worker hospedado.
- **System.IO.Compression**: Para criação de arquivos ZIP.

## Estrutura do Projeto

O projeto está organizado em camadas seguindo princípios de arquitetura limpa:

- **Person.Domain**: Contém os modelos de domínio (ex: classe `Pessoa`).
- **Person.Application**: Define interfaces para os serviços (ex: `IMongoDbService`).
- **Person.Infra**: Implementa os serviços de infraestrutura:
  - `MongoDbService`: Acesso ao MongoDB.
  - `CsvGenerateService`: Geração de arquivos CSV.
  - `ZipService`: Compactação de arquivos.
  - `SftpService`: Envio via SFTP.
- **Person.Worker**: Contém o worker hospedado que orquestra o processo.

## Pré-requisitos

- .NET 8 SDK instalado.
- MongoDB em execução (local ou remoto).
- Servidor SFTP acessível (para testes, pode usar serviços como SFTP Cloud ou configurar um local).

## Configuração

As configurações são feitas no arquivo `appsettings.json` do projeto `Person.Worker`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "local",
    "CollectionName": "Pessoas"
  },
  "Sftp": {
    "Host": "seu-servidor-sftp.com",
    "Port": 22,
    "Username": "seu-usuario",
    "Password": "sua-senha",
    "PrivateKeyPath": "",  // Caminho para chave privada, se usar autenticação por chave
    "RemoteDirectory": "/uploads/dados"
  },
  "Worker": {
    "IntervaloMinutos": 60,
    "LimiteRegistros": 10000,
    "ApenasRegistrosAtivos": true
  }
}
```

### Autenticação SFTP

O projeto suporta dois métodos de autenticação:
- **Por senha**: Configure `Password` no `appsettings.json`.
- **Por chave privada**: Configure `PrivateKeyPath` com o caminho para o arquivo de chave privada (formato OpenSSH).

O código permite fallback: se ambos estiverem configurados, tenta chave privada primeiro, depois senha.

## Como Executar

1. Clone ou baixe o projeto.
2. Navegue até a pasta do projeto: `cd learning-stream-csharp`.
3. Restaure as dependências: `dotnet restore`.
4. Execute o worker: `dotnet run --project src/Person.Worker`.

O worker iniciará e executará o processo de acordo com o intervalo configurado.

## Demonstração

Abaixo, uma captura de tela mostrando o sucesso na execução do worker e envio dos dados via SFTP:

![Sucesso na Requisição](resources/images/Screenshot%202026-04-07%20204437.png)

## Funcionalidades Implementadas

- **Streaming de Arquivos**: O envio via SFTP usa streams para evitar carregamento completo na memória.
- **Logs Estruturados**: Uso de `ILogger` para logging detalhado das operações.
- **Configuração Flexível**: Suporte a diferentes ambientes via `IOptions`.
- **Tratamento de Erros**: Try-catch e logs para falhas em conexões e operações.
- **Validações**: Verificações de configurações obrigatórias no startup.

## Conceitos Aprendidos

Este projeto aborda conceitos importantes como:
- Protocolo SFTP e segurança na transferência de arquivos.
- Autenticação SSH (senha vs. chave privada).
- Integração com MongoDB em .NET.
- Geração dinâmica de arquivos CSV e ZIP.
- Desenvolvimento de workers hospedados com .NET.
- Injeção de dependência e configurações.

## Licença

Este projeto é para fins educacionais e não possui licença específica.</content>