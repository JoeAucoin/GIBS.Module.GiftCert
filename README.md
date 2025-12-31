# GIBS.Module.GiftCert

An [Oqtane](https://www.oqtane.org/) module for selling and managing gift certificates. Customers enter gift certificate details, pay via PayPal, and the module can generate a PDF certificate and send email notifications.

- **Oqtane Framework:** 6.2.1
- **Target Framework:** .NET 9 (`net9.0`)
- **Module Name:** `GiftCert`
- **Package Id:** `GIBS.Module.GiftCert`

## Features

- Customer gift certificate purchase flow (amount, recipient, sender, notes)
- PayPal Checkout (Sandbox and Production)
- Persists purchases and PayPal capture response details
- Admin management UI:
  - View payment status
  - Mark certificates as processed
  - Generate/download PDF after payment is `PAID`
- Email notifications (HTML) using MailKit/MimeKit
- PDF generation using PDFsharp/MigraDoc

## Project Structure

- `Client/`  
  Blazor UI components (`Index.razor`, `List.razor`, `Edit.razor`, `Settings.razor`) and client-side services.
- `Server/`  
  Controllers, services, repository/data access, PDF + email implementation.
- `Shared/`  
  Shared models and DTOs used by client and server.
- `Package/`  
  NuGet packaging assets including `GIBS.Module.GiftCert.nuspec`.

## Installation

1. Build the module in **Release** configuration.
2. Create the `.nupkg` from the `Package` project/build step.
3. Install the package into your Oqtane site using the Oqtane module installation process.

> Note: This project uses a `.nuspec` and explicitly includes third-party DLLs in the package.

## Configuration (Module Settings)

Settings UI: `Client/Modules/GIBS.Module.GiftCert/Settings.razor`

### PayPal
- `PayPalSandboxMode` (true/false)
- Sandbox credentials:
  - `PayPalSandboxPayee`
  - `PayPalSandboxClientId`
  - `PayPalSandboxClientSecret`
- Production credentials:
  - `PayPalPayee`
  - `OAuthClientId`
  - `OAuthClientSecret`

### General
- `DefaultValue` (default certificate amount)
- `ModuleInstructions`
- `NumPerPage`
- `FileFolder` (PDF output folder)

### PDF Settings
- `CertBannerText`
- `CertFooterText`
- `CertWatermark`
- `CertLogo`
- `CertReturnAddress`

### Email Settings
- `EmailReplyTo`
- `EmailNotify`
- `EmailBCC`
- `EmailSubject`
- `SpecialInstructions`

## Key Endpoints

Controller: `Server/Controllers/GiftCertController.cs`

- `GET /api/[...]/GiftCert?moduleid={moduleId}` - list certificates
- `GET /api/[...]/GiftCert/{id}/{moduleid}` - get certificate
- `POST /api/[...]/GiftCert` - create certificate
- `PUT /api/[...]/GiftCert/{id}` - update certificate
- `DELETE /api/[...]/GiftCert/{id}/{moduleid}` - delete certificate
- `POST /api/[...]/GiftCert/SendEmail` - send email (authorized)
- `GET /api/[...]/GiftCert/pdf/{id}/{moduleid}` - download generated PDF

## Production Deployment Notes (Important)

Oqtane performs assembly scanning during startup. If any module assembly has missing dependencies, the site can fail to start with **HTTP 500.30**.

If you see errors like:

- `ReflectionTypeLoadException`
- `Could not load file or assembly 'APIMatic.Core'`

then your package is missing transitive dependencies. Ensure that required third-party DLLs (for example, `APIMatic.Core.dll` required by `PayPalServerSDK`) are included in the `.nupkg` under `lib/net9.0`.

Packaging file: `Package/GIBS.Module.GiftCert.nuspec`

## Development

- Open the solution in Visual Studio.
- Ensure you are targeting the same Oqtane version as production (`6.2.1`).
- Build in `Release` before generating the installation package.

## License

MIT (see `Package/GIBS.Module.GiftCert.nuspec` for the package license expression).
