using System.IO.Ports;
using System.Text;

class Program
{
    private const byte CMD_HELLO = 0x01;
    private const byte CMD_LED = 0x02;
    private const byte CMD_INT = 0x03;
    private const byte CMD_BOOL = 0x04;
    private const byte CMD_FLOAT = 0x05;

    private static readonly int _timeout = 10000;
    private static bool alternarEstado = false;
    private static SerialPort _serialPort;

    static void Main()
    {
        InicializarPorta();

        while (true)
        {
            string opcao = ExibirMenu();
            if (opcao == "6") break;

            ExecutarComando(opcao);
        }

        _serialPort.Close();
    }

    private static void InicializarPorta()
    {
        _serialPort = new SerialPort("COM11", 115200); // Alterar nome da porta se necessário
        try
        {
            _serialPort.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao abrir a porta: {ex.Message}");
        }
    }

    private static string ExibirMenu()
    {
        Console.WriteLine("\nEscolha uma opção:");
        Console.WriteLine("1 - Alterar estado do LED");
        Console.WriteLine("2 - Enviar/Receber string");
        Console.WriteLine("3 - Enviar/Receber inteiro");
        Console.WriteLine("4 - Enviar/Receber booleano");
        Console.WriteLine("5 - Enviar/Receber valor decimal (float)");
        Console.WriteLine("6 - Sair");

        return Console.ReadLine() ?? "";
    }

    private static void ExecutarComando(string opcao)
    {
        byte comando = opcao switch
        {
            "1" => CMD_LED,
            "2" => CMD_HELLO,
            "3" => CMD_INT,
            "4" => CMD_BOOL,
            "5" => CMD_FLOAT,
            _ => throw new ArgumentException("Opção inválida.")
        };

        EnviarComando(comando);
    }

    private static void EnviarComando(byte id)
    {
        object dados = GerarDados(id);
        var pacote = new PacoteDado(id, dados);

        Console.WriteLine("Pacote a ser enviado:");
        pacote.ImprimirDetalhes();

        EnviarPacote(pacote);
        string resposta = ReceberResposta();
        Console.WriteLine($"Resposta do Arduino: {resposta}");
    }

    private static object GerarDados(byte id)
    {
        if (id == CMD_BOOL)
        {
            alternarEstado = !alternarEstado;
        }

        return id switch
        {
            CMD_LED => "LED",
            CMD_HELLO => "HELLO",
            CMD_INT => 12345,
            CMD_BOOL => alternarEstado,
            CMD_FLOAT => 3.14159f,
            _ => "Comando genérico"
        };
    }

    private static void EnviarPacote(PacoteDado pacote)
    {
        byte[] bytesPacote = pacote.ToByteArray();
        _serialPort.Write(bytesPacote, 0, bytesPacote.Length);
    }

    private static string ReceberResposta()
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
                    return ProcessarPacote(buffer, bytesRead);
                }
                else
                {
                    return "Pacote recebido tem tamanho inválido.";
                }
            }
        }

        return "Timeout sem resposta.";
    }

    private static string ProcessarPacote(byte[] buffer, int bytesRead)
    {
        try
        {
            var pacote = new PacoteDado(buffer[..bytesRead]);

            Console.WriteLine("Pacote recebido:");
            pacote.ImprimirDetalhes();

            if (pacote.Header != 0xAA || pacote.Footer != 0xFF)
            {
                return "Pacote inválido. Header ou Footer incorretos.";
            }

            return InterpretarResposta(pacote);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Erro ao processar pacote: {ex.Message}");
            return "Erro ao processar pacote.";
        }
    }

    private static string InterpretarResposta(PacoteDado pacote)
    {
        return pacote.Id switch
        {
            CMD_INT => InterpretarInteiro(pacote),
            CMD_BOOL => InterpretarBooleano(pacote),
            CMD_FLOAT => InterpretarFloat(pacote),
            _ => Encoding.ASCII.GetString(pacote.Dados)
        };
    }

    private static string InterpretarInteiro(PacoteDado pacote)
    {
        return pacote.Dados.Length switch
        {
            2 => $"Int recebido: {BitConverter.ToInt16(pacote.Dados, 0)}", // Se for 2 bytes, converte como Int16
            4 => $"Int recebido: {BitConverter.ToInt32(pacote.Dados, 0)}", // Se for 4 bytes, converte como Int32
            _ => "Tamanho inválido para INT."
        };
    }

    private static string InterpretarBooleano(PacoteDado pacote)
    {
        return pacote.Dados.Length == 1
            ? $"Bool recebido: {BitConverter.ToBoolean(pacote.Dados, 0)}"
            : "Tamanho inválido para BOOL.";
    }

    private static string InterpretarFloat(PacoteDado pacote)
    {
        return pacote.Dados.Length == 4
            ? $"Float recebido: {BitConverter.ToSingle(pacote.Dados, 0)}"
            : "Tamanho inválido para FLOAT.";
    }
}
