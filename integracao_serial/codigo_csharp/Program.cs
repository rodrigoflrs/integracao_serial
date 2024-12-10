using System.IO.Ports;
using System.Text;

class Program
{
    private const byte CMD_HELLO = 0x01;
    private const byte CMD_LED = 0x02;

    private static readonly int _timeout = 10000;
    private static SerialPort _serialPort;

    static void Main()
    {
        _serialPort = new SerialPort("COM11", 115200);
        try
        {
            _serialPort.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao abrir a porta: {ex.Message}");
            return;
        }

        while (true)
        {
            Console.WriteLine("\nEscolha uma opção:");
            Console.WriteLine("1 - Pedir mensagem 'Hello'");
            Console.WriteLine("2 - Alterar estado do LED");
            Console.WriteLine("3 - Sair");

            string? opcao = Console.ReadLine();
            if (opcao == "3") break;

            switch (opcao)
            {
                case "1":
                    EnviarComando(CMD_HELLO);
                    break;
                case "2":
                    EnviarComando(CMD_LED);
                    break;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }

        _serialPort.Close();
    }

    static void EnviarComando(byte id)
    {
        // Define os dados baseados no comando enviado
        string dados = id switch
        {
            0x01 => "HELLO",
            0x02 => "LED",
            _ => "Comando genérico"
        };

        // Cria o pacote com ID e dados
        var pacote = new PacoteDado(id, dados);

        Console.WriteLine("Pacote a ser enviado:");
        pacote.ImprimirDetalhes();

        // Converte para bytes e envia
        byte[] bytesPacote = pacote.ToByteArray();
        _serialPort.Write(bytesPacote, 0, bytesPacote.Length);

        Console.WriteLine("Aguardando resposta...");
        string resposta = ReceberResposta();
        Console.WriteLine($"Resposta do Arduino: {resposta}");
    }



    static string ReceberResposta()
    {
        DateTime startTime = DateTime.Now;

        while ((DateTime.Now - startTime).TotalMilliseconds < _timeout)
        {
            if (_serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[128];
                int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

                if (bytesRead >= 5)
                {
                    try
                    {
                        var pacote = new PacoteDado(buffer[..bytesRead]);
                        Console.WriteLine("Pacote recebido:");
                        pacote.ImprimirDetalhes();

                        return Encoding.ASCII.GetString(pacote.Dados);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Erro ao processar pacote: {ex.Message}");
                    }
                }
            }
        }

        return "Timeout sem resposta.";
    }
}
