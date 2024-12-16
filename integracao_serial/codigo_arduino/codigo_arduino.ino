#define HEADER 0xAA
#define FOOTER 0xFF
#define LED_PIN LED_BUILTIN

void setup() {
  Serial.begin(115200);
  pinMode(LED_PIN, OUTPUT);
  while (!Serial);
}

void loop() {
  processarPacote();
}

void processarPacote() {
  if (Serial.available() > 0) {
    uint8_t buffer[128];
    size_t bytesLidos = Serial.readBytes(buffer, sizeof(buffer));

    // Verifica se o pacote possui o Header e Footer corretos
    if (bytesLidos >= 6 && buffer[0] == HEADER && buffer[bytesLidos - 1] == FOOTER) {
      piscarLED(3, 200);

      // Envia de volta o pacote inteiro, incluindo Header e Footer
      Serial.write(buffer, bytesLidos);
    }
  }
}

void piscarLED(int vezes, int intervalo) {
  for (int i = 0; i < vezes; i++) {
    digitalWrite(LED_PIN, HIGH);
    delay(intervalo);
    digitalWrite(LED_PIN, LOW);
    delay(intervalo);
  }
}
