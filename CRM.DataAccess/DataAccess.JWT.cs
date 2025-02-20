namespace CRM;

public partial interface IDataAccess
{
    string GetSourceJWT(Guid TenantId, string Source);
    Dictionary<string, object> JwtDecode(Guid TenantId, string Encrypted);
    string JwtEncode(Guid TenantId, Dictionary<string, object> Payload);
    bool ValidateSourceJWT(Guid TenantId, string Source, string JWT);
}

public partial class DataAccess
{
    public string GetSourceJWT(Guid TenantId, string Source)
    {
        string output = String.Empty;
        Dictionary<string, object> Payload = new Dictionary<string, object> {
                {"Source", Source }
            };
        output = JwtEncode(TenantId, Payload);
        return output;
    }

    public Dictionary<string, object> JwtDecode(Guid TenantId, string Encrypted)
    {
        Dictionary<string, object> output = new Dictionary<string, object>();

        var settings = GetTenantSettings(TenantId);
        var jwt = new JWTHelper(_appName, StringValue(settings.JwtRsaPublicKey), StringValue(settings.JwtRsaPrivateKey));

        var decoded = jwt.Decode(Encrypted);
        if (decoded.Success && decoded.Payload != null) {
            output = decoded.Payload;
        }

        return output;
    }

    public string JwtEncode(Guid TenantId, Dictionary<string, object> Payload)
    {
        string output = String.Empty;

        var settings = GetTenantSettings(TenantId);
        var jwt = new JWTHelper(_appName, StringValue(settings.JwtRsaPublicKey), StringValue(settings.JwtRsaPrivateKey));

        var encoded = jwt.Encode(Payload);
        if (encoded.Success) {
            output += encoded.Token;
        }

        return output;
    }

    public bool ValidateSourceJWT(Guid TenantId, string Source, string JWT)
    {
        bool output = false;

        string SourceCheck = String.Empty;
        Dictionary<string, object> decrypted = JwtDecode(TenantId, JWT);
        try {
            SourceCheck = decrypted["Source"] + String.Empty;
            if (SourceCheck == Source) {
                output = true;
            }
        } catch { }

        return output;
    }
}