namespace Domain.Enums;
public enum UserRole { Unknown, Renter, Staff, Admin }
public enum BookingStatus { Pending_Verification, Verified, Cancelled, Rental_Created }
public enum BookingVerificationStatus { Pending, Approved, Rejected_Mismatch, Rejected_Other }
public enum FeeType { Deposit, Rental_Charge, Surcharge, Damage, Other }
public enum PaymentStatus { Unpaid, Paid, Refunded, Failed }
public enum PaymentMethod { Unknown, Cash, Card, Ewallet, Bank_Transfer, VNPay_QR, International_Card }
public enum RentalStatus { Reserved, In_Progress, Completed, Late, Cancelled }
public enum InspectionType { Pre_Rental, Post_Rental }
public enum VehicleAtStationStatus { Maintenance, Available, Booked }
public enum ContractStatus { Issued, Partially_Signed, Signed, Voided, Expired }
public enum EsignProvider { Native, Docusign, Adobesign, Signnow, Other }
public enum PartyRole { Renter, Staff, Other }
public enum SignatureEvent { Pickup, Dropoff }
public enum SignatureType { Drawn, Typed, Digital_Cert, OnPaper }
public enum TransferStatus { Draft, Approved, In_Transit, Completed, Cancelled }
public enum KycType { National_ID, Driver_License, Passport, Other }
public enum KycStatus { Submitted, Verified, Rejected, Expired }
public enum Currency { USD, VND }