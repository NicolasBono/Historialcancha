namespace HistorialCancha.Domain;

/// <summary>
/// Parámetros de negocio que vienen de configuración, nunca hardcodeados.
/// POCO puro: la API lo puebla, el dominio sólo lo lee.
/// </summary>
public class OpcionesDominio
{
    /// <summary>Equipo propio. Un rival nunca puede ser este valor.</summary>
    public string MiEquipo { get; set; } = string.Empty;

    /// <summary>Partidos mínimos contra un rival para entrar al ranking.</summary>
    public int MinPartidosRanking { get; set; } = 3;

    /// <summary>Mes de corte de temporada (7 = julio).</summary>
    public int MesInicioTemporada { get; set; } = 7;

    /// <summary>Partidos mínimos por grupo para emitir veredicto cábala/yeta.</summary>
    public int MinPartidosVeredicto { get; set; } = 5;

    /// <summary>Diferencia mínima de efectividad, en puntos, para el veredicto.</summary>
    public int UmbralVeredictoPuntos { get; set; } = 10;
}
