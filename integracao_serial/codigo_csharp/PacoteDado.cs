using System.Text;

class PacoteDado
{
    public byte Header { get; set; }
    public ushort Id { get; set; }
    public byte TamanhoDados { get; set; }
    public byte[] Dados { get; set; }
    public byte Checksum { get; set; }
    public byte Footer { get; set; }

    public PacoteDado(ushort id, object dados)
    {
        Header = 0xAA;
        Id = id;
        Dados = dados switch
        {
            string s => Encoding.ASCII.GetBytes(s),
            int i => BitConverter.GetBytes(i),
            bool b => [(byte)(b ? 1 : 0)],
            float f => BitConverter.GetBytes(f),
            _ => throw new ArgumentException("Tipo de dado não suportado.")
        };
        TamanhoDados = (byte)Dados.Length;
        Checksum = CalcularChecksum();
        Footer = 0xFF;
    }

    public PacoteDado(byte[] pacoteRecebido)
    {
        if (pacoteRecebido.Length >= 6 + pacoteRecebido[3])                         // Verifica se o pacote tem o tamanho suficiente
        {
            Header = pacoteRecebido[0];
            byte[] idBytes = new byte[2] { pacoteRecebido[2], pacoteRecebido[1] };  // ID está em Little Endian
            Id = BitConverter.ToUInt16(idBytes, 0);                                 // Converte os dois bytes na ordem correta
            TamanhoDados = pacoteRecebido[3];
            Dados = new byte[TamanhoDados];
            Array.Copy(pacoteRecebido, 4, Dados, 0, TamanhoDados);
            Checksum = pacoteRecebido[pacoteRecebido.Length - 2];
            Footer = pacoteRecebido[pacoteRecebido.Length - 1];
        }
        else
        {
            throw new ArgumentException("Pacote inválido: tamanho incorreto.");
        }
    }

    public byte CalcularChecksum()
    {
        byte sum = Header;
        sum += (byte)(Id & 0xFF);                       // Soma o primeiro byte de Id
        sum += (byte)((Id >> 8) & 0xFF);                // Soma o segundo byte de Id
        sum += TamanhoDados;
        foreach (var b in Dados) sum += b;
        return (byte)(sum % 256);                       // Garante que o checksum será 1 byte
    }

    public byte[] ToByteArray()
    {
        byte[] pacote = new byte[6 + Dados.Length];     // 6 bytes fixos (Header, ID, Tamanho, Checksum, Footer) + Dados
        pacote[0] = Header;                             // Header (1 byte)
        pacote[1] = (byte)(Id & 0xFF);                  // Menos significativo do ID (LSB)
        pacote[2] = (byte)((Id >> 8) & 0xFF);           // Mais significativo do ID (MSB)
        pacote[3] = TamanhoDados;                       // Tamanho dos dados
        Array.Copy(Dados, 0, pacote, 4, Dados.Length);  // Copia os dados para o pacote, começando no índice 4
        pacote[4 + Dados.Length] = Checksum;            // Checksum (1 byte)
        pacote[5 + Dados.Length] = Footer;              // Footer (1 byte)
        return pacote;
    }

    public void ImprimirDetalhes()
    {
        Console.WriteLine($"Header: 0x{Header:X2}");
        Console.WriteLine($"ID: 0x{Id:X2} (Bytes: 0x{(Id & 0xFF):X2} 0x{((Id >> 8) & 0xFF):X2})");
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