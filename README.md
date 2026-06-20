# GIBS.Module.GiftCert

An [Oqtane](https://www.oqtane.org/) module for selling and managing gift certificates. Customers enter gift certificate details, pay via PayPal, and the module can generate a PDF certificate and send email notifications.

-	 **Oqtane Framework:** 10.2.1
-	 **Target Framework:** .NET 10 (`net10.0`)
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

#### 📋 Step-by-Step Owner Instructions

Site owners should follow these steps to generate and input their credentials:

1. **Access Portal:** Open the [PayPal Developer Dashboard](https://developer.paypal.com/) and sign in with a PayPal Business account.
2. **Open Credentials:** Click on **My Apps & Credentials** from the left-hand navigation menu.
3. **Select Environment:** Click the **Sandbox** tab first to set up a testing environment.
4. **Create App:** Click the **Create App** button under the REST API apps section.
5. **Name App:** Name the application (e.g., "Oqtane Gift Certificates") and save.
6. **Copy Keys:** Copy the **Client ID**, click **Show** under Secret, and copy the **Secret Key**.
7. **Repeat for Live:** Toggle the dashboard switch to **Live** and repeat the steps for real transactions.
8. **Update Oqtane:** Paste both sets of keys into your Oqtane module settings panel.

#### ⚙️ Recommended Module Settings UI

To ensure a smooth user experience, your Oqtane module settings component should include these specific fields:

- **Mode Switch:** A dropdown or toggle to switch the module between **Sandbox** and **Live** modes.
- **Sandbox Pair:** Two distinct text inputs labeled:
  - `PayPalSandboxClientId`
  - `PayPalSandboxClientSecret`
- **Live Pair:** Two distinct text inputs labeled:
  - `OAuthClientId` (Live Client ID)
  - `OAuthClientSecret` (Live Secret Key)
- **Payee Pair:** Two text inputs for:
  - `PayPalSandboxPayee` (Sandbox)
  - `PayPalPayee` (Live)
- **Secure Storage:** Ensure your module saves these keys securely using Oqtane's encrypted site settings API.

#### PayPal Configuration Settings

Settings are stored with the following keys:
- `PayPalSandboxMode` (true/false) - Toggle between Sandbox and Production
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
