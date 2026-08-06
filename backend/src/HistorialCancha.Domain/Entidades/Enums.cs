namespace HistorialCancha.Domain.Entidades;

public enum Condicion : byte
{
    Local = 0,
    Visitante = 1
}

public enum Modalidad : byte
{
    EnCancha = 0,
    TV = 1,
    Streaming = 2,
    Radio = 3,
    NoLoVi = 4
}

public enum Resultado : byte
{
    Victoria = 0,
    Empate = 1,
    Derrota = 2
}
