# VNPay Payment API - Angular 20 Integration Guide

This guide provides step-by-step instructions for integrating the VNPay Payment Gateway API into an Angular 20 frontend application.

---

## ?? Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Architecture & Flow](#architecture--flow)
4. [Setup Angular Project](#setup-angular-project)
5. [Create Payment Service](#create-payment-service)
6. [Create Payment Models](#create-payment-models)
7. [Implement Payment Flow](#implement-payment-flow)
8. [Handle Payment Result](#handle-payment-result)
9. [Error Handling](#error-handling)
10. [Security Considerations](#security-considerations)
11. [Testing](#testing)
12. [Complete Examples](#complete-examples)

---

## ?? Overview

The VNPay Payment Gateway integration follows this flow:

1. **Frontend** calls backend API to create payment URL
2. **Backend** generates signed VNPay URL and returns it
3. **Frontend** redirects user to VNPay gateway
4. User completes payment on VNPay
5. **VNPay** sends IPN (webhook) to backend (server-to-server)
6. **VNPay** redirects user back to frontend with payment result
7. **Frontend** displays payment result to user

---

## ?? Prerequisites

### Backend Requirements
- Backend API running (WebAPI project)
- VNPay credentials configured in `appsettings.json`
- Payment endpoints accessible

### Frontend Requirements
- Angular 20
- Node.js 18+ and npm
- TypeScript 5+
- RxJS 7+

### Required Packages

```bash
npm install --save @angular/common @angular/core @angular/router
npm install --save rxjs
```

---

## ??? Architecture & Flow

### Payment Flow Diagram

```
???????????????      ???????????????      ???????????????
?   Angular   ?      ?   Backend   ?      ?   VNPay     ?
?  Frontend   ?      ?   (API)     ?      ?  Gateway    ?
???????????????      ???????????????      ???????????????
       ?                    ?                     ?
       ? 1. POST /api/      ?                     ?
       ?    payment/create  ?                     ?
       ????????????????????>?                     ?
       ?                    ?                     ?
       ? 2. Return payment  ?                     ?
       ?    URL             ?                     ?
       ?<????????????????????                     ?
       ?                    ?                     ?
       ? 3. Redirect user   ?                     ?
       ?    to payment URL  ?                     ?
       ??????????????????????????????????????????>?
       ?                    ?                     ?
       ?                    ? 4. IPN Webhook      ?
       ?                    ?    (server-to-      ?
       ?                    ?     server)         ?
       ?                    ?<?????????????????????
       ?                    ?                     ?
       ?                    ? 5. Update payment   ?
       ?                    ?    status in DB     ?
       ?                    ?                     ?
       ? 6. User redirect   ?                     ?
       ?    after payment   ?                     ?
       ?<??????????????????????????????????????????
       ?                    ?                     ?
       ? 7. Display result  ?                     ?
       ?    to user         ?                     ?
       ?                    ?                     ?
```

### Key Points

- ? **Step 4 (IPN)** is where payment status is **actually updated** in the database
- ?? **Step 6 (Return URL)** is **only for display** - user may not reach this page
- ?? Both IPN and Return URL validate VNPay signature for security

---

## ?? Setup Angular Project

### Project Structure

```
src/
??? app/
?   ??? core/
?   ?   ??? services/
?   ?   ?   ??? payment.service.ts
?   ?   ?   ??? http-client.service.ts
?   ?   ??? interceptors/
?   ?   ?   ??? auth.interceptor.ts
?   ?   ??? guards/
?   ?       ??? auth.guard.ts
?   ??? features/
?   ?   ??? payment/
?   ?       ??? models/
?   ?       ?   ??? payment.models.ts
?   ?       ??? components/
?   ?       ?   ??? payment-form/
?   ?       ?   ??? payment-result/
?   ?       ?   ??? payment-processing/
?   ?       ??? pages/
?   ?           ??? payment-page/
?   ?           ??? payment-callback/
?   ??? shared/
?       ??? models/
?       ?   ??? api-response.model.ts
?       ??? utils/
?           ??? error-handler.ts
```

---

## ?? Create Payment Models

### File: `src/app/features/payment/models/payment.models.ts`

```typescript
/**
 * Request model for creating VNPay payment
 */
export interface VnPayPaymentRequest {
  orderId: string;           // Booking ID (GUID)
  amount: number;            // Amount in VND
  orderDescription: string;  // Payment description
  bankCode?: string;         // Optional: VNPAYQR, VNBANK, INTCARD
  locale?: string;           // Language: "vn" or "en" (default: "vn")
  orderType?: string;        // Order type (default: "other")
  ipAddress?: string;        // Client IP (optional, backend will detect)
}

/**
 * Response model from payment creation
 */
export interface VnPayPaymentResponse {
  paymentUrl: string;        // URL to redirect user to VNPay
  orderId: string;           // Order/booking reference
  amount: number;            // Payment amount
  createdAt: string;         // ISO 8601 timestamp
}

/**
 * Payment result model from VNPay return URL
 */
export interface VnPayReturnData {
  orderId: string;           // Order/booking reference
  transactionId: number;     // VNPay transaction ID
  amount: number;            // Amount paid
  bankCode: string;          // Bank code used
  bankTranNo: string;        // Bank transaction number
  cardType: string;          // Card type
  responseCode: string;      // Response code (00 = success)
  transactionStatus: string; // Transaction status (00 = success)
  payDate: string;           // Payment date (ISO 8601)
  isSuccess: boolean;        // Whether payment succeeded
  isValidSignature: boolean; // Whether signature is valid
  orderInfo: string;         // Order description
}

/**
 * API response wrapper
 */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

/**
 * Payment status for local tracking
 */
export enum PaymentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Success = 'Success',
  Failed = 'Failed',
  Cancelled = 'Cancelled'
}

/**
 * Response codes from VNPay
 */
export const VNPAY_RESPONSE_CODES: { [key: string]: string } = {
  '00': 'Giao d?ch thành công',
  '07': 'Giao d?ch thành công (nghi ng?)',
  '09': 'Ch?a ??ng ký Internet Banking',
  '10': 'Xác th?c th?t b?i quá 3 l?n',
  '11': 'H?t h?n ch? thanh toán',
  '12': 'Tài kho?n b? khóa',
  '13': 'Nh?p sai OTP',
  '24': 'Khách hàng h?y giao d?ch',
  '51': 'Tài kho?n không ?? s? d?',
  '65': 'V??t quá h?n m?c giao d?ch',
  '75': 'Ngân hàng ?ang b?o trì',
  '79': 'Nh?p sai m?t kh?u quá s? l?n quy ??nh'
};
```

---

## ?? Create Payment Service

### File: `src/app/core/services/payment.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  VnPayPaymentRequest,
  VnPayPaymentResponse,
  VnPayReturnData,
  ApiResponse,
  PaymentStatus
} from '../../features/payment/models/payment.models';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly API_URL = environment.apiUrl;
  private readonly PAYMENT_ENDPOINT = `${this.API_URL}/api/payment`;

  constructor(private http: HttpClient) {}

  /**
   * Create payment URL and redirect user to VNPay gateway
   * @param request Payment request details
   * @returns Observable with payment response
   */
  createPayment(request: VnPayPaymentRequest): Observable<ApiResponse<VnPayPaymentResponse>> {
    const url = `${this.PAYMENT_ENDPOINT}/create`;
    
    console.log('[PaymentService] Creating payment:', request);

    return this.http.post<ApiResponse<VnPayPaymentResponse>>(url, request).pipe(
      tap(response => {
        console.log('[PaymentService] Payment URL created:', response);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Process payment return from VNPay (called by return URL)
   * @param queryParams Query parameters from VNPay
   * @returns Observable with payment result
   */
  processPaymentReturn(queryParams: { [key: string]: string }): Observable<ApiResponse<VnPayReturnData>> {
    const url = `${this.PAYMENT_ENDPOINT}/return`;
    const queryString = new URLSearchParams(queryParams).toString();
    
    console.log('[PaymentService] Processing payment return');

    return this.http.get<ApiResponse<VnPayReturnData>>(`${url}?${queryString}`).pipe(
      tap(response => {
        console.log('[PaymentService] Payment result:', response);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Redirect user to VNPay payment gateway
   * @param paymentUrl VNPay payment URL
   */
  redirectToPaymentGateway(paymentUrl: string): void {
    console.log('[PaymentService] Redirecting to VNPay:', paymentUrl);
    window.location.href = paymentUrl;
  }

  /**
   * Get payment status message from response code
   * @param responseCode VNPay response code
   * @returns Human-readable message
   */
  getPaymentStatusMessage(responseCode: string): string {
    const VNPAY_RESPONSE_CODES: { [key: string]: string } = {
      '00': 'Giao d?ch thành công',
      '07': 'Giao d?ch thành công (nghi ng?)',
      '09': 'Ch?a ??ng ký Internet Banking',
      '10': 'Xác th?c th?t b?i quá 3 l?n',
      '11': 'H?t h?n ch? thanh toán',
      '12': 'Tài kho?n b? khóa',
      '13': 'Nh?p sai OTP',
      '24': 'Khách hàng h?y giao d?ch',
      '51': 'Tài kho?n không ?? s? d?',
      '65': 'V??t quá h?n m?c giao d?ch',
      '75': 'Ngân hàng ?ang b?o trì',
      '79': 'Nh?p sai m?t kh?u quá s? l?n quy ??nh'
    };

    return VNPAY_RESPONSE_CODES[responseCode] || 'L?i không xác ??nh';
  }

  /**
   * Store payment info in session storage for tracking
   */
  storePaymentInfo(orderId: string, amount: number): void {
    const paymentInfo = {
      orderId,
      amount,
      timestamp: new Date().toISOString(),
      status: PaymentStatus.Processing
    };
    sessionStorage.setItem('currentPayment', JSON.stringify(paymentInfo));
  }

  /**
   * Get stored payment info from session storage
   */
  getStoredPaymentInfo(): any {
    const stored = sessionStorage.getItem('currentPayment');
    return stored ? JSON.parse(stored) : null;
  }

  /**
   * Clear stored payment info
   */
  clearStoredPaymentInfo(): void {
    sessionStorage.removeItem('currentPayment');
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = '?ã x?y ra l?i khi x? lý thanh toán';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `L?i: ${error.error.message}`;
    } else {
      // Server-side error
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else {
        errorMessage = `L?i máy ch?: ${error.status} - ${error.message}`;
      }
    }

    console.error('[PaymentService] Error:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }
}
```

### File: `src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001', // Update with your backend URL
  vnpay: {
    returnUrl: 'http://localhost:4200/payment/callback',
    defaultLocale: 'vn',
    defaultOrderType: 'other'
  }
};
```

### File: `src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com',
  vnpay: {
    returnUrl: 'https://your-production-frontend.com/payment/callback',
    defaultLocale: 'vn',
    defaultOrderType: 'other'
  }
};
```

---

## ?? Implement Payment Flow

### File: `src/app/features/payment/components/payment-form/payment-form.component.ts`

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PaymentService } from '../../../../core/services/payment.service';
import { VnPayPaymentRequest } from '../../models/payment.models';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-form.component.html',
  styleUrls: ['./payment-form.component.scss']
})
export class PaymentFormComponent implements OnInit {
  @Input() bookingId!: string;
  @Input() amount!: number;
  @Input() description?: string;

  isProcessing = false;
  errorMessage: string | null = null;

  // Payment method options
  paymentMethods = [
    { code: 'VNPAYQR', name: 'VNPay QR', icon: 'qr_code' },
    { code: 'VNBANK', name: 'Th? ATM n?i ??a', icon: 'credit_card' },
    { code: 'INTCARD', name: 'Th? qu?c t?', icon: 'payment' }
  ];

  selectedBankCode?: string;
  locale: string = environment.vnpay.defaultLocale;

  constructor(
    private paymentService: PaymentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.bookingId || !this.amount) {
      this.errorMessage = 'Thông tin thanh toán không h?p l?';
    }
  }

  /**
   * Handle payment submission
   */
  onSubmit(): void {
    if (!this.bookingId || !this.amount) {
      this.errorMessage = 'Vui lòng ?i?n ??y ?? thông tin';
      return;
    }

    this.isProcessing = true;
    this.errorMessage = null;

    const paymentRequest: VnPayPaymentRequest = {
      orderId: this.bookingId,
      amount: this.amount,
      orderDescription: this.description || `Thanh toán ??t c?c cho booking ${this.bookingId}`,
      bankCode: this.selectedBankCode,
      locale: this.locale,
      orderType: environment.vnpay.defaultOrderType
    };

    // Store payment info for later tracking
    this.paymentService.storePaymentInfo(this.bookingId, this.amount);

    // Create payment and redirect
    this.paymentService.createPayment(paymentRequest).subscribe({
      next: (response) => {
        if (response.success && response.data?.paymentUrl) {
          console.log('Redirecting to VNPay payment gateway...');
          // Redirect to VNPay gateway
          this.paymentService.redirectToPaymentGateway(response.data.paymentUrl);
        } else {
          this.isProcessing = false;
          this.errorMessage = response.message || 'Không th? t?o liên k?t thanh toán';
        }
      },
      error: (error) => {
        this.isProcessing = false;
        this.errorMessage = error.message || '?ã x?y ra l?i khi t?o thanh toán';
        console.error('Payment creation error:', error);
      }
    });
  }

  /**
   * Handle payment method selection
   */
  selectPaymentMethod(bankCode: string): void {
    this.selectedBankCode = bankCode;
  }

  /**
   * Cancel payment and go back
   */
  onCancel(): void {
    this.router.navigate(['/bookings']);
  }
}
```

### File: `src/app/features/payment/components/payment-form/payment-form.component.html`

```html
<div class="payment-form-container">
  <div class="payment-card">
    <div class="payment-header">
      <h2>Thanh Toán</h2>
      <p class="subtitle">Ch?n ph??ng th?c thanh toán</p>
    </div>

    <div class="payment-body">
      <!-- Amount Display -->
      <div class="amount-display">
        <span class="label">S? ti?n c?n thanh toán</span>
        <span class="amount">{{ amount | number:'1.0-0' }} VN?</span>
      </div>

      <!-- Payment Method Selection -->
      <div class="payment-methods">
        <h3>Ch?n ph??ng th?c</h3>
        <div class="method-grid">
          <div 
            *ngFor="let method of paymentMethods"
            class="method-card"
            [class.selected]="selectedBankCode === method.code"
            (click)="selectPaymentMethod(method.code)">
            <span class="material-icons">{{ method.icon }}</span>
            <span class="method-name">{{ method.name }}</span>
            <span class="checkmark material-icons">check_circle</span>
          </div>
        </div>
      </div>

      <!-- Error Message -->
      <div *ngIf="errorMessage" class="error-alert">
        <span class="material-icons">error</span>
        <span>{{ errorMessage }}</span>
      </div>

      <!-- Action Buttons -->
      <div class="button-group">
        <button 
          type="button"
          class="btn btn-primary"
          [disabled]="isProcessing || !selectedBankCode"
          (click)="onSubmit()">
          <span *ngIf="!isProcessing" class="material-icons">lock</span>
          <span *ngIf="isProcessing" class="material-icons spinning">hourglass_empty</span>
          {{ isProcessing ? '?ang x? lý...' : 'Thanh Toán' }}
        </button>
        <button 
          type="button"
          class="btn btn-secondary"
          [disabled]="isProcessing"
          (click)="onCancel()">
          <span class="material-icons">arrow_back</span>
          Quay L?i
        </button>
      </div>

      <!-- Security Notice -->
      <div class="security-notice">
        <span class="material-icons">info</span>
        <p>B?n s? ???c chuy?n ??n c?ng thanh toán VNPay ?? hoàn t?t giao d?ch.</p>
      </div>
    </div>
  </div>
</div>
```

---

## ?? Handle Payment Result

### File: `src/app/features/payment/pages/payment-callback/payment-callback.component.ts`

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../../../core/services/payment.service';
import { VnPayReturnData } from '../../models/payment.models';

@Component({
  selector: 'app-payment-callback',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-callback.component.html',
  styleUrls: ['./payment-callback.component.scss']
})
export class PaymentCallbackComponent implements OnInit {
  isLoading = true;
  paymentResult: VnPayReturnData | null = null;
  errorMessage: string | null = null;
  statusMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    // Get query parameters from VNPay
    this.route.queryParams.subscribe(params => {
      if (Object.keys(params).length === 0) {
        this.errorMessage = 'Không tìm th?y thông tin thanh toán';
        this.isLoading = false;
        return;
      }

      this.processPaymentReturn(params);
    });
  }

  /**
   * Process payment return from VNPay
   */
  private processPaymentReturn(queryParams: any): void {
    this.paymentService.processPaymentReturn(queryParams).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.paymentResult = response.data;
        
        if (response.data.isSuccess && response.data.isValidSignature) {
          this.statusMessage = 'Thanh toán thành công!';
          console.log('Payment successful:', response.data);
          
          // Clear stored payment info
          this.paymentService.clearStoredPaymentInfo();
        } else {
          this.statusMessage = this.paymentService.getPaymentStatusMessage(
            response.data.responseCode
          );
          console.warn('Payment failed or invalid:', response.data);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Không th? xác minh thanh toán';
        console.error('Payment verification error:', error);
      }
    });
  }

  /**
   * Navigate to booking details
   */
  goToBooking(): void {
    if (this.paymentResult?.orderId) {
      this.router.navigate(['/bookings', this.paymentResult.orderId]);
    } else {
      this.router.navigate(['/bookings']);
    }
  }

  /**
   * Try payment again
   */
  retryPayment(): void {
    if (this.paymentResult?.orderId) {
      this.router.navigate(['/payment'], {
        queryParams: { bookingId: this.paymentResult.orderId }
      });
    } else {
      this.router.navigate(['/bookings']);
    }
  }
}
```

### File: `src/app/features/payment/pages/payment-callback/payment-callback.component.html`

```html
<div class="payment-callback-container">
  <!-- Loading State -->
  <div *ngIf="isLoading" class="loading-card">
    <div class="spinner"></div>
    <p>?ang xác minh thanh toán...</p>
  </div>

  <!-- Error State -->
  <div *ngIf="!isLoading && errorMessage" class="result-card error">
    <span class="material-icons status-icon">error</span>
    <h2>L?i Xác Minh</h2>
    <p class="message">{{ errorMessage }}</p>
    <button class="btn btn-primary" (click)="goToBooking()">
      <span class="material-icons">arrow_back</span>
      Quay V? Danh Sách ??t Ch?
    </button>
  </div>

  <!-- Success State -->
  <div *ngIf="!isLoading && paymentResult?.isSuccess && paymentResult?.isValidSignature" 
       class="result-card success">
    <span class="material-icons status-icon">check_circle</span>
    <h2>Thanh Toán Thành Công!</h2>
    <p class="message">{{ statusMessage }}</p>

    <div class="payment-details">
      <div class="detail-row">
        <span class="label">Mã ??t ch?:</span>
        <span class="value">{{ paymentResult.orderId }}</span>
      </div>
      <div class="detail-row">
        <span class="label">Mã giao d?ch:</span>
        <span class="value">{{ paymentResult.transactionId }}</span>
      </div>
      <div class="detail-row">
        <span class="label">S? ti?n:</span>
        <span class="value">{{ paymentResult.amount | number:'1.0-0' }} VN?</span>
      </div>
      <div class="detail-row">
        <span class="label">Ngân hàng:</span>
        <span class="value">{{ paymentResult.bankCode }}</span>
      </div>
      <div class="detail-row">
        <span class="label">Th?i gian:</span>
        <span class="value">{{ paymentResult.payDate | date:'dd/MM/yyyy HH:mm:ss' }}</span>
      </div>
    </div>

    <div class="info-box">
      <span class="material-icons">info</span>
      <p>B?n có th? xem chi ti?t ??t ch? trong trang qu?n lý c?a mình.</p>
    </div>

    <button class="btn btn-primary" (click)="goToBooking()">
      <span class="material-icons">visibility</span>
      Xem Chi Ti?t ??t Ch?
    </button>
  </div>

  <!-- Failed State -->
  <div *ngIf="!isLoading && paymentResult && !paymentResult.isSuccess" 
       class="result-card failed">
    <span class="material-icons status-icon">cancel</span>
    <h2>Thanh Toán Th?t B?i</h2>
    <p class="message">{{ statusMessage }}</p>

    <div class="payment-details">
      <div class="detail-row">
        <span class="label">Mã ??t ch?:</span>
        <span class="value">{{ paymentResult.orderId }}</span>
      </div>
      <div class="detail-row">
        <span class="label">Mã l?i:</span>
        <span class="value error-code">{{ paymentResult.responseCode }}</span>
      </div>
    </div>

    <div class="button-group">
      <button class="btn btn-primary" (click)="retryPayment()">
        <span class="material-icons">refresh</span>
        Th? L?i
      </button>
      <button class="btn btn-secondary" (click)="goToBooking()">
        <span class="material-icons">arrow_back</span>
        Quay L?i
      </button>
    </div>
  </div>
</div>
```

---

## ??? Error Handling

### File: `src/app/core/interceptors/http-error.interceptor.ts`

```typescript
import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An error occurred';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Server-side error
          if (error.status === 401) {
            errorMessage = 'Unauthorized. Please login again.';
            // Trigger logout or redirect to login
          } else if (error.status === 403) {
            errorMessage = 'Access forbidden';
          } else if (error.status === 404) {
            errorMessage = 'Resource not found';
          } else if (error.status >= 500) {
            errorMessage = 'Server error. Please try again later.';
          } else if (error.error?.message) {
            errorMessage = error.error.message;
          }
        }

        console.error('HTTP Error:', errorMessage, error);
        return throwError(() => new Error(errorMessage));
      })
    );
  }
}
```

### Register Interceptor in `app.config.ts`

```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { HttpErrorInterceptor } from './core/interceptors/http-error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([HttpErrorInterceptor])
    )
  ]
};
```

---

## ?? Security Considerations

### 1. **Authentication**

Always include authentication token in API requests:

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    
    if (token) {
      const cloned = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      return next.handle(cloned);
    }

    return next.handle(req);
  }
}
```

### 2. **Route Guards**

Protect payment routes:

```typescript
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Redirect to login with return URL
  router.navigate(['/login'], {
    queryParams: { returnUrl: state.url }
  });
  return false;
};
```

### 3. **HTTPS Only**

Ensure all API calls use HTTPS in production:

```typescript
// environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-secure-api.com', // HTTPS only!
  //...
};
```

### 4. **Input Validation**

Always validate user input before sending to API:

```typescript
validatePaymentAmount(amount: number): boolean {
  return amount > 0 && amount <= 1000000000; // Max 1 billion VND
}

validateBookingId(bookingId: string): boolean {
  const guidRegex = /^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$/;
  return guidRegex.test(bookingId);
}
```

---

## ?? Testing

### Unit Test Example

```typescript
// payment.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaymentService } from './payment.service';
import { environment } from '../../../environments/environment';

describe('PaymentService', () => {
  let service: PaymentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PaymentService]
    });

    service = TestBed.inject(PaymentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create payment successfully', () => {
    const mockRequest = {
      orderId: 'test-booking-id',
      amount: 500000,
      orderDescription: 'Test payment',
      locale: 'vn'
    };

    const mockResponse = {
      success: true,
      message: 'Payment URL created successfully',
      data: {
        paymentUrl: 'https://sandbox.vnpayment.vn/...',
        orderId: 'test-booking-id',
        amount: 500000,
        createdAt: '2024-01-01T00:00:00Z'
      }
    };

    service.createPayment(mockRequest).subscribe(response => {
      expect(response.success).toBe(true);
      expect(response.data.paymentUrl).toBeTruthy();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/payment/create`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should handle payment creation error', () => {
    const mockRequest = {
      orderId: 'test-booking-id',
      amount: 500000,
      orderDescription: 'Test payment',
      locale: 'vn'
    };

    service.createPayment(mockRequest).subscribe(
      () => fail('should have failed'),
      (error) => {
        expect(error).toBeTruthy();
      }
    );

    const req = httpMock.expectOne(`${environment.apiUrl}/api/payment/create`);
    req.flush('Payment creation failed', { status: 400, statusText: 'Bad Request' });
  });
});
```

---

## ?? Complete Examples

### Example 1: Complete Payment Page Component

```typescript
// payment-page.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentFormComponent } from '../../components/payment-form/payment-form.component';

@Component({
  selector: 'app-payment-page',
  standalone: true,
  imports: [CommonModule, PaymentFormComponent],
  template: `
    <div class="payment-page">
      <app-payment-form
        [bookingId]="bookingId"
        [amount]="amount"
        [description]="description">
      </app-payment-form>
    </div>
  `
})
export class PaymentPageComponent implements OnInit {
  bookingId: string = '';
  amount: number = 0;
  description: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get booking info from query params or route params
    this.route.queryParams.subscribe(params => {
      this.bookingId = params['bookingId'];
      this.amount = Number(params['amount']);
      this.description = params['description'] || '';

      if (!this.bookingId || !this.amount) {
        // Redirect if missing required params
        this.router.navigate(['/bookings']);
      }
    });
  }
}
```

### Example 2: Routing Configuration

```typescript
// app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'payment',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => 
          import('./features/payment/pages/payment-page/payment-page.component')
            .then(m => m.PaymentPageComponent)
      },
      {
        path: 'callback',
        loadComponent: () => 
          import('./features/payment/pages/payment-callback/payment-callback.component')
            .then(m => m.PaymentCallbackComponent)
      }
    ]
  },
  // ... other routes
];
```

### Example 3: Complete Usage in Booking Component

```typescript
// booking-detail.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { BookingService } from '../../services/booking.service';

@Component({
  selector: 'app-booking-detail',
  template: `
    <div class="booking-detail">
      <h2>Chi Ti?t ??t Ch?</h2>
      
      <!-- Booking info display -->
      <div class="booking-info">
        <p>Mã ??t ch?: {{ booking.id }}</p>
        <p>S? ti?n ??t c?c: {{ depositAmount | number:'1.0-0' }} VN?</p>
      </div>

      <!-- Payment button -->
      <button 
        class="btn btn-primary"
        (click)="proceedToPayment()"
        [disabled]="booking.isPaid">
        <span class="material-icons">payment</span>
        {{ booking.isPaid ? '?ã Thanh Toán' : 'Thanh Toán Ngay' }}
      </button>
    </div>
  `
})
export class BookingDetailComponent {
  booking: any = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    isPaid: false
  };
  depositAmount = 500000;

  constructor(private router: Router) {}

  proceedToPayment(): void {
    this.router.navigate(['/payment'], {
      queryParams: {
        bookingId: this.booking.id,
        amount: this.depositAmount,
        description: `Deposit payment for booking ${this.booking.id}`
      }
    });
  }
}
```

---

## ?? Summary Checklist

Before deploying to production:

### Frontend
- [ ] Environment variables configured correctly
- [ ] HTTPS URLs in production environment
- [ ] Authentication interceptor configured
- [ ] Error handling implemented
- [ ] Loading states displayed to user
- [ ] Payment timeout handling (15 minutes)
- [ ] Session storage cleanup implemented

### Backend
- [ ] VNPay credentials configured
- [ ] Return URL and IPN URL are public and accessible
- [ ] HTTPS enabled
- [ ] Authentication/authorization implemented
- [ ] Payment status updates via IPN (not Return URL)
- [ ] Idempotent IPN handling
- [ ] Comprehensive logging

### Testing
- [ ] Test successful payment flow
- [ ] Test failed payment flow
- [ ] Test payment cancellation
- [ ] Test timeout scenarios
- [ ] Test network error handling
- [ ] Test with VNPay sandbox

### Security
- [ ] Authentication tokens included in requests
- [ ] Input validation on frontend and backend
- [ ] HTTPS only in production
- [ ] No sensitive data in URLs or logs
- [ ] CORS configured correctly

---

## ?? Troubleshooting

### Common Issues

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| CORS error | Backend not configured for frontend origin | Add frontend URL to CORS allowed origins |
| 401 Unauthorized | Missing or expired token | Check auth interceptor, refresh token logic |
| Payment URL not working | Wrong VNPay credentials | Verify TmnCode and HashSecret in backend |
| IPN not received | IPN URL not accessible | Ensure IPN URL is public, check firewall |
| Invalid signature | Wrong hash secret or parameter order | Verify backend hash secret matches VNPay |

### Debug Checklist

1. Check browser console for errors
2. Verify API endpoint URLs in environment files
3. Check network tab for API responses
4. Verify authentication token is included
5. Check backend logs for errors
6. Test with VNPay sandbox first

---

## ?? Additional Resources

- **VNPay Documentation**: Contact VNPay support for API docs
- **VNPay Sandbox**: https://sandbox.vnpayment.vn
- **Angular HttpClient**: https://angular.io/guide/http
- **RxJS**: https://rxjs.dev

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Angular Version**: 20  
**API Version**: VNPay 2.1.0
