# Projeto de Integração de Comunicação Serial

Este projeto contém exemplos de código para comunicação serial entre um Arduino e uma aplicação C#. O Arduino envia e recebe pacotes de dados utilizando um protocolo específico, e a aplicação C# se comunica com o Arduino por meio de uma porta serial.

## **Código do Arduino**

### Passos para subir o código na IDE do Arduino:

1. **Instale a IDE do Arduino**: Se ainda não a tiver, faça o download e instale a IDE do Arduino [aqui](https://www.arduino.cc/en/software).

2. **Baixe as bibliotecas necessárias**:
   - Para este projeto, você pode não precisar de bibliotecas adicionais, mas caso precise de algum driver específico ou biblioteca para o seu modelo de Arduino, instale-a diretamente pela IDE:
     - Vá em **Sketch** > **Include Library** > **Manage Libraries** e procure pela biblioteca desejada.

3. **Conecte o Arduino ao computador**:
   - Conecte seu Arduino à porta USB do computador.
   - Certifique-se de que o Arduino esteja sendo detectado corretamente pela IDE.

4. **Selecione o modelo do Arduino**:
   - Vá em **Tools** > **Board** e escolha o modelo do seu Arduino (por exemplo, Arduino Uno).

5. **Escolha a porta correta**:
   - Vá em **Tools** > **Port** e selecione a porta serial à qual o seu Arduino está conectado. Caso não saiba qual porta, você pode verificar no Gerenciador de Dispositivos do Windows.

6. **Carregue o código**:
   - Abra o arquivo `.ino` do Arduino contido neste repositório na IDE do Arduino.
   - Clique no botão **Upload** para carregar o código no Arduino.

Após carregar o código, o Arduino estará pronto para se comunicar com a aplicação C#.

---

## **Código C#**

### Passos para configurar o código C#:

1. **Instale as bibliotecas necessárias**:
   - O código C# utiliza a biblioteca `System.IO.Ports` para a comunicação serial. Essa biblioteca já faz parte do .NET Framework, mas caso esteja usando o .NET Core ou .NET 5+, você pode precisar adicionar o pacote `System.IO.Ports` ao seu projeto:
     ```bash
     dotnet add package System.IO.Ports
     ```

2. **Configuração da porta serial**:
   - No código C#, é necessário configurar corretamente a porta serial que o Arduino está utilizando. Altere o valor de `"COM11"` para a porta que você está usando no seu sistema. A porta pode ser diferente dependendo do sistema operacional (Windows, Linux, etc.).
   - Para verificar qual porta está sendo utilizada, consulte o Gerenciador de Dispositivos do Windows ou o comando `ls /dev/tty*` no Linux.

3. **Bibliotecas e dependências**:
   - Certifique-se de que todas as bibliotecas e dependências estejam corretamente configuradas no seu projeto.
   - Se estiver utilizando um banco de dados local, verifique a configuração do SQLite no seu projeto.

4. **Rodando a aplicação**:
   - Após a configuração, compile e execute o código C#.
   - A aplicação irá se comunicar com o Arduino através da porta serial, enviando comandos e recebendo respostas.

---

## **Protocolo de Comunicação - Estrutura do Pacote**

Pacote de dados utilizado na comunicação entre o hardware e o software, facilitando o envio e recebimento de informações. A estrutura do pacote é composta pelas seguintes partes, nesta ordem: **Header**, **ID**, **Tipo de dado**, **Tamanho dos dados**, **Dados (payload)**, **Checksum** e **Footer**.

## Estrutura do Pacote

### 1. **Header (Início do pacote)**  
- **Tamanho:** 1 byte  
- **Valor:** `0xAA`  
- **Descrição:** Identificador fixo para marcar o início do pacote.

### 2. **ID**  
- **Tamanho:** 2 bytes  
- **Valor:** Número de 0 a 65535 (identifica o tipo ou sequência de pacotes).  
  O valor do ID pode ser representado no formato **little-endian**.

### 3. **Tipo de Dado (Data Type)**  
- **Tamanho:** 1 byte  
- **Valor:** Código que especifica o tipo de dado presente no payload.  
  O tipo de dado é baseado no enum `PayloadType`, com os seguintes valores:
  - `1`: **Int32**
  - `2`: **Float**
  - `3`: **Bool**
  - `4`: **String**

### 4. **Tamanho dos Dados (Data Length)**  
- **Tamanho:** 1 byte  
- **Valor:** Quantidade de bytes do campo **Dados (Payload)**.

### 5. **Dados (Payload)**  
- **Tamanho:** Variável (definido pelo campo **Tamanho dos dados**)  
- **Valor:** Conteúdo real do pacote, de acordo com o tipo de dado especificado.

### 6. **Checksum**  
- **Tamanho:** 1 byte  
- **Valor:** Valor resultante da soma dos bytes do pacote (incluindo o Header, mas não o Footer).  
  O checksum é calculado utilizando a operação módulo 256 para garantir que o valor caiba em um único byte.

### 7. **Footer (Fim do pacote)**  
- **Tamanho:** 1 byte  
- **Valor:** `0xFF`  
- **Descrição:** Identificador fixo para marcar o final do pacote.

---

## Exemplos de Pacote

### Exemplo 1: Envio de um **Int32** com Valor `1234`

- **Header:** `0xAA`  
- **ID:** `0x01, 0x00` (Representação little-endian de `0x0100`)  
- **Tipo de Dado:** `0x01` (Int32)  
- **Tamanho dos Dados:** `0x04` (4 bytes, pois um `Int32` ocupa 4 bytes)  
- **Dados (Payload):** `0xD2 0x04 0x00 0x00` (Valor `1234` em formato little-endian)  
- **Checksum:** `0x86` (Cálculo descrito abaixo)  
- **Footer:** `0xFF`

**Bytes Finais:**  
`AA 01 00 01 04 D2 04 00 00 86 FF`

#### Cálculo do Checksum para o Exemplo 1:

Para calcular o checksum, somam-se todos os bytes do pacote (incluindo o Header, mas não o Footer), utilizando a operação módulo 256 para garantir que o valor caiba em um único byte:

1. **Header:** `0xAA`  
2. **ID:** `0x01` e `0x00` → `0x01 + 0x00 = 0x01`
3. **Tipo de Dado:** `0x01` (1, conforme Tipo de Dado descrito na estrutura do pacote)
4. **Tamanho dos Dados:** `0x04`
5. **Dados (Payload):** `0xD2 0x04 0x00 0x00` → `0xD2 + 0x04 + 0x00 + 0x00 = 0xD6`

Soma total dos bytes:

- `0xAA + 0x01 + 0x00 + 0x01 + 0x04 + 0xD2 + 0x04 + 0x00 + 0x00 = 0x186`

Agora, aplica-se o módulo 256 ao resultado em decimal:

- `0x186 para decimal = 390`
- `390 % 256 = 134`

E por fim convertemos o resultado de volta para HEX:

- `134 para hexadecimal = 86`

Portanto, o **Checksum calculado** é `0x86`.

---

### Exemplo 2: Envio de uma **String** `"Hello"`

- **Header:** `0xAA`  
- **ID:** `0x01, 0x00` (Representação little-endian de `0x0100`)  
- **Tipo de Dado:** `0x04` (String)  
- **Tamanho dos Dados:** `0x05` (5 bytes, pois a string `"Hello"` tem 5 caracteres)  
- **Dados (Payload):** `0x48 0x65 0x6C 0x6C 0x6F` (Valores ASCII de `"Hello"`)  
- **Checksum:** `0xA8` (Cálculo descrito abaixo)  
- **Footer:** `0xFF`

**Bytes Finais:**  
`AA 01 00 04 05 48 65 6C 6C 6F A8 FF`

#### Cálculo do Checksum para o Exemplo 2:

Para calcular o checksum, somam-se todos os bytes do pacote (incluindo o Header, mas não o Footer), utilizando a operação módulo 256 para garantir que o valor caiba em um único byte:

1. **Header:** `0xAA`  
2. **ID:** `0x01` e `0x00` → `0x01 + 0x00 = 0x01`
3. **Tipo de Dado:** `0x04` (String)
4. **Tamanho dos Dados:** `0x05`
5. **Dados (Payload):** `0x48 0x65 0x6C 0x6C 0x6F` → `0x48 + 0x65 + 0x6C + 0x6C + 0x6F = 0x1F4`

Soma total dos bytes:

- `0xAA + 0x01 + 0x00 + 0x04 + 0x05 + 0x1F4 = 0x2A8`

Agora, aplica-se o módulo 256 ao resultado em decimal:

- `0x2A8 para decimal = 680`
- `680 % 256 = 168`

E por fim convertemos o resultado de volta para HEX:

- `168 para hexadecimal = A8`

Portanto, o **Checksum calculado** é `0xA8`.

---

## Planilha do Protocolo

| Parte             | Descrição                              | Tamanho (em bytes) | Valor Exemplo                     |
|-------------------|----------------------------------------|--------------------|-----------------------------------|
| **Header**        | Início do pacote                       | 1 byte             | `0xAA`                            |
| **ID**            | Identificador do pacote                | 2 bytes            | `0x01 0x00`                       |
| **Tipo de Dado**  | Identifica o tipo de dado no payload   | 1 byte             | `0x01` (Int32)                    |
| **Tamanho Dados** | Tamanho dos dados (payload)            | 1 byte             | `0x04` (4 bytes, Int32)           |
| **Dados**         | Conteúdo real do pacote                | Variável           | `0xD2 0x04 0x00 0x00` (1234)      |
| **Checksum**      | Verificação de integridade             | 1 byte             | `0x86`                            |
| **Footer**        | Final do pacote                        | 1 byte             | `0xFF`                            |

| Parte             | Descrição                              | Tamanho (em bytes) | Valor Exemplo                     |
|-------------------|----------------------------------------|--------------------|-----------------------------------|
| **Header**        | Início do pacote                       | 1 byte             | `0xAA`                            |
| **ID**            | Identificador do pacote                | 2 bytes            | `0x01 0x00`                       |
| **Tipo de Dado**  | Identifica o tipo de dado no payload   | 1 byte             | `0x04` (String)                   |
| **Tamanho Dados** | Tamanho dos dados (payload)            | 1 byte             | `0x05` (5 bytes, String)          |
| **Dados**         | Conteúdo real do pacote                | Variável           | `0x48 0x65 0x6C 0x6C 0x6F` (Hello)|
| **Checksum**      | Verificação de integridade             | 1 byte             | `0xA8`                            |
| **Footer**        | Final do pacote                        | 1 byte             | `0xFF`                            |

---
