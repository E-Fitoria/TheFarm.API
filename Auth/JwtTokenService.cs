using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TheFarm.API.Domain.Entities;

namespace TheFarm.API.Auth;

/// <summary>
/// Servicio encargado de crear JWTs (JSON Web Tokens) después de un login exitoso.
/// El token es un "pase" firmado: el cliente lo envía en cada request (Authorization: Bearer ...)
/// y la API lo valida sin volver a consultar la contraseña.
/// </summary>
public class JwtTokenService
{
    // IConfiguration lee valores de appsettings.json, User Secrets, variables de entorno, etc.
    private readonly IConfiguration _config;

    // Inyección de dependencias: ASP.NET Core pasa IConfiguration al crear esta clase.
    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Genera un JWT con los datos del usuario autenticado.
    /// Retorna una tupla: el string del token y la fecha UTC en que deja de ser válido.
    /// </summary>
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        // Clave secreta usada para firmar el token (HMAC). Debe ser larga y nunca subirse al repo.
        // Si falta, fallamos explícitamente: sin Key no se puede emitir tokens seguros.
        var key = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurada");

        // Quién emite el token (esta API). Quien lo valida debe coincidir con este valor.
        var issuer = _config["Jwt:Issuer"];

        // Para quién está pensado el token (tu frontend/cliente). También se valida al recibir requests.
        var audience = _config["Jwt:Audience"];

        // Tiempo de vida del token en minutos (por defecto 60 si no está en config).
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");

        // Momento exacto (UTC) en que el token expira. Después de esa fecha la API lo rechaza.
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        // Claims = datos embebidos en el token (payload).
        // Cualquiera puede leerlos si decodifica el JWT, pero no puede alterarlos sin invalidar la firma.
        // No pongas aquí secretos (contraseñas, etc.).
        var claims = new List<Claim>
        {
            // "sub" (subject): identidad principal del usuario (estándar JWT).
            new(JwtRegisteredClaimNames.Sub, user.IdUser.ToString()),

            // Nombre de usuario legible (útil en logs / UI).
            new(JwtRegisteredClaimNames.UniqueName, user.Username),

            // Rol para autorización: permite [Authorize(Roles = "Admin")] y User.IsInRole(...).
            new(ClaimTypes.Role, user.Role),

            // Claim custom: mismo id, con un nombre propio por si lo lees a mano en el código.
            new("idUser", user.IdUser.ToString())
        };

        // Convierte la clave string a bytes y crea la llave simétrica de firma.
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        // Credenciales de firma: algoritmo HMAC-SHA256 (misma clave para firmar y validar).
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Arma el JWT con emisor, audiencia, claims, expiración y firma.
        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        // Serializa el token a string (header.payload.signature en Base64Url).
        // Eso es lo que se devuelve al cliente en el login.
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return (token, expiresAt);
    }
}
