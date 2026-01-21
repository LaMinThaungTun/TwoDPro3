using System.Net.Http.Headers;
using System.Text;

public class TwilioVerifyService
{
    private readonly HttpClient _http;
    private readonly string _verifySid;
    private readonly string _accountSid;
    private readonly string _authToken;

    public TwilioVerifyService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _verifySid = config["Twilio:VerifyServiceSid"];
        _accountSid = config["Twilio:AccountSid"];
        _authToken = config["Twilio:AuthToken"];

        var authBytes = Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    public async Task<bool> SendOtpAsync(string phone)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("To", phone),
            new KeyValuePair<string, string>("Channel", "sms")
        });

        var response = await _http.PostAsync(
            $"https://verify.twilio.com/v2/Services/{_verifySid}/Verifications",
            content
        );

        return response.IsSuccessStatusCode;
    }
    public async Task<bool> VerifyOtpAsync(string phone, string code)
    {
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("To", phone),
        new KeyValuePair<string, string>("Code", code)
    });

        var response = await _http.PostAsync(
            $"https://verify.twilio.com/v2/Services/{_verifySid}/VerificationCheck",
            content
        );

        return response.IsSuccessStatusCode;
    }

}
