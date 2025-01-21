using System.Text;

class PacoteDado
{
    public byte Header { get; set; }
    public ushort Id { get; set; }
    public TipoDado TipoDado { get; set; }
    public byte TamanhoDados { get; set; }
    public byte[] Dados { get; set; }
    public byte Checksum { get; set; }
    public byte Footer { get; set; }


    public PacoteDado(ushort id, TipoDado tipoDado, object dados)
    {
        Header = 0xAA;
        Id = (ushort)((id >> 8) | ((id & 0xFF) << 8));
        TipoDado = tipoDado;

        // Atribuindo os Dados de acordo com o tipo fornecido
        Dados = tipoDado switch
        {
            TipoDado.String => Encoding.ASCII.GetBytes((string)dados),
            TipoDado.Int32 => [.. BitConverter.GetBytes((int)dados).Reverse()], // Inverte a ordem dos bytes para Big-Endian
            TipoDado.Bool => [(byte)((bool)dados ? 1 : 0)],
            TipoDado.Float => [.. BitConverter.GetBytes((float)dados).Reverse()], // Inverte a ordem dos bytes para Big-Endian
            _ => throw new ArgumentException("Tipo de dado não suportado.")
        };

        TamanhoDados = (byte)Dados.Length;
        Checksum = CalcularChecksum();
        Footer = 0xFF; // Valor fixo para o footer
    }

    public PacoteDado(byte[] pacoteRecebido)
    {
        if (pacoteRecebido == null || pacoteRecebido.Length < 7)
        {
            throw new ArgumentException("Pacote inválido: pacote nulo ou muito curto.");
        }

        // Verifica o comprimento mínimo esperado do pacote
        int tamanhoEsperado = 7 + pacoteRecebido[4];
        if (pacoteRecebido.Length < tamanhoEsperado)
        {
            throw new ArgumentException(
                $"Pacote inválido: tamanho incorreto. Esperado: {tamanhoEsperado}, Recebido: {pacoteRecebido.Length}");
        }

        // Atribui os campos do pacote
        Header = pacoteRecebido[0];
        if (Header != 0xAA)
        {
            throw new ArgumentException($"Pacote inválido: Header incorreto (Recebido: {Header:X2}).");
        }

        Id = (ushort)((pacoteRecebido[1] << 8) | pacoteRecebido[2]);
        TipoDado = (TipoDado)pacoteRecebido[3];
        TamanhoDados = pacoteRecebido[4];

        Dados = new byte[TamanhoDados];
        Array.Copy(pacoteRecebido, 5, Dados, 0, TamanhoDados);

        Checksum = pacoteRecebido[pacoteRecebido.Length - 2];
        Footer = pacoteRecebido[pacoteRecebido.Length - 1];
        if (Footer != 0xFF)
        {
            throw new ArgumentException($"Pacote inválido: Footer incorreto (Recebido: {Footer:X2}).");
        }

        // Valida o checksum
        byte checksumCalculado = CalcularChecksum();
        if (Checksum != checksumCalculado)
        {
            throw new ArgumentException(
                $"Pacote inválido: Checksum incorreto (Esperado: {checksumCalculado:X2}, Recebido: {Checksum:X2}).");
        }
    }

    public byte CalcularChecksum()
    {
        byte sum = Header;

        // Soma os dois bytes do ID (mais significativo e depois o menos significativo para big-endian)
        sum += (byte)((Id >> 8) & 0xFF);    // Byte mais significativo
        sum += (byte)(Id & 0xFF);           // Byte menos significativo

        sum += (byte)TipoDado;              // Tipo de dado (enum convertido para byte)
        sum += TamanhoDados;                // Tamanho dos dados

        // Soma todos os bytes dos dados
        foreach (var b in Dados)
        {
            sum += b;
        }

        // Aplica módulo 256 para garantir que o checksum seja um byte (0-255)
        return (byte)(sum % 256);
    }

    public byte[] ToByteArray()
    {
        byte[] pacote = new byte[7 + Dados.Length];
        pacote[0] = Header;
        pacote[1] = (byte)((Id >> 8) & 0xFF);
        pacote[2] = (byte)(Id & 0xFF);
        pacote[3] = (byte)TipoDado;
        pacote[4] = TamanhoDados;
        Array.Copy(Dados, 0, pacote, 5, Dados.Length);
        pacote[5 + Dados.Length] = Checksum;
        pacote[6 + Dados.Length] = Footer;
        return pacote;
    }

    public void ImprimirDetalhes()
    {
        Console.WriteLine($"Header: 0x{Header:X2}");
        Console.WriteLine($"ID: 0x{Id:X4} (Bytes: 0x{Id & 0xFF:X2} 0x{Id >> 8:X2})"); // Exibição Big-Endian
        Console.WriteLine($"Tipo de Dado: {TipoDado}");
        Console.WriteLine($"Tamanho dos Dados: {TamanhoDados}");
        Console.WriteLine($"Dados (Bytes): {(Dados.Length > 0 ? BitConverter.ToString(Dados) : "Nenhum dado")}");
        Console.WriteLine($"Dados (Texto): {(Dados.Length > 0 ? Encoding.ASCII.GetString(Dados) : "Nenhum dado")}");
        Console.WriteLine($"Checksum: 0x{Checksum:X2}");
        Console.WriteLine($"Footer: 0x{Footer:X2}");
        byte[] pacoteCompleto = ToByteArray();
        Console.WriteLine("Pacote Completo (Bytes): " + BitConverter.ToString(pacoteCompleto));
        Console.WriteLine();
    }
}