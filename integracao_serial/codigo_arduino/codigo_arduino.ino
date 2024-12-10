#define HEADER 0xAA
#define FOOTER 0xFF
#define CMD_HELLO 0x01
#define CMD_LED 0x02
#define CMD_INT 0x03
#define CMD_BOOL 0x04
#define CMD_FLOAT 0x05

bool ledState = false;

void enviarPacote(uint8_t id, void *data, size_t dataLength);
uint8_t calcularChecksum(uint8_t *data, size_t length);

void setup()
{
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(115200);
  while (!Serial)
    ;
}

void loop()
{
  processarComando();
}

void processarComando()
{
  if (Serial.available() > 0)
  {
    uint8_t buffer[128];
    size_t bytesLidos = Serial.readBytes(buffer, sizeof(buffer));

    if (bytesLidos >= 5 && buffer[0] == HEADER && buffer[bytesLidos - 1] == FOOTER)
    {
      uint8_t checksum = calcularChecksum(buffer, bytesLidos - 2);
      if (checksum == buffer[bytesLidos - 2])
      {
        uint8_t comando = buffer[1];

        if (comando == CMD_HELLO)
        {
          enviarPacote(CMD_HELLO, "Hello from Arduino", strlen("Hello from Arduino"));
        }
        else if (comando == CMD_LED)
        {
          ledState = !ledState;
          digitalWrite(LED_BUILTIN, ledState ? HIGH : LOW);
          enviarPacote(CMD_LED, "Arduino LED changed", strlen("Arduino LED changed"));
        }
        else if (comando == CMD_INT)
        {
          if (bytesLidos >= 9) // Verifica se o pacote contém dados suficientes para um int (5 bytes de cabeçalho + 4 bytes para o int)
          {
            int valor = 0;
            memcpy(&valor, &buffer[3], sizeof(valor));    // Lê o valor inteiro
            enviarPacote(CMD_INT, &valor, sizeof(valor)); // Envia o valor de volta
          }
        }
        else if (comando == CMD_BOOL)
        {
          if (bytesLidos >= 4)
          {
            bool estado = false;
            memcpy(&estado, &buffer[3], sizeof(estado));
            enviarPacote(CMD_BOOL, &estado, sizeof(estado));
          }
        }
        else if (comando == CMD_FLOAT)
        {
          if (bytesLidos >= 7)
          {
            float valorDecimal = 0.0f;
            memcpy(&valorDecimal, &buffer[3], sizeof(valorDecimal));
            enviarPacote(CMD_FLOAT, &valorDecimal, sizeof(valorDecimal));
          }
        }
      }
    }
  }
}

void enviarPacote(uint8_t id, void *data, size_t dataLength)
{
  uint8_t pacote[128];

  pacote[0] = HEADER;
  pacote[1] = id;
  pacote[2] = dataLength;

  memcpy(&pacote[3], data, dataLength); // Copiar os dados para o pacote

  uint8_t checksum = calcularChecksum(pacote, 3 + dataLength);

  pacote[3 + dataLength] = checksum;
  pacote[4 + dataLength] = FOOTER;

  Serial.write(pacote, 5 + dataLength);
}

uint8_t calcularChecksum(uint8_t *data, size_t length)
{
  uint8_t sum = 0;
  for (size_t i = 0; i < length; i++)
  {
    sum += data[i];
  }
  return sum;
}