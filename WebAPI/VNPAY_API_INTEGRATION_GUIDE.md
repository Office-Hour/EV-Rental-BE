# VNPay Payment Gateway API Integration Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Architecture Overview](#architecture-overview)
4. [Core Components](#core-components)
5. [Payment Flow](#payment-flow)
6. [API Integration Steps](#api-integration-steps)
7. [Security & Signature Validation](#security--signature-validation)
8. [API Endpoints](#api-endpoints)
9. [Request/Response Models](#requestresponse-models)
10. [Error Handling](#error-handling)
11. [Testing](#testing)
12. [Best Practices](#best-practices)

---

## Overview

This document provides a comprehensive guide for integrating VNPay Payment Gateway into any backend system. VNPay is a popular Vietnamese payment gateway supporting multiple payment methods including bank transfers, QR codes, and international cards.

**API Version**: 2.1.0

**Protocol**: HTTPS

**Data Format**: URL-encoded query strings for payment requests, JSON for API calls

---

## Prerequisites

Before integrating VNPay, you need:

1. **VNPay Merchant Account**: Register at https://vnpay.vn
2. **Merchant Credentials**:
   - `vnp_TmnCode`: Terminal/Merchant Code
   - `vnp_HashSecret`: Secret key for HMAC-SHA512 signature
3. **Return URLs**: Configure callback URLs for payment results
4. **Test Environment Access**: Sandbox credentials for testing

---

## Architecture Overview

### Payment Integration Flow

```
[Client] ? [Your Backend] ? [VNPay Gateway] ? [Bank/Payment Method]
                ?                    ?
         [Database]          [VNPay Webhook/IPN]
                ?                    ?
         [Return URL] ? [User Redirect] ? [VNPay]
```

### Key Components

1. **Payment Request Handler**: Creates payment URL with signed parameters
2. **Return URL Handler**: Processes customer return after payment
3. **IPN Handler**: Instant Payment Notification webhook endpoint
4. **Query Transaction API**: Check transaction status
5. **Refund API**: Process refund requests

---

## Core Components

### 1. VNPayLibrary Class

The core library for handling VNPay operations.

```csharp
public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private SortedList<String, String> _requestData;
    private SortedList<String, String> _responseData;
    
    // Add payment request parameters
    public void AddRequestData(string key, string value);
    
    // Add response parameters for validation
    public void AddResponseData(string key, string value);
    
    // Get response data by key
    public string GetResponseData(string key);
    
    // Create signed payment URL
    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret);
    
    // Validate response signature
    public bool ValidateSignature(string inputHash, string secretKey);
}
```

### 2. Utils Class

Utility functions for signature generation and IP detection.

```csharp
public class Utils
{
    // Generate HMAC-SHA512 signature
    public static string HmacSHA512(string key, string inputData);
    
    // Get client IP address
    public static string GetIpAddress();
}
```

### 3. Data Models

```csharp
public class OrderInfo
{
    public long OrderId { get; set; }
    public long Amount { get; set; }          // Amount in VND
    public string OrderDesc { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; }        // 0: Pending, 1: Success, 2: Failed
    public long PaymentTranId { get; set; }   // VNPay Transaction ID
    public string BankCode { get; set; }
    public string PayStatus { get; set; }
}
```

---

## Payment Flow

### Step 1: Create Payment Request

**Endpoint**: Your backend API endpoint (e.g., `/api/payment/create`)

**Process**:
1. Create order in your database with status "Pending"
2. Build VNPay payment URL with required parameters
3. Sign the request with HMAC-SHA512
4. Return payment URL to client
5. Client redirects to VNPay gateway

**Required Parameters**:
```
vnp_Version       : 2.1.0
vnp_Command       : pay
vnp_TmnCode       : Your merchant code
vnp_Amount        : Amount * 100 (VNPay requires amount in smallest unit)
vnp_CreateDate    : yyyyMMddHHmmss format
vnp_CurrCode      : VND
vnp_IpAddr        : Client IP address
vnp_Locale        : vn or en
vnp_OrderInfo     : Order description
vnp_OrderType     : Bill type (other, billpayment, etc.)
vnp_ReturnUrl     : URL to redirect after payment
vnp_TxnRef        : Your unique order/transaction reference
vnp_SecureHash    : HMAC-SHA512 signature
```

**Optional Parameters**:
```
vnp_BankCode      : Specific bank/payment method code
                    - VNPAYQR: VNPay QR
                    - VNBANK: Local ATM card
                    - INTCARD: International card
```

**Implementation Example**:
```csharp
public string CreatePaymentUrl(OrderInfo order)
{
    // Configuration
    string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    string vnp_ReturnUrl = "https://yoursite.com/api/payment/return";
    string vnp_TmnCode = "YOUR_TMN_CODE";
    string vnp_HashSecret = "YOUR_HASH_SECRET";
    
    // Initialize library
    VnPayLibrary vnpay = new VnPayLibrary();
    
    // Add required parameters
    vnpay.AddRequestData("vnp_Version", "2.1.0");
    vnpay.AddRequestData("vnp_Command", "pay");
    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
    vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
    vnpay.AddRequestData("vnp_CurrCode", "VND");
    vnpay.AddRequestData("vnp_IpAddr", GetClientIpAddress());
    vnpay.AddRequestData("vnp_Locale", "vn");
    vnpay.AddRequestData("vnp_OrderInfo", "Payment for order " + order.OrderId);
    vnpay.AddRequestData("vnp_OrderType", "other");
    vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
    vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());
    
    // Optional: Specify payment method
    if (!string.IsNullOrEmpty(order.BankCode))
    {
        vnpay.AddRequestData("vnp_BankCode", order.BankCode);
    }
    
    // Generate signed URL
    string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
    
    return paymentUrl;
}
```

### Step 2: Handle Return URL (Customer Redirect)

**Endpoint**: Your configured return URL (e.g., `/api/payment/return`)

**Purpose**: Handle customer redirect after payment completion

**Process**:
1. Receive all vnp_* parameters from query string
2. Validate signature to ensure data integrity
3. Extract transaction information
4. Display payment result to customer
5. Update UI based on payment status

**Important**: This endpoint is for **display purposes only**. Don't update order status here as customers may not reach this page. Use IPN for reliable status updates.

**Response Parameters**:
```
vnp_TmnCode           : Merchant code
vnp_Amount            : Amount * 100
vnp_BankCode          : Bank code used
vnp_BankTranNo        : Bank transaction number
vnp_CardType          : Card type
vnp_OrderInfo         : Order description
vnp_PayDate           : Payment date (yyyyMMddHHmmss)
vnp_ResponseCode      : Response code (00 = success)
vnp_TmnCode           : Terminal code
vnp_TransactionNo     : VNPay transaction number
vnp_TransactionStatus : Transaction status (00 = success)
vnp_TxnRef            : Your order reference
vnp_SecureHashType    : SHA512
vnp_SecureHash        : Response signature
```

**Response Codes**:
- `00`: Success
- `07`: Tr? ti?n thành công. Giao d?ch b? nghi ng? (Success. Suspect transaction)
- `09`: Giao d?ch không thành công do: Th?/Tài kho?n c?a khách hàng ch?a ??ng ký d?ch v? InternetBanking t?i ngân hàng
- `10`: Giao d?ch không thành công do: Khách hàng xác th?c thông tin th?/tài kho?n không ?úng quá 3 l?n
- `11`: Giao d?ch không thành công do: ?ã h?t h?n ch? thanh toán
- `12`: Giao d?ch không thành công do: Th?/Tài kho?n c?a khách hàng b? khóa
- `13`: Giao d?ch không thành công do Quý khách nh?p sai m?t kh?u xác th?c giao d?ch (OTP)
- `24`: Giao d?ch không thành công do: Khách hàng h?y giao d?ch
- `51`: Giao d?ch không thành công do: Tài kho?n c?a quý khách không ?? s? d? ?? th?c hi?n giao d?ch
- `65`: Giao d?ch không thành công do: Tài kho?n c?a Quý khách ?ã v??t quá h?n m?c giao d?ch trong ngày
- `75`: Ngân hàng thanh toán ?ang b?o trì
- `79`: Giao d?ch không thành công do: KH nh?p sai m?t kh?u thanh toán quá s? l?n quy ??nh
- Other: Contact VNPay for details

**Implementation Example**:
```csharp
public PaymentReturnResponse ProcessReturnUrl(Dictionary<string, string> vnpayData)
{
    string vnp_HashSecret = "YOUR_HASH_SECRET";
    VnPayLibrary vnpay = new VnPayLibrary();
    
    // Add all vnp_* parameters to library
    foreach (var item in vnpayData)
    {
        if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
        {
            vnpay.AddResponseData(item.Key, item.Value);
        }
    }
    
    // Extract key information
    long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
    long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
    string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
    string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
    long amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
    string bankCode = vnpay.GetResponseData("vnp_BankCode");
    string secureHash = vnpayData["vnp_SecureHash"];
    
    // Validate signature
    bool isValid = vnpay.ValidateSignature(secureHash, vnp_HashSecret);
    
    if (!isValid)
    {
        return new PaymentReturnResponse
        {
            Success = false,
            Message = "Invalid signature"
        };
    }
    
    // Check payment status
    if (responseCode == "00" && transactionStatus == "00")
    {
        return new PaymentReturnResponse
        {
            Success = true,
            Message = "Payment successful",
            OrderId = orderId,
            TransactionId = vnpayTranId,
            Amount = amount,
            BankCode = bankCode
        };
    }
    else
    {
        return new PaymentReturnResponse
        {
            Success = false,
            Message = $"Payment failed with code: {responseCode}",
            OrderId = orderId,
            ResponseCode = responseCode
        };
    }
}
```

### Step 3: Handle IPN (Instant Payment Notification)

**Endpoint**: Your IPN webhook URL (e.g., `/api/payment/ipn`)

**Purpose**: Reliable server-to-server notification for payment status updates

**Process**:
1. Receive notification from VNPay
2. Validate signature
3. Check order exists and amount matches
4. Update order status in database
5. Return JSON response to VNPay

**?? CRITICAL**: 
- This is the **primary** endpoint for updating order status
- Must be accessible from VNPay servers (public URL)
- Must process idempotently (handle duplicate notifications)
- Must return response within 30 seconds

**Implementation Example**:
```csharp
public string ProcessIPN(Dictionary<string, string> vnpayData)
{
    string vnp_HashSecret = "YOUR_HASH_SECRET";
    VnPayLibrary vnpay = new VnPayLibrary();
    
    // Add all vnp_* parameters
    foreach (var item in vnpayData)
    {
        if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
        {
            vnpay.AddResponseData(item.Key, item.Value);
        }
    }
    
    // Extract information
    long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
    long vnpAmount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
    long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
    string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
    string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
    string secureHash = vnpayData["vnp_SecureHash"];
    
    // Validate signature
    bool isValid = vnpay.ValidateSignature(secureHash, vnp_HashSecret);
    if (!isValid)
    {
        return "{\"RspCode\":\"97\",\"Message\":\"Invalid signature\"}";
    }
    
    // Get order from database
    var order = GetOrderById(orderId);
    if (order == null)
    {
        return "{\"RspCode\":\"01\",\"Message\":\"Order not found\"}";
    }
    
    // Validate amount
    if (order.Amount != vnpAmount)
    {
        return "{\"RspCode\":\"04\",\"Message\":\"Invalid amount\"}";
    }
    
    // Check if already processed
    if (order.Status != "0") // 0 = Pending
    {
        return "{\"RspCode\":\"02\",\"Message\":\"Order already confirmed\"}";
    }
    
    // Update order status
    if (responseCode == "00" && transactionStatus == "00")
    {
        order.Status = "1"; // Success
        order.PaymentTranId = vnpayTranId;
        UpdateOrderInDatabase(order);
        
        // Trigger business logic (e.g., send confirmation email, release goods, etc.)
        
        return "{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}";
    }
    else
    {
        order.Status = "2"; // Failed
        UpdateOrderInDatabase(order);
        return "{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}";
    }
}
```

**IPN Response Codes**:
- `00`: Success
- `01`: Order not found
- `02`: Order already confirmed
- `04`: Invalid amount
- `97`: Invalid signature
- `99`: Unknown error

---

## API Endpoints

### 1. Query Transaction API

**Purpose**: Query transaction status

**Endpoint**: `https://sandbox.vnpayment.vn/merchant_webapi/api/transaction`

**Method**: POST

**Content-Type**: application/json

**Request Body**:
```json
{
    "vnp_RequestId": "unique-request-id",
    "vnp_Version": "2.1.0",
    "vnp_Command": "querydr",
    "vnp_TmnCode": "YOUR_TMN_CODE",
    "vnp_TxnRef": "order-id",
    "vnp_OrderInfo": "Query transaction order-id",
    "vnp_TransactionDate": "20240101120000",
    "vnp_CreateDate": "20240101130000",
    "vnp_IpAddr": "127.0.0.1",
    "vnp_SecureHash": "generated-signature"
}
```

**Signature Data** (pipe-separated):
```
vnp_RequestId|vnp_Version|vnp_Command|vnp_TmnCode|vnp_TxnRef|vnp_TransactionDate|vnp_CreateDate|vnp_IpAddr|vnp_OrderInfo
```

**Implementation Example**:
```csharp
public string QueryTransaction(string orderId, string transactionDate)
{
    string vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    string vnp_HashSecret = "YOUR_HASH_SECRET";
    string vnp_TmnCode = "YOUR_TMN_CODE";
    
    // Generate request data
    string vnp_RequestId = DateTime.Now.Ticks.ToString();
    string vnp_Version = "2.1.0";
    string vnp_Command = "querydr";
    string vnp_TxnRef = orderId;
    string vnp_OrderInfo = "Query transaction: " + orderId;
    string vnp_TransactionDate = transactionDate; // yyyyMMddHHmmss
    string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
    string vnp_IpAddr = GetServerIpAddress();
    
    // Create signature
    string signData = vnp_RequestId + "|" + vnp_Version + "|" + vnp_Command + "|" + 
                      vnp_TmnCode + "|" + vnp_TxnRef + "|" + vnp_TransactionDate + "|" + 
                      vnp_CreateDate + "|" + vnp_IpAddr + "|" + vnp_OrderInfo;
    string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
    
    // Create request object
    var requestData = new
    {
        vnp_RequestId = vnp_RequestId,
        vnp_Version = vnp_Version,
        vnp_Command = vnp_Command,
        vnp_TmnCode = vnp_TmnCode,
        vnp_TxnRef = vnp_TxnRef,
        vnp_OrderInfo = vnp_OrderInfo,
        vnp_TransactionDate = vnp_TransactionDate,
        vnp_CreateDate = vnp_CreateDate,
        vnp_IpAddr = vnp_IpAddr,
        vnp_SecureHash = vnp_SecureHash
    };
    
    // Send POST request
    string jsonData = JsonSerializer.Serialize(requestData);
    var httpRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
    httpRequest.ContentType = "application/json";
    httpRequest.Method = "POST";
    
    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
    {
        streamWriter.Write(jsonData);
    }
    
    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
    {
        return streamReader.ReadToEnd();
    }
}
```

### 2. Refund API

**Purpose**: Process refund for a transaction

**Endpoint**: `https://sandbox.vnpayment.vn/merchant_webapi/api/transaction`

**Method**: POST

**Content-Type**: application/json

**Request Body**:
```json
{
    "vnp_RequestId": "unique-request-id",
    "vnp_Version": "2.1.0",
    "vnp_Command": "refund",
    "vnp_TmnCode": "YOUR_TMN_CODE",
    "vnp_TransactionType": "02",
    "vnp_TxnRef": "order-id",
    "vnp_Amount": 10000000,
    "vnp_OrderInfo": "Refund for order-id",
    "vnp_TransactionNo": "vnpay-transaction-id",
    "vnp_TransactionDate": "20240101120000",
    "vnp_CreateBy": "admin",
    "vnp_CreateDate": "20240101130000",
    "vnp_IpAddr": "127.0.0.1",
    "vnp_SecureHash": "generated-signature"
}
```

**Transaction Types**:
- `02`: Full refund
- `03`: Partial refund

**Signature Data** (pipe-separated):
```
vnp_RequestId|vnp_Version|vnp_Command|vnp_TmnCode|vnp_TransactionType|vnp_TxnRef|vnp_Amount|vnp_TransactionNo|vnp_TransactionDate|vnp_CreateBy|vnp_CreateDate|vnp_IpAddr|vnp_OrderInfo
```

**Implementation Example**:
```csharp
public string ProcessRefund(string orderId, string transactionDate, 
                            long amount, string transactionType, string createdBy)
{
    string vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    string vnp_HashSecret = "YOUR_HASH_SECRET";
    string vnp_TmnCode = "YOUR_TMN_CODE";
    
    // Generate request data
    string vnp_RequestId = DateTime.Now.Ticks.ToString();
    string vnp_Version = "2.1.0";
    string vnp_Command = "refund";
    string vnp_TransactionType = transactionType; // 02: full refund, 03: partial
    long vnp_Amount = amount * 100;
    string vnp_TxnRef = orderId;
    string vnp_OrderInfo = "Refund for order: " + orderId;
    string vnp_TransactionNo = ""; // Leave empty if not available
    string vnp_TransactionDate = transactionDate; // yyyyMMddHHmmss
    string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
    string vnp_CreateBy = createdBy;
    string vnp_IpAddr = GetServerIpAddress();
    
    // Create signature
    string signData = vnp_RequestId + "|" + vnp_Version + "|" + vnp_Command + "|" + 
                      vnp_TmnCode + "|" + vnp_TransactionType + "|" + vnp_TxnRef + "|" + 
                      vnp_Amount + "|" + vnp_TransactionNo + "|" + vnp_TransactionDate + "|" + 
                      vnp_CreateBy + "|" + vnp_CreateDate + "|" + vnp_IpAddr + "|" + vnp_OrderInfo;
    string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
    
    // Create request object
    var requestData = new
    {
        vnp_RequestId = vnp_RequestId,
        vnp_Version = vnp_Version,
        vnp_Command = vnp_Command,
        vnp_TmnCode = vnp_TmnCode,
        vnp_TransactionType = vnp_TransactionType,
        vnp_TxnRef = vnp_TxnRef,
        vnp_Amount = vnp_Amount,
        vnp_OrderInfo = vnp_OrderInfo,
        vnp_TransactionNo = vnp_TransactionNo,
        vnp_TransactionDate = vnp_TransactionDate,
        vnp_CreateBy = vnp_CreateBy,
        vnp_CreateDate = vnp_CreateDate,
        vnp_IpAddr = vnp_IpAddr,
        vnp_SecureHash = vnp_SecureHash
    };
    
    // Send POST request
    string jsonData = JsonSerializer.Serialize(requestData);
    var httpRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
    httpRequest.ContentType = "application/json";
    httpRequest.Method = "POST";
    
    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
    {
        streamWriter.Write(jsonData);
    }
    
    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
    {
        return streamReader.ReadToEnd();
    }
}
```

---

## Security & Signature Validation

### HMAC-SHA512 Signature Generation

**Purpose**: Ensure data integrity and authenticity

**Algorithm**: HMAC-SHA512

**Key Points**:
1. All parameters must be sorted alphabetically by key
2. Empty values are excluded
3. URL encoding applied to keys and values
4. Format: `key1=value1&key2=value2&key3=value3`
5. Hash result is lowercase hexadecimal string

**Implementation**:
```csharp
public static string HmacSHA512(string key, string inputData)
{
    var hash = new StringBuilder();
    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
    byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
    
    using (var hmac = new HMACSHA512(keyBytes))
    {
        byte[] hashValue = hmac.ComputeHash(inputBytes);
        foreach (var theByte in hashValue)
        {
            hash.Append(theByte.ToString("x2"));
        }
    }
    
    return hash.ToString();
}
```

### Payment URL Signature Process

1. **Build Query String**: Sort parameters alphabetically
   ```
   vnp_Amount=10000000&vnp_Command=pay&vnp_CreateDate=20240101120000&...
   ```

2. **Generate Signature**:
   ```csharp
   string signature = HmacSHA512(hashSecret, queryString);
   ```

3. **Append to URL**:
   ```
   https://vnpay.vn/paymentv2/vpcpay.html?vnp_Amount=...&vnp_SecureHash={signature}
   ```

### Response Signature Validation

1. **Extract Parameters**: Get all vnp_* parameters except vnp_SecureHash and vnp_SecureHashType

2. **Build Data String**: Same format as request (sorted, URL-encoded)

3. **Generate Expected Hash**:
   ```csharp
   string expectedHash = HmacSHA512(hashSecret, dataString);
   ```

4. **Compare**:
   ```csharp
   bool isValid = expectedHash.Equals(receivedHash, StringComparison.InvariantCultureIgnoreCase);
   ```

### API Request Signature (Query/Refund)

Different format: pipe-separated values in specific order

**Example for Query**:
```
requestId|version|command|tmnCode|txnRef|transactionDate|createDate|ipAddr|orderInfo
```

**Example for Refund**:
```
requestId|version|command|tmnCode|transactionType|txnRef|amount|transactionNo|transactionDate|createBy|createDate|ipAddr|orderInfo
```

---

## Request/Response Models

### Payment Request Model

```csharp
public class PaymentRequestModel
{
    public long OrderId { get; set; }
    public long Amount { get; set; }              // Amount in VND
    public string OrderDescription { get; set; }
    public string BankCode { get; set; }          // Optional: VNPAYQR, VNBANK, INTCARD
    public string Language { get; set; }          // vn or en
    public string OrderType { get; set; }         // Default: "other"
    public DateTime CreatedDate { get; set; }
}
```

### Payment Response Model

```csharp
public class PaymentResponseModel
{
    public long OrderId { get; set; }
    public long TransactionId { get; set; }       // VNPay transaction ID
    public long Amount { get; set; }
    public string BankCode { get; set; }
    public string BankTranNo { get; set; }
    public string CardType { get; set; }
    public string ResponseCode { get; set; }      // 00 = success
    public string TransactionStatus { get; set; } // 00 = success
    public DateTime PayDate { get; set; }
    public bool IsSuccess { get; set; }
}
```

### IPN Response Model

```csharp
public class IPNResponseModel
{
    public string RspCode { get; set; }
    public string Message { get; set; }
}
```

Response codes:
- `00`: Success
- `01`: Order not found
- `02`: Order already confirmed
- `04`: Invalid amount
- `97`: Invalid signature
- `99`: Unknown error

### Query Transaction Response

```json
{
    "vnp_ResponseId": "response-id",
    "vnp_Command": "querydr",
    "vnp_ResponseCode": "00",
    "vnp_Message": "Success",
    "vnp_TmnCode": "terminal-code",
    "vnp_TxnRef": "order-id",
    "vnp_Amount": 10000000,
    "vnp_OrderInfo": "Order description",
    "vnp_BankCode": "NCB",
    "vnp_PayDate": "20240101120000",
    "vnp_TransactionNo": "vnpay-transaction-id",
    "vnp_TransactionType": "01",
    "vnp_TransactionStatus": "00",
    "vnp_PromotionCode": "",
    "vnp_PromotionAmount": 0,
    "vnp_SecureHash": "signature"
}
```

### Refund Response

```json
{
    "vnp_ResponseId": "response-id",
    "vnp_Command": "refund",
    "vnp_ResponseCode": "00",
    "vnp_Message": "Success",
    "vnp_TmnCode": "terminal-code",
    "vnp_TxnRef": "order-id",
    "vnp_Amount": 10000000,
    "vnp_OrderInfo": "Refund description",
    "vnp_BankCode": "NCB",
    "vnp_PayDate": "20240101120000",
    "vnp_TransactionNo": "vnpay-transaction-id",
    "vnp_SecureHash": "signature"
}
```

---

## Error Handling

### Common Error Scenarios

1. **Invalid Signature**
   - Cause: Wrong hash secret or incorrect signature generation
   - Response: Return error to client or IPN failure
   - Solution: Verify hash secret and signature algorithm

2. **Duplicate Transaction Reference**
   - Cause: vnp_TxnRef not unique within the day
   - Response: VNPay rejects payment
   - Solution: Use timestamp-based unique IDs

3. **Amount Mismatch**
   - Cause: IPN amount differs from order amount
   - Response: Return RspCode 04
   - Solution: Validate amount before processing

4. **Order Already Processed**
   - Cause: Multiple IPN notifications for same order
   - Response: Return RspCode 02
   - Solution: Check order status before updating

5. **Timeout**
   - Cause: User doesn't complete payment within time limit
   - Response: ResponseCode 11
   - Solution: Handle expired orders properly

### Error Logging

Always log:
- All payment requests with order ID
- All IPN notifications received
- Signature validation failures
- API request/response for query and refund

```csharp
public void LogPaymentActivity(string activity, string orderId, string details)
{
    logger.Info($"[VNPAY] {activity} | OrderId: {orderId} | Details: {details}");
}
```

---

## Testing

### Sandbox Environment

**Payment URL**: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`

**API URL**: `https://sandbox.vnpayment.vn/merchant_webapi/api/transaction`

### Test Card Information

VNPay provides test cards for sandbox testing:

**Bank**: NCB
- Card Number: 9704198526191432198
- Card Holder: NGUYEN VAN A
- Expiry Date: 07/15
- OTP: 123456

### Test Scenarios

1. **Successful Payment**
   - Create payment with valid parameters
   - Complete payment with test card
   - Verify IPN received
   - Verify order status updated

2. **Failed Payment**
   - Initiate payment
   - Cancel on payment page
   - Verify ResponseCode 24
   - Verify order remains pending

3. **Timeout**
   - Create payment
   - Don't complete within time limit
   - Verify ResponseCode 11

4. **Invalid Signature**
   - Modify return URL parameters
   - Verify signature validation fails

5. **Query Transaction**
   - Complete a payment
   - Query transaction status
   - Verify response matches payment

6. **Refund**
   - Complete a payment
   - Process full refund
   - Verify refund success

### Integration Testing Checklist

- [ ] Payment URL generation works correctly
- [ ] Signature is properly calculated
- [ ] Return URL handles all response codes
- [ ] IPN endpoint is publicly accessible
- [ ] IPN validates signature correctly
- [ ] IPN updates order status correctly
- [ ] IPN handles duplicate notifications
- [ ] Query API works correctly
- [ ] Refund API works correctly
- [ ] Logging captures all transactions
- [ ] Error handling works properly

---

## Best Practices

### 1. Security

? **DO**:
- Store hash secret in secure configuration (environment variables, key vault)
- Always validate signatures on return URL and IPN
- Use HTTPS for all endpoints
- Log all transactions for audit trail
- Implement rate limiting on payment creation

? **DON'T**:
- Store hash secret in code or version control
- Trust return URL data without signature validation
- Process payments without IPN confirmation
- Expose internal order details in vnp_OrderInfo

### 2. Transaction Management

? **DO**:
- Generate unique vnp_TxnRef for each transaction
- Store payment request details before redirecting
- Update order status based on IPN, not return URL
- Handle duplicate IPN notifications idempotently
- Implement transaction timeout handling
- Keep transaction logs for at least 1 year

? **DON'T**:
- Reuse transaction references
- Update order status only from return URL
- Process IPN without checking order current status
- Delete transaction records

### 3. Error Handling

? **DO**:
- Implement comprehensive error logging
- Return proper IPN response codes
- Handle all payment response codes
- Provide user-friendly error messages
- Implement retry logic for API calls

? **DON'T**:
- Ignore IPN errors silently
- Show technical errors to users
- Fail silently without logging

### 4. Performance

? **DO**:
- Process IPN quickly (< 30 seconds)
- Use asynchronous processing for heavy operations
- Cache configuration values
- Implement connection pooling for database
- Use queues for post-payment processing

? **DON'T**:
- Perform heavy operations in IPN handler
- Make synchronous calls to slow external services
- Block IPN response with business logic

### 5. User Experience

? **DO**:
- Provide clear payment instructions
- Show payment status clearly
- Handle browser back button properly
- Implement payment status polling
- Send email confirmations

? **DON'T**:
- Redirect users away from payment result
- Assume users complete payment flow
- Rely solely on return URL for order completion

### 6. Monitoring

? **DO**:
- Monitor IPN endpoint availability
- Track payment success/failure rates
- Alert on signature validation failures
- Monitor API response times
- Track refund rates

? **DON'T**:
- Rely on manual checking
- Ignore failed transactions
- Skip performance monitoring

---

## Configuration Example

### appsettings.json (ASP.NET Core)

```json
{
  "VNPay": {
    "Version": "2.1.0",
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ApiUrl": "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction",
    "ReturnUrl": "https://yoursite.com/api/payment/return",
    "IpnUrl": "https://yoursite.com/api/payment/ipn"
  }
}
```

### Web.config (.NET Framework)

```xml
<appSettings>
  <add key="vnp_Version" value="2.1.0"/>
  <add key="vnp_TmnCode" value="YOUR_TMN_CODE"/>
  <add key="vnp_HashSecret" value="YOUR_HASH_SECRET"/>
  <add key="vnp_Url" value="https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"/>
  <add key="vnp_Api" value="https://sandbox.vnpayment.vn/merchant_webapi/api/transaction"/>
  <add key="vnp_Returnurl" value="https://yoursite.com/vnpay_return"/>
</appSettings>
```

### Environment Variables

```bash
VNPAY_TMN_CODE=your_tmn_code
VNPAY_HASH_SECRET=your_hash_secret
VNPAY_PAYMENT_URL=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VNPAY_API_URL=https://sandbox.vnpayment.vn/merchant_webapi/api/transaction
VNPAY_RETURN_URL=https://yoursite.com/api/payment/return
VNPAY_IPN_URL=https://yoursite.com/api/payment/ipn
```

---

## Production Checklist

Before going live:

### Configuration
- [ ] Update to production URLs
- [ ] Configure production credentials
- [ ] Set correct return and IPN URLs
- [ ] Verify URLs are publicly accessible
- [ ] Enable HTTPS on all endpoints

### Security
- [ ] Store hash secret securely
- [ ] Implement signature validation on all endpoints
- [ ] Add rate limiting
- [ ] Implement request validation
- [ ] Add logging for all transactions

### Testing
- [ ] Test full payment flow
- [ ] Test IPN notification handling
- [ ] Test all error scenarios
- [ ] Test refund process
- [ ] Test query transaction
- [ ] Perform load testing

### Monitoring
- [ ] Set up logging
- [ ] Configure alerts for failures
- [ ] Monitor IPN endpoint
- [ ] Track payment metrics
- [ ] Set up error tracking

### Documentation
- [ ] Document internal processes
- [ ] Create runbooks for common issues
- [ ] Document troubleshooting steps
- [ ] Train support team

---

## Support & Resources

### Official Documentation
- VNPay API Documentation: Contact VNPay for access
- VNPay Sandbox: https://sandbox.vnpayment.vn

### Contact VNPay
- Technical Support: support@vnpay.vn
- Business Inquiry: business@vnpay.vn
- Website: https://vnpay.vn

### Useful Response Codes Reference

#### Payment Response Codes
| Code | Description |
|------|-------------|
| 00 | Transaction successful |
| 07 | Transaction successful but suspicious |
| 09 | Internet Banking not registered |
| 10 | Authentication failed 3 times |
| 11 | Payment timeout |
| 12 | Account locked |
| 13 | Wrong OTP |
| 24 | Customer cancelled |
| 51 | Insufficient balance |
| 65 | Transaction limit exceeded |
| 75 | Bank under maintenance |
| 79 | Wrong password too many times |

#### IPN Response Codes
| Code | Description |
|------|-------------|
| 00 | Success |
| 01 | Order not found |
| 02 | Order already confirmed |
| 04 | Invalid amount |
| 97 | Invalid signature |
| 99 | Unknown error |

---

## Conclusion

This guide provides a comprehensive overview of VNPay payment gateway integration. The core concepts remain the same across different backend technologies:

1. **Create Payment Request**: Build signed URL and redirect customer
2. **Handle Return**: Display payment result (optional, for UX only)
3. **Process IPN**: Update order status reliably (required)
4. **Query Transaction**: Check payment status when needed
5. **Process Refund**: Handle refund requests

Key success factors:
- Proper signature generation and validation
- Reliable IPN handling
- Comprehensive error handling
- Thorough testing
- Good monitoring and logging

Adapt the code examples to your specific backend framework while maintaining the same integration principles.

---

**Document Version**: 1.0
**Last Updated**: 2024
**Based on VNPay API Version**: 2.1.0
