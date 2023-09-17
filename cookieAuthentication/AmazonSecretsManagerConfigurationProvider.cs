using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
{
    private readonly string _region;
    private readonly string _secretName;
    
    public AmazonSecretsManagerConfigurationProvider(string region, string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public override void Load()
    {
        var secret = GetSecret();

        Data = JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
    }

   private string GetSecret()
    {
        /*
        var request = new GetSecretValueRequest
        {
            SecretId = _secretName,
            VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
        };

        using (var client = 
		new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region)))
        {
            var response = client.GetSecretValueAsync(request).Result;

            string secretString;
            if (response.SecretString != null)
            {
                secretString = response.SecretString;
            }
            else
            {
                var memoryStream = response.SecretBinary;
                var reader = new StreamReader(memoryStream);
                secretString = 
		System.Text.Encoding.UTF8
			.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            }

            return secretString;
        }
        */
        return @"{
    ""TEST:Instance"": ""https://login.microsoftonline.com/"",
    ""TEST:Domain"": ""tenant_name"",
    ""TEST:TenantId"": ""tenant_id"",
    ""TEST:ClientId"": ""azure_ad_app_id"",
    ""TEST:ClientSecret"": ""azure_ad_client_secret"",
    ""TEST:CallbackPath"": ""/home"", 
    ""TEST:SignedOutCallbackPath "": ""/signout-callback-oidc""
  }";
    }
}


public class AmazonSecretsManagerConfigurationSource : IConfigurationSource
{
    private readonly string _region;
    private readonly string _secretName;

    public AmazonSecretsManagerConfigurationSource(string region, string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AmazonSecretsManagerConfigurationProvider(_region, _secretName);
    }
}


public class MyApiCredentials
{
    public string Instance { get; set; }
    public string Domain { get; set; }
    public string TenantId { get;set; }
    public string ClientId { get;set; }
    public string ClientSecret { get;set; }
    public string CallbackPath { get;set; }
    public string SignedOutCallbackPath { get;set; }
}