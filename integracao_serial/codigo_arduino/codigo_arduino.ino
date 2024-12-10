#define HEADER 0xAA
#define FOOTER 0xFF
#define CMD_HELLO 0x01
#define CMD_LED 0x02

bool ledState = false;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(115200);
  while (!Serial);
}

void loop() {
  processarComando();
}

// Função para processar comandos recebidos
void processarComando() {
  if (Serial.available() > 0) {
    uint8_t buffer[128];
    size_t bytesLidos = Serial.readBytes(buffer, sizeof(buffer));

    if (bytesLidos >= 5 && buffer[0] == HEADER && buffer[bytesLidos - 1] == FOOTER) {
      uint8_t checksum = calcularChecksum(buffer, bytesLidos - 2);
      if (checksum == buffer[bytesLidos - 2]) {
        uint8_t comando = buffer[1];
        if (comando == CMD_HELLO) {
          enviarPacote(CMD_HELLO, "Hello from Arduino");
        } else if (comando == CMD_LED) {
          ledState = !ledState;
          digitalWrite(LED_BUILTIN, ledState ? HIGH : LOW);
          enviarPacote(CMD_LED, "Arduino LED changed");
        }
      }
    }
  }
}

// Função para enviar pacotes
void enviarPacote(uint8_t id, String data) {
  uint8_t pacote[128];
  int dataLength = data.length();

  pacote[0] = HEADER;
  pacote[1] = id;
  pacote[2] = dataLength;
  data.getBytes(&pacote[3], dataLength + 1);

  uint8_t checksum = calcularChecksum(pacote, 3 + dataLength);
  pacote[3 + dataLength] = checksum;
  pacote[4 + dataLength] = FOOTER;

  Serial.write(pacote, 5 + dataLength);
}

// Função para calcular checksum
uint8_t calcularChecksum(uint8_t *data, size_t length) {
  uint8_t sum = 0;
  for (size_t i = 0; i < length; i++) {
    sum += data[i];
  }
  return sum;
}
