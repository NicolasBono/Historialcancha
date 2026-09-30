using System.Diagnostics.CodeAnalysis;

// Toda esta capa queda afuera de la cuenta de cobertura, y por tres motivos distintos:
//
//  · Las migraciones de Persistencia/Migraciones las escribió Entity Framework, no yo.
//    Testearlas sería testear al generador.
//  · El DbContext, las configuraciones de EF y los repositorios no tienen reglas de
//    negocio: traducen entre el dominio y SQL. Verificar que esa traducción ande
//    exige una base de datos de verdad, o sea un test de integración, no unitario.
//    El dominio no depende de ellos: depende de las interfaces que él mismo declara,
//    y ésas se verifican con dobles.
//  · El hasheo y la firma de tokens son envoltorios finitos sobre PBKDF2 y sobre el
//    JWT del framework. Lo que hay que verificar ahí no es la cuenta sino que la
//    configuración esté bien puesta, y eso se ve arrancando la app.
//
// Se declara a nivel de ensamblado y no con un filtro en el pipeline a propósito:
// el atributo lo respetan tanto coverlet (el que aplica el umbral) como
// ReportGenerator (el que arma el reporte), así que el número que frena el build y
// el número que se muestra no se pueden desincronizar. Un filtro escrito en dos
// lugares sí se desincroniza, y entonces el reporte dice una cosa y el umbral otra.
[assembly: ExcludeFromCodeCoverage]
