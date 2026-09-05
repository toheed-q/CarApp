using System.Text;
using System.Text.Json;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DMF_Services.Services
{
    // Fast2SMS OTP integration. Fast2SMS owns OTP generation and verification;
    // this service only forwards the request with our server-side API key.
    // Endpoints (see https://docs.fast2sms.com):
    //   POST {BaseUrl}/send    { mobile, otp_id, otp_expiry, otp_length }
    //   POST {BaseUrl}/resend  { mobile }
    //   POST {BaseUrl}/verify  { mobile, otp }
    public class OtpService : IOtpService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly Fast2SmsOptions _opt;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IHttpClientFactory httpFactory,
            IOptions<Fast2SmsOptions> opt,
            ILogger<OtpService> logger)
        {
            _httpFactory = httpFactory;
            _opt = opt.Value;
            _logger = logger;
        }

        public bool Enabled => _opt.Enabled;

        public Task<OtpResult> SendAsync(string mobile)
        {
            var payload = new
            {
                mobile = Normalize(mobile),
                otp_id = _opt.OtpId,
                otp_expiry = _opt.OtpExpiryMinutes,
                otp_length = _opt.OtpLength
            };
            return PostAsync("send", payload, "OTP sent successfully");
        }

        public Task<OtpResult> ResendAsync(string mobile)
        {
            var payload = new { mobile = Normalize(mobile) };
            return PostAsync("resend", payload, "OTP resent successfully");
        }

        public Task<OtpResult> VerifyAsync(string mobile, string otp)
        {
            var payload = new { mobile = Normalize(mobile), otp };
            return PostAsync("verify", payload, "OTP verified successfully", failMessage: "Invalid OTP");
        }

        // ------------------------------------------------------------------
        private async Task<OtpResult> PostAsync(string path, object payload, string okMessage, string? failMessage = null)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_opt.BaseUrl.TrimEnd('/')}/{path}");
                request.Headers.TryAddWithoutValidation("accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _opt.ApiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                var (ok, message) = Parse(body);

                if (ok)
                    return OtpResult.Ok(okMessage);

                // Fast2SMS returned return:false (e.g. wrong OTP) — surface a clean message.
                return OtpResult.Fail(message ?? failMessage ?? "Request failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fast2SMS {Path} call failed.", path);
                return OtpResult.Fail("SMS service is temporarily unavailable. Please try again.");
            }
        }

        // Fast2SMS responses look like { "return": true, "message": [...] } or
        // { "return": false, "message": "OTP Not Matched" }.
        private static (bool ok, string? message) Parse(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                bool ok = root.TryGetProperty("return", out var ret)
                          && ret.ValueKind == JsonValueKind.True;

                string? message = null;
                if (root.TryGetProperty("message", out var msg))
                {
                    message = msg.ValueKind switch
                    {
                        JsonValueKind.String => msg.GetString(),
                        JsonValueKind.Array => string.Join(" ",
                            msg.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrWhiteSpace(s))),
                        _ => null
                    };
                }

                return (ok, message);
            }
            catch
            {
                return (false, null);
            }
        }

        // Fast2SMS expects a bare 10-digit Indian number. Strip spaces, +91, etc.
        private static string Normalize(string mobile)
        {
            var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
            return digits.Length > 10 ? digits[^10..] : digits;
        }
    }
}
