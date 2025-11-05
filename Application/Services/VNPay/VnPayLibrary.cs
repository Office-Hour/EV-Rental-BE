using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.VNPay;

/// <summary>
/// Core library for VNPay payment gateway operations
/// Handles parameter management and signature generation
/// </summary>
public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new();
    private readonly SortedList<string, string> _responseData = new();

    /// <summary>
    /// Add a parameter to the payment request
    /// </summary>
    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    /// <summary>
    /// Add a parameter from the payment response
    /// </summary>
    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    /// <summary>
    /// Get a value from the response data
    /// </summary>
    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Create the signed payment URL
    /// </summary>
    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();
        
        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        var queryString = data.ToString();
        
        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1); // Remove trailing &
        }

        var signData = queryString;
        var vnpSecureHash = Utils.HmacSHA512(vnpHashSecret, signData);
        
        return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
    }

    /// <summary>
    /// Validate the signature from VNPay response
    /// </summary>
    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var data = new StringBuilder();
        
        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value) && 
                                                                 !kv.Key.Equals("vnp_SecureHash", StringComparison.InvariantCultureIgnoreCase) &&
                                                                 !kv.Key.Equals("vnp_SecureHashType", StringComparison.InvariantCultureIgnoreCase)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        var checkSum = data.ToString();
        
        if (checkSum.Length > 0)
        {
            checkSum = checkSum.Remove(checkSum.Length - 1, 1);
        }

        var vnpSecureHash = Utils.HmacSHA512(secretKey, checkSum);
        
        return vnpSecureHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }
}

/// <summary>
/// Utility functions for VNPay integration
/// </summary>
public static class Utils
{
    /// <summary>
    /// Generate HMAC-SHA512 signature
    /// </summary>
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        
        return hash.ToString();
    }

    /// <summary>
    /// Get current date/time in VNPay format (yyyyMMddHHmmss)
    /// </summary>
    public static string GetVnPayDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parse VNPay date/time format to DateTime
    /// </summary>
    public static DateTime ParseVnPayDateTime(string vnpDateTime)
    {
        return DateTime.ParseExact(vnpDateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }
}
