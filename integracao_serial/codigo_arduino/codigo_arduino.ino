#define HEADER 0xAA
#define FOOTER 0xFF
const uint16_t ID1 = 0x01;
const uint16_t ID2 = 0x02;
const uint16_t ID3 = 0x03;
const uint16_t ID4 = 0x04;
const uint16_t ID5 = 0x05;

bool ledState = false;

void enviarPacote(uint16_t id, void *data, size_t dataLength);
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
  processarPacote();
}

void processarPacote()
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
        uint16_t id = (uint16_t)(buffer[1]) | ((uint16_t)(buffer[2]) << 8);

        if (id == ID2)
        {
          if (bytesLidos >= 5) // ID2 - String "HELLO"
          {
            enviarPacote(ID2, "HELLO", strlen("HELLO"));
          }
        }
        else if (id == ID1)
        {
          if (bytesLidos >= 5) // ID1 - LED control
          {
            ledState = !ledState;
            digitalWrite(LED_BUILTIN, ledState ? HIGH : LOW);
            enviarPacote(ID1, "Arduino LED changed", strlen("Arduino LED changed"));
          }
        }
        else if (id == ID3)
        {
          if (bytesLidos >= 9) // ID3 - Inteiro
          {
            int16_t valor = (buffer[3]) | (buffer[4] << 8);
            memcpy(&valor, &buffer[3], sizeof(valor));
            enviarPacote(ID3, &valor, sizeof(valor));
          }
        }
        else if (id == ID4)
        {
          if (bytesLidos >= 7) // ID4 - Booleano (4 bytes)
          {
            bool estado = false;
            memcpy(&estado, &buffer[3], sizeof(estado));
            enviarPacote(ID4, &estado, sizeof(estado));
          }
        }
        else if (id == ID5)
        {
          if (bytesLidos >= 7)
          {
            float valorDecimal = 0.0f;
            memcpy(&valorDecimal, &buffer[3], sizeof(valorDecimal));
            enviarPacote(ID5, &valorDecimal, sizeof(valorDecimal));
          }
        }
      }
    }
  }
}

void enviarPacote(uint16_t id, void *data, size_t dataLength)
{
  uint8_t pacote[128];

  pacote[0] = HEADER;
  pacote[1] = (uint8_t)(id & 0xFF);        // Primeiro byte do ID
  pacote[2] = (uint8_t)((id >> 8) & 0xFF); // Segundo byte do ID
  pacote[3] = (uint8_t)(dataLength);       // Tamanho dos dados (agora corretamente calculado)

  memcpy(&pacote[4], data, dataLength); // Copiar os dados para o pacote

  uint8_t checksum = calcularChecksum(pacote, 4 + dataLength); // Calcular o checksum corretamente

  pacote[4 + dataLength] = checksum;  // Adiciona o checksum ao final
  pacote[5 + dataLength] = FOOTER;   // Adiciona o footer

  Serial.write(pacote, 6 + dataLength); // Envia o pacote completo
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