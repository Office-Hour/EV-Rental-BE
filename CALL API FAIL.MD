curl 'https://swd-be-webapi-production.politerock-c1f6f715.japaneast.azurecontainerapps.io/api/Staff/bookings' \
  -H 'accept: application/json' \
  -H 'accept-language: en-US,en;q=0.9,vi;q=0.8' \
  -H 'authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijc2OTFlMGRjLTNmMDEtNDIzNS1iMTE2LTNhNzQ2ZTFlODQ1ZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdGFmZjFAZXYubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzdGFmZjFAZXYubG9jYWwiLCJTdGFmZiI6IlRydWUiLCJTdGFmZklkIjoiYTQ1Njc4ZjItYTliZC00ZTI3LTg3N2QtM2U3M2M0NGYyY2JiIiwibmJmIjoxNzYyNDc0MzI2LCJleHAiOjE3NjI0NzQ5MjZ9.mdneBOu3TzXrG2jq6c3fPCYvUTGC_Kq-278yFaQK7Qg' \
  -H 'origin: http://localhost:4200' \
  -H 'priority: u=1, i' \
  -H 'referer: http://localhost:4200/' \
  -H 'sec-ch-ua: "Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"' \
  -H 'sec-ch-ua-mobile: ?0' \
  -H 'sec-ch-ua-platform: "Windows"' \
  -H 'sec-fetch-dest: empty' \
  -H 'sec-fetch-mode: cors' \
  -H 'sec-fetch-site: cross-site' \
  -H 'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36'


{
    "message": "An error occurred while retrieving bookings",
    "errorDetails": [
        {
            "errorMessage": "Unable to resolve service for type 'FluentValidation.IValidator`1[Application.UseCases.BookingManagement.Queries.GetBookingFull.GetBookingFullQuery]' while attempting to activate 'WebAPI.Behaviors.ValidationBehavior`2[Application.UseCases.BookingManagement.Queries.GetBookingFull.GetBookingFullQuery,System.Collections.Generic.List`1[Application.DTOs.BookingManagement.BookingDetailsDto]]'.",
            "errorCode": null,
            "errorField": null
        }
    ]
}

curl 'https://swd-be-webapi-production.politerock-c1f6f715.japaneast.azurecontainerapps.io/api/Staff/rentals' \
  -H 'accept: application/json' \
  -H 'accept-language: en-US,en;q=0.9,vi;q=0.8' \
  -H 'authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijc2OTFlMGRjLTNmMDEtNDIzNS1iMTE2LTNhNzQ2ZTFlODQ1ZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdGFmZjFAZXYubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzdGFmZjFAZXYubG9jYWwiLCJTdGFmZiI6IlRydWUiLCJTdGFmZklkIjoiYTQ1Njc4ZjItYTliZC00ZTI3LTg3N2QtM2U3M2M0NGYyY2JiIiwibmJmIjoxNzYyNDc0MzI2LCJleHAiOjE3NjI0NzQ5MjZ9.mdneBOu3TzXrG2jq6c3fPCYvUTGC_Kq-278yFaQK7Qg' \
  -H 'origin: http://localhost:4200' \
  -H 'priority: u=1, i' \
  -H 'referer: http://localhost:4200/' \
  -H 'sec-ch-ua: "Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"' \
  -H 'sec-ch-ua-mobile: ?0' \
  -H 'sec-ch-ua-platform: "Windows"' \
  -H 'sec-fetch-dest: empty' \
  -H 'sec-fetch-mode: cors' \
  -H 'sec-fetch-site: cross-site' \
  -H 'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36'

{
    "message": "An error occurred while retrieving rentals",
    "errorDetails": [
        {
            "errorMessage": "Unable to resolve service for type 'FluentValidation.IValidator`1[Application.UseCases.RentalManagement.Queries.GetRentalFull.GetRentalFullQuery]' while attempting to activate 'WebAPI.Behaviors.ValidationBehavior`2[Application.UseCases.RentalManagement.Queries.GetRentalFull.GetRentalFullQuery,System.Collections.Generic.List`1[Application.DTOs.RentalManagement.RentalDetailsDto]]'.",
            "errorCode": null,
            "errorField": null
        }
    ]
}

curl 'https://swd-be-webapi-production.politerock-c1f6f715.japaneast.azurecontainerapps.io/api/Staff/renters' \
  -H 'accept: application/json' \
  -H 'accept-language: en-US,en;q=0.9,vi;q=0.8' \
  -H 'authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijc2OTFlMGRjLTNmMDEtNDIzNS1iMTE2LTNhNzQ2ZTFlODQ1ZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdGFmZjFAZXYubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzdGFmZjFAZXYubG9jYWwiLCJTdGFmZiI6IlRydWUiLCJTdGFmZklkIjoiYTQ1Njc4ZjItYTliZC00ZTI3LTg3N2QtM2U3M2M0NGYyY2JiIiwibmJmIjoxNzYyNDc0MzI2LCJleHAiOjE3NjI0NzQ5MjZ9.mdneBOu3TzXrG2jq6c3fPCYvUTGC_Kq-278yFaQK7Qg' \
  -H 'origin: http://localhost:4200' \
  -H 'priority: u=1, i' \
  -H 'referer: http://localhost:4200/' \
  -H 'sec-ch-ua: "Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"' \
  -H 'sec-ch-ua-mobile: ?0' \
  -H 'sec-ch-ua-platform: "Windows"' \
  -H 'sec-fetch-dest: empty' \
  -H 'sec-fetch-mode: cors' \
  -H 'sec-fetch-site: cross-site' \
  -H 'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36'
{
    "message": "An error occurred while retrieving renters",
    "errorDetails": [
        {
            "errorMessage": "Unable to resolve service for type 'FluentValidation.IValidator`1[Application.UseCases.BookingManagement.Queries.GetRenterFull.GetRenterFullQuery]' while attempting to activate 'WebAPI.Behaviors.ValidationBehavior`2[Application.UseCases.BookingManagement.Queries.GetRenterFull.GetRenterFullQuery,System.Collections.Generic.List`1[Application.DTOs.Profile.RenterProfileDto]]'.",
            "errorCode": null,
            "errorField": null
        }
    ]
}