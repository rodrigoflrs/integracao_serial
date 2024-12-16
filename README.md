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

## **Estrutura do Pacote**

O pacote enviado e recebido entre o Hardware e o Software segue uma estrutura bem definida para facilitar a comunicação. Essa estrutura é dividida nas seguintes partes, nessa ordem: `Header`, `ID`, `Tipo de dado`, `Tamanho dos dados`, `Dados (payload)`, `Checksum` e `Footer`.

### Detalhes da Estrutura:

1. **Header (Início do pacote)**  
   - **Tamanho:** 1 byte  
   - **Valor:** `0xAA` (Valor fixo para identificar o início do pacote)

2. **ID**  
   - **Tamanho:** 2 bytes  
   - **Valor:** Um número de 0 a 65535 (usado para identificar o tipo de pacote ou a sequência de pacotes)

3. **Tipo de Dado (Data Type)**  
   - **Tamanho:** 1 byte  
   - **Valor:** Um código que identifica o tipo de dado contido no payload. Exemplos:
     - `0x01`: **Bool**
     - `0x02`: **Int16**
     - `0x03`: **Int32**
     - `0x04`: **Float**
     - `0x05`: **String**

4. **Tamanho dos dados (Data Length)**  
   - **Tamanho:** 1 byte  
   - **Valor:** Quantidade de bytes do campo **Dados (Payload)**

5. **Dados (Payload)**  
   - **Tamanho:** Variável (definido pelo campo **Tamanho dos dados**)  
   - **Valor:** Conteúdo principal do pacote. Pode ser um bool, int, float ou string, conforme indicado pelo campo **Tipo de Dado**.

6. **Checksum**  
   - **Tamanho:** 1 byte  
   - **Valor:** Soma dos bytes do pacote (excluindo Header e Footer) para validar a integridade.

7. **Footer (Fim do pacote)**  
   - **Tamanho:** 1 byte  
   - **Valor:** `0xFF` (Valor fixo para identificar o final do pacote)

---

### Exemplo de Pacote

#### Exemplo 1: Pacote Enviando um `Int16` com Valor `1234`
- **Header:** `0xAA`  
- **ID:** `0x00, 0x01`  
- **Tipo de Dado:** `0x02` (Int16)  
- **Tamanho dos Dados:** `0x02`  
- **Dados:** `0x04, 0xD2` (Valor `1234` em bytes)  
- **Checksum:** `0xDF` (Soma de `ID + Tamanho + Tipo + Dados`)  
- **Footer:** `0xFF`

**Bytes Finais:** `AA 00 01 02 02 04 D2 DF FF`

#### Exemplo 2: Pacote Enviando uma String `"Hello"`
- **Header:** `0xAA`  
- **ID:** `0x00, 0x02`  
- **Tipo de Dado:** `0x05` (String)  
- **Tamanho dos Dados:** `0x05`  
- **Dados:** `48 65 6C 6C 6F` (ASCII de `"Hello"`)  
- **Checksum:** `0x2B`  
- **Footer:** `0xFF`

**Bytes Finais:** `AA 00 02 05 05 48 65 6C 6C 6F 2B FF`

---

### Planilha do Protocolo

| Parte             | Descrição                              | Tamanho (em bytes) | Valor Exemplo              |
|-------------------|----------------------------------------|--------------------|----------------------------|
| **Header**        | Início do pacote                       | 1 byte             | `0xAA`                     |
| **ID**            | Identificador do pacote                | 2 bytes            | `0x01`                     |
| **Tipo de Dado**  | Identifica o tipo de dado no payload   | 1 byte             | `0x05` (String)            |
| **Tamanho Dados** | Tamanho dos dados (payload)            | 1 byte             | `0x12` (18 bytes)          |
| **Dados**         | Conteúdo real do pacote                | Variável           | `"Hello from Arduino"`     |
| **Checksum**      | Verificação de integridade             | 1 byte             | `0x77`                     |
| **Footer**        | Final do pacote                        | 1 byte             | `0xFF`                     |

---