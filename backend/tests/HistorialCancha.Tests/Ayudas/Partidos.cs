using HistorialCancha.Domain.Entidades;

namespace HistorialCancha.Tests.Ayudas;

/// <summary>
/// Fábrica de partidos para los tests de estadísticas. Existe para que cada test
/// declare SÓLO lo que le importa —el marcador, la modalidad o el rival— y no diez
/// campos de relleno que esconden qué se está probando.
/// </summary>
internal static class Partidos
{
    private const string RivalPorDefecto = "River Plate";

    /// <summary>
    /// Partido del día <paramref name="dia"/> de enero de 2026. Las fechas concretas
    /// no importan para las estadísticas, pero el ORDEN sí: las rachas son secuencias
    /// temporales, así que un día distinto por partido alcanza y se lee fácil.
    /// </summary>
    public static Partido Dia(int dia, int aFavor, int enContra,
        Modalidad modalidad = Modalidad.EnCancha, string rival = RivalPorDefecto) =>
        new()
        {
            Fecha = new DateOnly(2026, 1, dia),
            Rival = rival,
            Torneo = "Liga Profesional",
            GolesAFavor = aFavor,
            GolesEnContra = enContra,
            Vivencia = new Vivencia { Modalidad = modalidad }
        };

    /// <summary>Varios partidos con el mismo marcador y modalidad, en días consecutivos.</summary>
    public static IEnumerable<Partido> Varios(int cuantos, int aFavor, int enContra,
        Modalidad modalidad = Modalidad.EnCancha, string rival = RivalPorDefecto, int desdeElDia = 1) =>
        Enumerable.Range(desdeElDia, cuantos)
            .Select(d => Dia(d, aFavor, enContra, modalidad, rival));

    /// <summary>Partido sin vivencia cargada: existe en el historial pero sin modalidad.</summary>
    public static Partido SinVivencia(int dia, int aFavor, int enContra, string rival = RivalPorDefecto) =>
        new()
        {
            Fecha = new DateOnly(2026, 1, dia),
            Rival = rival,
            Torneo = "Liga Profesional",
            GolesAFavor = aFavor,
            GolesEnContra = enContra,
            Vivencia = null
        };
}
