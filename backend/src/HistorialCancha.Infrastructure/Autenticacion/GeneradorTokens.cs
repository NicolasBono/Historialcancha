using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HistorialCancha.Domain.Entidades;
using Microsoft.IdentityModel.Tokens;

namespace HistorialCancha.Infrastructure.Autenticacion;

public record TokenGenerado(string Token, DateTime ExpiraEn);

/// <summary>
/// Emite el JWT firmado que el frontend guarda y manda en cada request.
/// El id del usuario viaja en el claim estándar <c>sub</c> / NameIdentifier:
/// de ahí lo saca cada controller para acotar los datos a su dueño.
/// </summary>
public interface IGeneradorTokens
{
    TokenGenerado Generar(Usuario usuario);
}

public class GeneradorTokens : IGeneradorTokens
{
    private readonly OpcionesJwt _opciones;

    public GeneradorTokens(OpcionesJwt opciones) => _opciones = opciones;

    public TokenGenerado Generar(Usuario usuario)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(_opciones.ExpiraMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim("nombre", usuario.Nombre),
            new Claim("apellido", usuario.Apellido),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
