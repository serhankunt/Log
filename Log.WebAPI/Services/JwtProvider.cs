using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Log.WebAPI.Services;

public class JwtProvider
{
    public string CreateToken()
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier,"SerhanKunt"),
        };
        
        DateTime expires = DateTime.Now.AddDays(1);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: "Serhan",
            audience: "Ozge",
            claims: claims,
            expires: expires,
            signingCredentials: 
                new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("my secret key my secret key my secret key my secret key my secret key")), 
                        SecurityAlgorithms.HmacSha512));

        JwtSecurityTokenHandler handler = new();
        string token = handler.WriteToken(jwtSecurityToken);

        return token;
    }
}
