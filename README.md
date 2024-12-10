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

O pacote que estamos enviando e recebendo entre o Arduino e o C# segue uma estrutura bem definida. Essa estrutura pode ser dividida em várias partes, como `Header`, `ID`, `Tamanho dos dados`, `Dados`, `Checksum` e `Footer`.

### Detalhes da Estrutura:

1. **Header (Início do pacote)**  
   - **Tamanho:** 1 byte  
   - **Valor:** `0xAA` (Este valor é fixo e serve para identificar o início do pacote)

2. **ID**  
   - **Tamanho:** 1 byte  
   - **Valor:** Um número de 0 a 255 (geralmente, é usado para identificar o tipo de pacote ou a sequência de pacotes)

3. **Tamanho dos dados (Data Length)**  
   - **Tamanho:** 1 byte  
   - **Valor:** Um número que representa o comprimento dos dados que serão enviados no pacote (a quantidade de bytes para o conteúdo principal do pacote)

4. **Dados (Payload)**  
   - **Tamanho:** Variável (depende do valor de "Tamanho dos dados")  
   - **Valor:** A informação real que você deseja transmitir (no nosso caso, é a string "mensagem arduino", convertida para bytes). Esse campo pode conter qualquer tipo de dado necessário para a comunicação.

5. **Checksum**  
   - **Tamanho:** 1 byte  
   - **Valor:** Uma soma dos bytes de dados (excluindo header, ID e footer) calculada para validar a integridade do pacote.

6. **Footer (Fim do pacote)**  
   - **Tamanho:** 1 byte  
   - **Valor:** `0xFF` (Este valor é fixo e serve para identificar o final do pacote)

---

### Exemplo de Pacote

#### Pacote Exemplo (em hex)

| Byte | Valor                                                                                     | Descrição                           |
|------|-------------------------------------------------------------------------------------------|-------------------------------------|
| 1    | 0xAA                                                                                      | Header (Início do pacote)           |
| 2    | 0x01                                                                                      | ID do pacote (por exemplo, 1)       |
| 3    | 0x12                                                                                      | Tamanho dos dados (18 bytes)        |
| 4-21 | 0x48 0x65 0x6C 0x6C 0x6F 0x20 0x66 0x72 0x6F 0x6D 0x20 0x41 0x72 0x64 0x75 0x69 0x6E 0x6F | Dados (string "Hello from Arduino") |
| 22   | 0x77                                                                                      | Checksum                            |
| 23   | 0xFF                                                                                      | Footer (Final do pacote)            |

### Explicando o Exemplo

1. **Header (`0xAA`)**: Este byte indica o início do pacote. Sempre terá o valor fixo `0xAA` para garantir que o C# saiba que o pacote começou corretamente.
   
2. **ID (`0x01`)**: O identificador do pacote. Pode ser usado para distinguir diferentes pacotes ou para sequenciar as mensagens.

3. **Tamanho dos dados (`0x12`)**: Este byte indica que os dados do pacote têm 18 bytes de comprimento. Essa informação ajuda a identificar até onde o conteúdo do pacote se estende.

4. **Dados (Payload)**: A parte principal do pacote contém a string "Hello from Arduino", que é convertida para bytes. O valor "Hello from Arduino" em hexadecimal é:
   ```
   48 65 6C 6C 6F 20 66 72 6F 6D 20 41 72 64 75 69 6E 6F
   ```

5. **Checksum (`0x77`)**: O checksum é calculado somando os valores dos bytes de dados (os 18 bytes de dados neste caso) e gerando um valor único para garantir a integridade. Caso o pacote seja corrompido, o C# pode identificar um erro de checksum.

6. **Footer (`0xFF`)**: O byte `0xFF` no final do pacote indica que o pacote foi corretamente fechado. Ele ajuda a garantir que o C# saiba que o pacote terminou corretamente.

---

### Planilha do Protocolo

| Parte             | Descrição                              | Tamanho (em bytes) | Valor Exemplo              |
|-------------------|----------------------------------------|--------------------|----------------------------|
| **Header**        | Início do pacote                       | 1 byte             | `0xAA`                     |
| **ID**            | Identificador do pacote                | 1 byte             | `0x01`                     |
| **Tamanho Dados** | Tamanho dos dados (payload)            | 1 byte             | `0x12` (18 bytes)          |
| **Dados**         | Conteúdo real do pacote (string, etc.) | 18 bytes           | `"Hello from Arduino"`     |
| **Checksum**      | Verificação de integridade             | 1 byte             | `0x77`                     |
| **Footer**        | Final do pacote                        | 1 byte             | `0xFF`                     |
