using System.IO.Ports;
using System.Text;

class Program
{
    private static readonly int _timeout = 10000;
    private static SerialPort _serialPort;

    static void Main()
    {
        InicializarPorta();

        while (true)
        {
            string opcao = ExibirMenu();
            if (opcao == "2") break;

            try
            {
                ProcessarOpcaoMenu(opcao);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        _serialPort.Close();
    }

    private static void InicializarPorta()
    {
        _serialPort = new SerialPort("COM13", 115200); // Alterar nome da porta se necessário
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
        Console.WriteLine("1 - Criar pacote de dados");
        Console.WriteLine("2 - Sair");

        return Console.ReadLine() ?? "";
    }

    private static void ProcessarOpcaoMenu(string opcao)
    {
        switch (opcao)
        {
            case "1":
                CriarEEnviarPacote();
                break;
            default:
                Console.WriteLine("Opção inválida. Tente novamente.");
                break;
        }
    }

    private static void CriarEEnviarPacote()
    {
        ushort id = SolicitarId();
        var (dado, tipoDado) = SolicitarDado();             // Agora solicitamos o tipo de dado junto com o valor

        var pacote = new PacoteDado(id, tipoDado, dado);    // Passamos o tipo de dado para o construtor

        Console.WriteLine("\nPacote de dados a ser enviado:");
        pacote.ImprimirDetalhes();

        EnviarPacote(pacote);
        string resposta = ReceberResposta();
        Console.WriteLine($"Resposta do Arduino: {resposta}");
    }

    private static ushort SolicitarId()
    {
        while (true)
        {
            Console.WriteLine("Digite o Id:");
            string input = Console.ReadLine();

            if (ushort.TryParse(input, out ushort id))
            {
                return id;
            }

            Console.WriteLine("Entrada inválida. Certifique-se de digitar um número entre 0 e 65535.");
        }
    }

    private static (object dado, TipoDado tipoDado) SolicitarDado()
    {
        Console.WriteLine("Digite o valor a ser enviado:");
        string input = Console.ReadLine() ?? "";

        if (input.ToLower() is "true" or "false")
        {
            bool boolValue = bool.Parse(input.ToLower());
            return (boolValue, TipoDado.Bool);
        }
        else if (int.TryParse(input, out int intValue))
        {
            return (intValue, TipoDado.Int32);
        }
        else if (float.TryParse(input, out float floatValue))
        {
            return (floatValue, TipoDado.Float);
        }
        else
        {
            return (input, TipoDado.String); // Se for uma string qualquer
        }
    }

    private static void EnviarPacote(PacoteDado pacote)
    {
        byte[] bytesPacote = pacote.ToByteArray();
        Console.WriteLine($"Bytes enviados: {bytesPacote.Length}");
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
                    Console.WriteLine($"Bytes recebidos: {bytesRead}");
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

            Console.WriteLine("\nPacote recebido:");
            pacote.ImprimirDetalhes();

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
        return pacote.TipoDado switch
        {
            TipoDado.Int32 => $"Int32 | {BitConverter.ToInt32(pacote.Dados, 0)}",
            TipoDado.Float => $"Float | {BitConverter.ToSingle(pacote.Dados, 0)}",
            TipoDado.Bool => $"Bool | {pacote.Dados[0] != 0}",
            TipoDado.String => $"String | {Encoding.ASCII.GetString(pacote.Dados)}",
            _ => "Tipo de dado não suportado."
        };
    }
}
