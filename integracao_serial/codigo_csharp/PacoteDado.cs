using System.Text;

class PacoteDado
{
    public byte Header { get; set; }
    public byte Id { get; set; }
    public byte TamanhoDados { get; set; }
    public byte[] Dados { get; set; }
    public byte Checksum { get; set; }
    public byte Footer { get; set; }

    public PacoteDado(byte id, string dados)
    {
        Header = 0xAA;
        Id = id;
        Dados = string.IsNullOrEmpty(dados) ? [] : Encoding.ASCII.GetBytes(dados); // Preenche os dados como texto ASCII
        TamanhoDados = (byte)Dados.Length;
        Checksum = CalcularChecksum();
        Footer = 0xFF;
    }

    public PacoteDado(byte[] pacoteRecebido)
    {
        if (pacoteRecebido.Length >= 5 && pacoteRecebido[0] == 0xAA && pacoteRecebido[^1] == 0xFF)
        {
            Header = pacoteRecebido[0];
            Id = pacoteRecebido[1];
            TamanhoDados = pacoteRecebido[2];
            Dados = new byte[TamanhoDados];
            Array.Copy(pacoteRecebido, 3, Dados, 0, TamanhoDados);
            Checksum = pacoteRecebido[^2];
            Footer = pacoteRecebido[^1];
        }
        else
        {
            throw new ArgumentException("Pacote inválido.");
        }
    }

    private byte CalcularChecksum()
    {
        byte sum = Header;
        sum += Id;
        sum += TamanhoDados;
        foreach (var b in Dados) sum += b;
        return sum;
    }

    public byte[] ToByteArray()
    {
        byte[] pacote = new byte[5 + Dados.Length];
        pacote[0] = Header;
        pacote[1] = Id;
        pacote[2] = TamanhoDados;
        Array.Copy(Dados, 0, pacote, 3, Dados.Length);
        pacote[3 + Dados.Length] = Checksum;
        pacote[4 + Dados.Length] = Footer;
        return pacote;
    }

    public void ImprimirDetalhes()
    {
        Console.WriteLine($"Header: 0x{Header:X2}");
        Console.WriteLine($"ID: 0x{Id:X2}");
        Console.WriteLine($"Tamanho dos Dados: {TamanhoDados}");
        Console.WriteLine($"Dados (Bytes): {(Dados.Length > 0 ? BitConverter.ToString(Dados) : "Nenhum dado")}");
        Console.WriteLine($"Dados (Texto): {(Dados.Length > 0 ? Encoding.ASCII.GetString(Dados) : "Nenhum dado")}");
        Console.WriteLine($"Checksum: 0x{Checksum:X2}");
        Console.WriteLine($"Footer: 0x{Footer:X2}");
        Console.WriteLine();
    }
}